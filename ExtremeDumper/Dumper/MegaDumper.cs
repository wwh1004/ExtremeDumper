using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NativeSharp;

namespace ExtremeDumper.Dumper {
	public sealed class MegaDumper : IDumper {
		private uint _processId;

		private bool _is64;

		private MegaDumper() {
		}

		public static IDumper Create(uint processId) {
			using (NativeProcess process = NativeProcess.Open(processId, ProcessAccess.QueryInformation))
				return new MegaDumper {
					_processId = processId,
					_is64 = process.Is64Bit
				};
		}

		public bool DumpModule(IntPtr moduleHandle, string filePath) {
			return _is64 ? MegaDumperPrivate64.DumpModule(_processId, filePath, (long)moduleHandle) : MegaDumperPrivate32.DumpModule(_processId, filePath, (int)moduleHandle);
		}

		public int DumpProcess(string directoryPath) {
			return _is64 ? MegaDumperPrivate64.DumpProcess(_processId, directoryPath) : MegaDumperPrivate32.DumpProcess(_processId, directoryPath);
		}

		public void Dispose() {
		}

		#region private class
		private static class MegaDumperPrivate32 {
			[DllImport("Kernel32.dll")]
			private static extern bool ReadProcessMemory
			(
				IntPtr hProcess,
				IntPtr lpBaseAddress,
				byte[] lpBuffer,
				uint nSize,
				ref uint lpNumberOfBytesRead
			);

			[DllImport("Kernel32.dll")]
			private static extern bool ReadProcessMemory
			(
				IntPtr hProcess,
				uint lpBaseAddress,
				byte[] lpBuffer,
				uint nSize,
				ref uint lpNumberOfBytesRead
			);

			[DllImport("kernel32.dll")]
			static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

			[DllImport("kernel32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			private static extern bool CloseHandle(IntPtr hObject);

			[StructLayout(LayoutKind.Sequential)]
			private struct SYSTEM_INFO {
				public uint dwOemId;
				public uint dwPageSize;
				public uint lpMinimumApplicationAddress;
				public uint lpMaximumApplicationAddress;
				public uint dwActiveProcessorMask;
				public uint dwNumberOfProcessors;
				public uint dwProcessorType;
				public uint dwAllocationGranularity;
				public uint dwProcessorLevel;
				public uint dwProcessorRevision;
			}

			[DllImport("kernel32")]
			private static extern void GetSystemInfo(ref SYSTEM_INFO pSI);

			private static readonly uint minaddress;

			private static readonly uint maxaddress;

			private static readonly uint pagesize;

			static MegaDumperPrivate32() {
				SYSTEM_INFO pSI = new SYSTEM_INFO();
				GetSystemInfo(ref pSI);
				minaddress = pSI.lpMinimumApplicationAddress;
				maxaddress = pSI.lpMaximumApplicationAddress;
				pagesize = pSI.dwPageSize;
			}

			private static int RVA2Offset(byte[] input, int rva) {
				int PEOffset = BitConverter.ToInt32(input, 0x3C);
				int nrofsection = BitConverter.ToInt16(input, PEOffset + 0x06);

				for (int i = 0; i < nrofsection; i++) {
					int virtualAddress = BitConverter.ToInt32(input, PEOffset + 0x0F8 + (0x28 * i) + 012);
					int fvirtualsize = BitConverter.ToInt32(input, PEOffset + 0x0F8 + (0x28 * i) + 08);
					int frawAddress = BitConverter.ToInt32(input, PEOffset + (0x28 * i) + 0x0F8 + 20);
					if ((virtualAddress <= rva) && (virtualAddress + fvirtualsize >= rva))
						return frawAddress + (rva - virtualAddress);
				}

				return -1;
			}


			private static int Offset2RVA(byte[] input, int offset) {
				int PEOffset = BitConverter.ToInt32(input, 0x3C);
				int nrofsection = BitConverter.ToInt16(input, PEOffset + 0x06);

				for (int i = 0; i < nrofsection; i++) {
					int virtualAddress = BitConverter.ToInt32(input, PEOffset + 0x0F8 + (0x28 * i) + 012);
					int frawAddress = BitConverter.ToInt32(input, PEOffset + (0x28 * i) + 0x0F8 + 20);
					int frawsize = BitConverter.ToInt32(input, PEOffset + (0x28 * i) + 0x0F8 + 16);
					if ((frawAddress <= offset) && (frawAddress + frawsize >= offset))
						return virtualAddress + (offset - frawAddress);
				}

				return -1;
			}

#pragma warning disable 0649
			private unsafe struct IMAGE_SECTION_HEADER {
				public fixed byte name[8];
				public int virtual_size;
				public int virtual_address;
				public int size_of_raw_data;
				public int pointer_to_raw_data;
				public int pointer_to_relocations;
				public int pointer_to_linenumbers;
				public short number_of_relocations;
				public short number_of_linenumbers;
				public int characteristics;
			};

			private struct IMAGE_FILE_HEADER {
				public short Machine;
				public short NumberOfSections;
				public int TimeDateStamp;
				public int PointerToSymbolTable;
				public int NumberOfSymbols;
				public short SizeOfOptionalHeader;
				public short Characteristics;
			}
#pragma warning restore 0649

			private static bool FixImportandEntryPoint(int dumpVA, byte[] Dump) {
				if (Dump == null || Dump.Length == 0) return false;

				int PEOffset = BitConverter.ToInt32(Dump, 0x3C);

				int ImportDirectoryRva = BitConverter.ToInt32(Dump, PEOffset + 0x080);
				int impdiroffset = RVA2Offset(Dump, ImportDirectoryRva);
				if (impdiroffset == -1) return false;

				byte[] mscoreeAscii = { 0x6D, 0x73, 0x63, 0x6F, 0x72, 0x65, 0x65, 0x2E, 0x64, 0x6C, 0x6C, 0x00 };
				byte[] CorExeMain = { 0x5F, 0x43, 0x6F, 0x72, 0x45, 0x78, 0x65, 0x4D, 0x61, 0x69, 0x6E, 0x00 };
				byte[] CorDllMain = { 0x5F, 0x43, 0x6F, 0x72, 0x44, 0x6C, 0x6C, 0x4D, 0x61, 0x69, 0x6E, 0x00 };
				int ThunkToFix = 0;
				int ThunkData = 0;
				int current = 0;
				int NameRVA = BitConverter.ToInt32(Dump, impdiroffset + current + 12);
				while (NameRVA > 0) {
					int NameOffset = RVA2Offset(Dump, NameRVA);
					if (NameOffset > 0) {
						try {
							bool ismscoree = true;
							for (int i = 0; i < mscoreeAscii.Length; i++) {
								if (Dump[NameOffset + i] != mscoreeAscii[i]) {
									ismscoree = false;
									break;
								}
							}

							if (ismscoree) {
								int OriginalFirstThunk = BitConverter.ToInt32(Dump, impdiroffset + current);
								int OriginalFirstThunkfo = RVA2Offset(Dump, OriginalFirstThunk);
								if (OriginalFirstThunkfo > 0) {
									ThunkData = BitConverter.ToInt32(Dump, OriginalFirstThunkfo);
									int ThunkDatafo = RVA2Offset(Dump, ThunkData);
									if (ThunkDatafo > 0) {
										ismscoree = true;
										for (int i = 0; i < mscoreeAscii.Length; i++) {
											if (Dump[ThunkDatafo + 2 + i] != CorExeMain[i] && Dump[ThunkDatafo + 2 + i] != CorDllMain[i]) {
												ismscoree = false;
												break;
											}
										}

										if (ismscoree) {
											ThunkToFix = BitConverter.ToInt32(Dump, impdiroffset + current + 16);  // FirstThunk;
											break;
										}

									}
								}

							}
						}
						catch {
						}

					}

					try {
						current += 20; // 20 = size of IMAGE_IMPORT_DESCRIPTOR
						NameRVA = BitConverter.ToInt32(Dump, ImportDirectoryRva + current + 12);
					}
					catch {
						break;
					}
				}

				if (ThunkToFix <= 0 || ThunkData == 0) return false;

				int ThunkToFixfo = RVA2Offset(Dump, ThunkToFix);
				if (ThunkToFixfo < 0) return false;

				BinaryWriter writer = new BinaryWriter(new MemoryStream(Dump));
				int ThunkValue = BitConverter.ToInt32(Dump, ThunkToFixfo);  // old thunk value
				if (ThunkValue <= 0 || RVA2Offset(Dump, ThunkValue) < 0) {
					writer.BaseStream.Position = ThunkToFixfo;
					writer.Write(ThunkData);
				}

				int EntryPoint = BitConverter.ToInt32(Dump, PEOffset + 0x028);
				if (EntryPoint <= 0 || RVA2Offset(Dump, EntryPoint) < 0) {
					byte[] ThunkToFixbytes = BitConverter.GetBytes(ThunkToFix + dumpVA);
					for (int i = 0; i < Dump.Length - 6; i++) {
						if (Dump[i + 0] == 0x0FF && Dump[i + 1] == 0x025 && Dump[i + 2] == ThunkToFixbytes[0] && Dump[i + 3] == ThunkToFixbytes[1] && Dump[i + 4] == ThunkToFixbytes[2] && Dump[i + 5] == ThunkToFixbytes[3]) {
							int EntrPointRVA = Offset2RVA(Dump, i);
							writer.BaseStream.Position = PEOffset + 0x028;
							writer.Write(EntrPointRVA);
							break;
						}
					}
				}

				writer.Close();
				return true;
			}

			private static bool BytesEqual(byte[] Array1, byte[] Array2) {
				if (Array1.Length != Array2.Length) return false;
				for (int i = 0; i < Array1.Length; i++) {
					if (Array1[i] != Array2[i]) return false;
				}
				return true;
			}

			private const uint PROCESS_VM_OPERATION = 0x0008;
			private const uint PROCESS_VM_READ = 0x0010;
			private const uint PROCESS_VM_WRITE = 0x0020;
			private const uint PROCESS_QUERY_INFORMATION = 0x0400;

			public static bool DumpModule(uint procid, string filePath, int ImageBase) {
				IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, 0, procid);

				if (hProcess != IntPtr.Zero) {
					bool isok;
					byte[] InfoKeep = new byte[8];
					uint BytesRead = 0;

					int nrofsection = 0;
					byte[] Dump = null;
					byte[] Partkeep = null;
					int filealignment = 0;
					int rawaddress;
					int offset = 0;
					bool ShouldFixrawsize = false;

					ReadProcessMemory(hProcess, (uint)(ImageBase + 0x03C), InfoKeep, 4, ref BytesRead);
					int PEOffset = BitConverter.ToInt32(InfoKeep, 0);

					try {
						isok = ReadProcessMemory(hProcess, (uint)(ImageBase + PEOffset + 0x0F8 + 20), InfoKeep, 4, ref BytesRead);
						byte[] PeHeader = new byte[pagesize];

						rawaddress = BitConverter.ToInt32(InfoKeep, 0);
						int sizetocopy = rawaddress;
						if (sizetocopy > pagesize) sizetocopy = (int)pagesize;
						isok = ReadProcessMemory(hProcess, (uint)ImageBase, PeHeader, (uint)sizetocopy, ref BytesRead);
						offset += rawaddress;

						nrofsection = BitConverter.ToInt16(PeHeader, PEOffset + 0x06);
						int sectionalignment = BitConverter.ToInt32(PeHeader, PEOffset + 0x038);
						filealignment = BitConverter.ToInt32(PeHeader, PEOffset + 0x03C);

						int sizeofimage = BitConverter.ToInt32(PeHeader, PEOffset + 0x050);

						int calculatedimagesize = BitConverter.ToInt32(PeHeader, PEOffset + 0x0F8 + 012);

						for (int i = 0; i < nrofsection; i++) {
							int virtualsize = BitConverter.ToInt32(PeHeader, PEOffset + 0x0F8 + (0x28 * i) + 08);
							int toadd = virtualsize % sectionalignment;
							if (toadd != 0) toadd = sectionalignment - toadd;
							calculatedimagesize = calculatedimagesize + virtualsize + toadd;
						}

						if (calculatedimagesize > sizeofimage) sizeofimage = calculatedimagesize;
						Dump = new byte[sizeofimage];
						Array.Copy(PeHeader, Dump, sizetocopy);
						Partkeep = new byte[sizeofimage];

					}
					catch {

					}

					for (int i = 0; i < nrofsection; i++) {

						int rawsize, virtualsize, virtualAddress;
						for (int l = 0; l < nrofsection; l++) {
							rawsize = BitConverter.ToInt32(Dump, PEOffset + 0x0F8 + (0x28 * l) + 16);
							virtualsize = BitConverter.ToInt32(Dump, PEOffset + 0x0F8 + (0x28 * l) + 08);

							// RawSize = Virtual Size rounded on FileAlligment
							int calcrawsize = virtualsize % filealignment;
							if (calcrawsize != 0) calcrawsize = filealignment - calcrawsize;
							calcrawsize = virtualsize + calcrawsize;

							if (calcrawsize != 0 && rawsize != calcrawsize && rawsize != virtualsize) {
								ShouldFixrawsize = true;
								break;
							}
						}

						rawsize = BitConverter.ToInt32(Dump, PEOffset + 0x0F8 + (0x28 * i) + 16);
						virtualsize = BitConverter.ToInt32(Dump, PEOffset + 0x0F8 + (0x28 * i) + 08);
						// RawSize = Virtual Size rounded on FileAlligment
						virtualAddress = BitConverter.ToInt32(Dump, PEOffset + 0x0F8 + (0x28 * i) + 012);


						if (ShouldFixrawsize) {
							rawsize = virtualsize;
							BinaryWriter writer = new BinaryWriter(new MemoryStream(Dump));
							writer.BaseStream.Position = PEOffset + 0x0F8 + (0x28 * i) + 16;
							writer.Write(virtualsize);
							writer.BaseStream.Position = PEOffset + 0x0F8 + (0x28 * i) + 20;
							writer.Write(virtualAddress);
							writer.Close();

						}



						int address = BitConverter.ToInt32(Dump, PEOffset + 0x0F8 + (0x28 * i) + 12);

						isok = ReadProcessMemory(hProcess, (uint)(ImageBase + address), Partkeep, (uint)rawsize, ref BytesRead);
						if (!isok) {
							byte[] onepage = new byte[512];
							for (int c = 0; c < virtualsize; c += 512) {
								ReadProcessMemory(hProcess, (uint)(ImageBase + virtualAddress + c), onepage, 512, ref BytesRead);
								Array.Copy(onepage, 0, Partkeep, c, 512);
							}
						}


						if (ShouldFixrawsize) {
							Array.Copy(Partkeep, 0, Dump, virtualAddress, rawsize);
							offset = virtualAddress + rawsize;
						}
						else {
							Array.Copy(Partkeep, 0, Dump, offset, rawsize);
							offset += rawsize;
						}
					}





					if (Dump != null && Dump.Length > 0 && Dump.Length >= offset) {
						int ImportDirectoryRva = BitConverter.ToInt32(Dump, PEOffset + 0x080);
						if (ImportDirectoryRva > 0 && ImportDirectoryRva < offset) {
							int current = 0;
							int ThunkToFix = 0;
							int ThunkData = 0;
							isok = ReadProcessMemory(hProcess, (uint)(ImageBase + ImportDirectoryRva + current + 12), Partkeep, 4, ref BytesRead);
							int NameOffset = BitConverter.ToInt32(Partkeep, 0);
							while (isok && NameOffset != 0) {
								byte[] mscoreeAscii = { 0x6D, 0x73, 0x63, 0x6F, 0x72, 0x65, 0x65, 0x2E, 0x64, 0x6C, 0x6C, 0x00 };
								byte[] NameKeeper = new byte[mscoreeAscii.Length];
								isok = ReadProcessMemory(hProcess, (uint)(ImageBase + NameOffset), NameKeeper, (uint)mscoreeAscii.Length, ref BytesRead);
								if (isok && BytesEqual(NameKeeper, mscoreeAscii)) {
									ReadProcessMemory(hProcess, (uint)(ImageBase + ImportDirectoryRva + current), Partkeep, 4, ref BytesRead);
									int OriginalFirstThunk = BitConverter.ToInt32(Partkeep, 0);  // OriginalFirstThunk;
									if (OriginalFirstThunk > 0 && OriginalFirstThunk < offset) {
										ReadProcessMemory(hProcess, (uint)(ImageBase + OriginalFirstThunk), Partkeep, 4, ref BytesRead);
										ThunkData = BitConverter.ToInt32(Partkeep, 0);
										if (ThunkData > 0 && ThunkData < offset) {
											byte[] CorExeMain = { 0x5F, 0x43, 0x6F, 0x72, 0x45, 0x78, 0x65, 0x4D, 0x61, 0x69, 0x6E, 0x00 };
											byte[] CorDllMain = { 0x5F, 0x43, 0x6F, 0x72, 0x44, 0x6C, 0x6C, 0x4D, 0x61, 0x69, 0x6E, 0x00 };
											NameKeeper = new byte[CorExeMain.Length];
											isok = ReadProcessMemory(hProcess, (uint)(ImageBase + ThunkData + 2), NameKeeper,
											(uint)CorExeMain.Length, ref BytesRead);
											if (isok && (BytesEqual(NameKeeper, CorExeMain) || BytesEqual(NameKeeper, CorDllMain))) {
												ReadProcessMemory(hProcess, (uint)(ImageBase + ImportDirectoryRva + current + 16), Partkeep, 4, ref BytesRead);
												ThunkToFix = BitConverter.ToInt32(Partkeep, 0);  // FirstThunk;
												break;
											}
										}
									}
								}

								current += 20; // 20 size of IMAGE_IMPORT_DESCRIPTOR
								isok = ReadProcessMemory(hProcess, (uint)(ImageBase + ImportDirectoryRva + current + 12), Partkeep, 4, ref BytesRead);
								NameOffset = BitConverter.ToInt32(Partkeep, 0);
							}

							if (ThunkToFix > 0 && ThunkToFix < offset) {
								BinaryWriter writer = new BinaryWriter(new MemoryStream(Dump));
								isok = ReadProcessMemory(hProcess, (uint)(ImageBase + ThunkToFix), Partkeep, 4, ref BytesRead);
								int ThunkValue = BitConverter.ToInt32(Partkeep, 0);
								if (isok && (ThunkValue < 0 || ThunkValue > offset)) {
									int fvirtualAddress = BitConverter.ToInt32(Dump, PEOffset + 0x0F8 + 012);
									int frawAddress = BitConverter.ToInt32(Dump, PEOffset + 0x0F8 + 20);
									writer.BaseStream.Position = ThunkToFix - fvirtualAddress + frawAddress;
									writer.Write(ThunkData);
								}

								int EntryPoint = BitConverter.ToInt32(Dump, PEOffset + 0x028);
								if (EntryPoint <= 0 || EntryPoint > offset) {
									int ca = 0;
									do {
										isok = ReadProcessMemory(hProcess, (uint)(ImageBase + ThunkData + ca), Partkeep, 1, ref BytesRead);
										if (isok && Partkeep[0] == 0x0FF) {
											isok = ReadProcessMemory(hProcess, (uint)(ImageBase + ThunkData + ca + 1), Partkeep, 1, ref BytesRead);
											if (isok && Partkeep[0] == 0x025) {
												isok = ReadProcessMemory(hProcess, (uint)(ImageBase + ThunkData + ca + 2), Partkeep, 4, ref BytesRead);
												if (isok) {
													int RealEntryPoint = ThunkData + ca;
													writer.BaseStream.Position = PEOffset + 0x028;
													writer.Write(RealEntryPoint);
												}

											}
										}
										ca++;
									}
									while (isok);
								}
								writer.Close();
							}

						}
					}
					if (Dump != null && Dump.Length > 0 && Dump.Length >= offset)
						using (var fout = new FileStream(filePath, FileMode.Create))
							fout.Write(Dump, 0, offset);
					else
						return false;
				}
				else
					return false;
				return true;
			}

			public static unsafe int DumpProcess(uint processId, string DirectoryName) {
				IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, 0, processId);
				if (hProcess == IntPtr.Zero)
					return 0;
				try {
					int CurrentCount = 1;
					bool isok;
					byte[] onepage = new byte[pagesize];
					uint BytesRead = 0;
					byte[] infokeep = new byte[8];
					MegaDumperHelper.CreateDirectories(DirectoryName);
					for (uint j = minaddress; j < maxaddress; j += pagesize) {

						isok = ReadProcessMemory(hProcess, j, onepage, pagesize, ref BytesRead);

						if (isok) {
							for (int k = 0; k < onepage.Length - 2; k++) {

								if (onepage[k] == 0x4D && onepage[k + 1] == 0x5A) {
									if (ReadProcessMemory(hProcess, (uint)(j + k + 0x03C), infokeep, 4, ref BytesRead)) {
										int PEOffset = BitConverter.ToInt32(infokeep, 0);
										if (PEOffset > 0 && (PEOffset + 0x0120) < pagesize) {
											if (ReadProcessMemory(hProcess, (uint)(j + k + PEOffset), infokeep, 2, ref BytesRead)) {
												if (infokeep[0] == 0x050 && infokeep[1] == 0x045) {
													long NetMetadata = 0;
													if (ReadProcessMemory(hProcess, (uint)(j + k + PEOffset + 0x0E8), infokeep, 8, ref BytesRead))
														NetMetadata = BitConverter.ToInt64(infokeep, 0);

													#region Dump Native
													if (NetMetadata != 0) {
														byte[] PeHeader = new byte[pagesize];
														if (ReadProcessMemory(hProcess, (uint)(j + k), PeHeader, pagesize, ref BytesRead)) {
															int nrofsection = BitConverter.ToInt16(PeHeader, PEOffset + 0x06);
															if (nrofsection > 0) {
																int sectionalignment = BitConverter.ToInt32(PeHeader, PEOffset + 0x038);
																int filealignment = BitConverter.ToInt32(PeHeader, PEOffset + 0x03C);
																short sizeofoptionalheader = BitConverter.ToInt16(PeHeader, PEOffset + 0x014);

																bool IsDll = false;
																if ((PeHeader[PEOffset + 0x017] & 32) != 0) IsDll = true;
																IntPtr pointer = IntPtr.Zero;
																IMAGE_SECTION_HEADER[] sections = new IMAGE_SECTION_HEADER[nrofsection];
																uint ptr = (uint)(j + k + PEOffset) + (uint)sizeofoptionalheader + 4 +
																	(uint)Marshal.SizeOf(typeof(IMAGE_FILE_HEADER));

																for (int i = 0; i < nrofsection; i++) {
																	byte[] datakeeper = new byte[Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER))];
																	ReadProcessMemory(hProcess, ptr, datakeeper, (uint)datakeeper.Length, ref BytesRead);
																	fixed (byte* p = datakeeper) {
																		pointer = (IntPtr)p;
																	}

																	sections[i] = (IMAGE_SECTION_HEADER)Marshal.PtrToStructure(pointer, typeof(IMAGE_SECTION_HEADER));
																	ptr += (uint)Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER));
																}



																// get total raw size (of all sections):
																int totalrawsize = 0;
																int rawsizeoflast = sections[nrofsection - 1].size_of_raw_data;
																int rawaddressoflast = sections[nrofsection - 1].pointer_to_raw_data;
																if (rawsizeoflast > 0 && rawaddressoflast > 0)
																	totalrawsize = rawsizeoflast + rawaddressoflast;
																string filename = "";

																// calculate right size of image
																int actualsizeofimage = BitConverter.ToInt32(PeHeader, PEOffset + 0x050);
																int sizeofimage = actualsizeofimage;
																int calculatedimagesize = BitConverter.ToInt32(PeHeader, PEOffset + 0x0F8 + 012);
																int rawsize, rawAddress, virtualsize, virtualAddress = 0;
																int calcrawsize = 0;

																for (int i = 0; i < nrofsection; i++) {
																	virtualsize = sections[i].virtual_size;
																	int toadd = virtualsize % sectionalignment;
																	if (toadd != 0) toadd = sectionalignment - toadd;
																	calculatedimagesize = calculatedimagesize + virtualsize + toadd;
																}

																if (calculatedimagesize > sizeofimage) sizeofimage = calculatedimagesize;

																try {
																	byte[] crap = new byte[totalrawsize];
																}
																catch {
																	totalrawsize = sizeofimage;
																}

																if (totalrawsize != 0) {
																	try {
																		byte[] rawdump = new byte[totalrawsize];
																		isok = ReadProcessMemory(hProcess, (uint)(j + k), rawdump, (uint)rawdump.Length, ref BytesRead);
																		if (isok) {
																			filename = Path.Combine(DirectoryName, "rawdump_" + (j + k).ToString("X8"));
																			if (File.Exists(filename))
																				filename = Path.Combine(DirectoryName, "rawdump" + CurrentCount.ToString() + "_" + (j + k).ToString("X8"));
																			filename = IsDll ? filename + ".dll" : filename + ".exe";

																			File.WriteAllBytes(filename, rawdump);
																			CurrentCount++;
																		}
																	}
																	catch {
																	}
																}



																byte[] virtualdump = new byte[sizeofimage];
																Array.Copy(PeHeader, virtualdump, pagesize);

																int rightrawsize = 0;
																for (int l = 0; l < nrofsection; l++) {
																	rawsize = sections[l].size_of_raw_data;
																	rawAddress = sections[l].pointer_to_raw_data;
																	virtualsize = sections[l].virtual_size;
																	virtualAddress = sections[l].virtual_address;

																	// RawSize = Virtual Size rounded on FileAlligment
																	calcrawsize = 0;
																	calcrawsize = virtualsize % filealignment;
																	if (calcrawsize != 0) calcrawsize = filealignment - calcrawsize;
																	calcrawsize = virtualsize + calcrawsize;

																	if (calcrawsize != 0 && rawsize != calcrawsize && rawsize != virtualsize
																	   || rawAddress < 0) {
																		// if raw size is bad:
																		rawsize = virtualsize;
																		rawAddress = virtualAddress;
																		BinaryWriter writer = new BinaryWriter(new MemoryStream(virtualdump));
																		writer.BaseStream.Position = PEOffset + 0x0F8 + (0x28 * l) + 16;
																		writer.Write(virtualsize);
																		writer.BaseStream.Position = PEOffset + 0x0F8 + (0x28 * l) + 20;
																		writer.Write(virtualAddress);
																		writer.Close();
																	}

																	byte[] csection = new byte[0];
																	try {
																		csection = new byte[rawsize];
																	}
																	catch {
																		csection = new byte[virtualsize];
																	}
																	int rightsize = csection.Length;
																	isok = ReadProcessMemory(hProcess, (uint)(j + k + virtualAddress), csection, (uint)rawsize, ref BytesRead);
																	if (!isok || BytesRead != rawsize) {
																		rightsize = 0;
																		byte[] currentpage = new byte[pagesize];
																		for (int c = 0; c < rawsize; c += (int)pagesize) {
																			// some section have a houge size so : try
																			try {
																				isok = ReadProcessMemory(hProcess, (uint)(j + k + virtualAddress + c), currentpage, pagesize, ref BytesRead);
																			}
																			catch {
																				break;
																			}

																			if (isok) {
																				rightsize += (int)pagesize;
																				for (int i = 0; i < pagesize; i++) {
																					if ((c + i) < csection.Length)
																						csection[c + i] = currentpage[i];
																				}
																			}


																		}
																	}


																	try {
																		Array.Copy(csection, 0, virtualdump, rawAddress, rightsize);
																	}
																	catch {
																	}

																	if (l == nrofsection - 1) {
																		rightrawsize = rawAddress + rawsize;
																	}

																}
																FixImportandEntryPoint((int)(j + k), virtualdump);
																filename = Path.Combine(DirectoryName, "vdump_" + (j + k).ToString("X8"));
																if (File.Exists(filename))
																	filename = Path.Combine(DirectoryName, "vdump" + CurrentCount.ToString() + "_" + (j + k).ToString("X8"));
																filename = IsDll ? filename + ".dll" : filename + ".exe";
																using (var fout = new FileStream(filename, FileMode.Create))
																	fout.Write(virtualdump, 0, Math.Min(rightrawsize, virtualdump.Length));
																CurrentCount++;
															}
														}
														// dumping end here
													}
													#endregion
												}
											}
										}
									}
								}

							}
						}


					}
					// rename files:
					foreach (FileInfo fi in new DirectoryInfo(DirectoryName).GetFiles()) {
						FileVersionInfo info = FileVersionInfo.GetVersionInfo(fi.FullName);
						string validOriginalFilename = MegaDumperHelper.EnsureValidFileName(info.OriginalFilename);
						if (validOriginalFilename != "") {
							string Newfilename = Path.Combine(DirectoryName, validOriginalFilename);
							int count = 2;
							if (File.Exists(Newfilename)) {
								string extension = Path.GetExtension(Newfilename);
								if (extension == "") extension = ".dll";
								do {
									Newfilename = Path.Combine(DirectoryName, Path.GetFileNameWithoutExtension(validOriginalFilename) + "(" + count.ToString() + ")" + extension);
									count++;
								}
								while (File.Exists(Newfilename));
							}

							File.Move(fi.FullName, Newfilename);
						}
					}
					MegaDumperHelper.Classify(DirectoryName);
					CurrentCount--;
					return CurrentCount;

				}
				finally {
					CloseHandle(hProcess);
				}
			}
		}

		private static class MegaDumperPrivate64 {
			[DllImport("kernel32.dll")]
			private static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

			[DllImport("kernel32.dll")]
			private static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

			[DllImport("kernel32.dll")]
			private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

			[DllImport("Kernel32.dll")]
			private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, ref uint lpNumberOfBytesRead);

			[DllImport("kernel32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			private static extern bool CloseHandle(IntPtr hObject);

			private static readonly ulong minAddress;

			private static readonly ulong maxAddress;

			private static readonly uint PageSize;

			static MegaDumperPrivate64() {
				minAddress = 0;
				maxAddress = long.MaxValue;
				PageSize = 0x1000;
				SYSTEM_INFO system_INFO = default;
				GetNativeSystemInfo(ref system_INFO);
				minAddress = (ulong)(long)system_INFO.lpMinimumApplicationAddress;
				maxAddress = (ulong)(long)system_INFO.lpMaximumApplicationAddress;
				PageSize = system_INFO.dwPageSize;
			}

			private static bool BytesEqual(byte[] Array1, byte[] Array2) {
				if (Array1.Length != Array2.Length) return false;
				for (int i = 0; i < Array1.Length; i++) {
					if (Array1[i] != Array2[i]) return false;
				}
				return true;
			}

			private const uint PROCESS_VM_OPERATION = 0x0008;
			private const uint PROCESS_VM_READ = 0x0010;
			private const uint PROCESS_VM_WRITE = 0x0020;
			private const uint PROCESS_QUERY_INFORMATION = 0x0400;

			public static bool DumpModule(uint procid, string filePath, long ImageBase) {
				IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, 0, procid);

				if (hProcess != IntPtr.Zero) {
					bool isok;
					byte[] InfoKeep = new byte[8];
					uint BytesRead = 0;

					int nrofsection = 0;
					byte[] Dump = null;
					byte[] Partkeep = null;
					int filealignment = 0;
					int rawaddress;
					int offset = 0;
					bool ShouldFixrawsize = false;

					ReadProcessMemory(hProcess, (IntPtr)(ImageBase + 0x03C), InfoKeep, 4, ref BytesRead);
					int PEOffset = BitConverter.ToInt32(InfoKeep, 0);

					try {
						isok = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + PEOffset + 0x0108 + 20), InfoKeep, 4, ref BytesRead);
						byte[] PeHeader = new byte[PageSize];

						rawaddress = BitConverter.ToInt32(InfoKeep, 0);
						int sizetocopy = rawaddress;
						if (sizetocopy > PageSize) sizetocopy = (int)PageSize;
						isok = ReadProcessMemory(hProcess, (IntPtr)ImageBase, PeHeader, (uint)sizetocopy, ref BytesRead);
						offset += rawaddress;

						nrofsection = BitConverter.ToInt16(PeHeader, PEOffset + 0x06);
						int sectionalignment = BitConverter.ToInt32(PeHeader, PEOffset + 0x038);
						filealignment = BitConverter.ToInt32(PeHeader, PEOffset + 0x03C);

						int sizeofimage = BitConverter.ToInt32(PeHeader, PEOffset + 0x050);

						int calculatedimagesize = BitConverter.ToInt32(PeHeader, PEOffset + 0x0108 + 012);

						for (int i = 0; i < nrofsection; i++) {
							int virtualsize = BitConverter.ToInt32(PeHeader, PEOffset + 0x0108 + (0x28 * i) + 08);
							int toadd = virtualsize % sectionalignment;
							if (toadd != 0) toadd = sectionalignment - toadd;
							calculatedimagesize = calculatedimagesize + virtualsize + toadd;
						}

						if (calculatedimagesize > sizeofimage) sizeofimage = calculatedimagesize;
						Dump = new byte[sizeofimage];
						Array.Copy(PeHeader, Dump, sizetocopy);
						Partkeep = new byte[sizeofimage];

					}
					catch {

					}

					for (int i = 0; i < nrofsection; i++) {

						int rawsize, virtualsize, virtualAddress;
						for (int l = 0; l < nrofsection; l++) {
							rawsize = BitConverter.ToInt32(Dump, PEOffset + 0x0108 + (0x28 * l) + 16);
							virtualsize = BitConverter.ToInt32(Dump, PEOffset + 0x0108 + (0x28 * l) + 08);
							_ = BitConverter.ToInt32(Dump, PEOffset + 0x0108 + (0x28 * l) + 012);

							// RawSize = Virtual Size rounded on FileAlligment
							int calcrawsize = virtualsize % filealignment;
							if (calcrawsize != 0) calcrawsize = filealignment - calcrawsize;
							calcrawsize = virtualsize + calcrawsize;

							if (calcrawsize != 0 && rawsize != calcrawsize && rawsize != virtualsize) {
								ShouldFixrawsize = true;
								break;
							}
						}

						rawsize = BitConverter.ToInt32(Dump, PEOffset + 0x0108 + (0x28 * i) + 16);
						virtualsize = BitConverter.ToInt32(Dump, PEOffset + 0x0108 + (0x28 * i) + 08);
						// RawSize = Virtual Size rounded on FileAlligment
						virtualAddress = BitConverter.ToInt32(Dump, PEOffset + 0x0108 + (0x28 * i) + 012);


						if (ShouldFixrawsize) {
							rawsize = virtualsize;
							BinaryWriter writer = new BinaryWriter(new MemoryStream(Dump));
							writer.BaseStream.Position = PEOffset + 0x0108 + (0x28 * i) + 16;
							writer.Write(virtualsize);
							writer.BaseStream.Position = PEOffset + 0x0108 + (0x28 * i) + 20;
							writer.Write(virtualAddress);
							writer.Close();

						}



						int address = BitConverter.ToInt32(Dump, PEOffset + 0x0108 + (0x28 * i) + 12);

						isok = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + address), Partkeep, (uint)rawsize, ref BytesRead);
						if (!isok) {
							byte[] onepage = new byte[512];
							for (int c = 0; c < virtualsize; c += 512) {
								_ = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + virtualAddress + c), onepage, 512, ref BytesRead);
								Array.Copy(onepage, 0, Partkeep, c, 512);
							}
						}


						if (ShouldFixrawsize) {
							Array.Copy(Partkeep, 0, Dump, virtualAddress, rawsize);
							offset = virtualAddress + rawsize;
						}
						else {
							Array.Copy(Partkeep, 0, Dump, offset, rawsize);
							offset += rawsize;
						}
					}





					if (Dump != null && Dump.Length > 0 && Dump.Length >= offset) {
						int ImportDirectoryRva = BitConverter.ToInt32(Dump, PEOffset + 0x090);
						if (ImportDirectoryRva > 0 && ImportDirectoryRva < offset) {
							int current = 0;
							int ThunkToFix = 0;
							int ThunkData = 0;
							isok = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + ImportDirectoryRva + current + 12), Partkeep, 4, ref BytesRead);
							int NameOffset = BitConverter.ToInt32(Partkeep, 0);
							while (isok && NameOffset != 0) {
								byte[] mscoreeAscii = { 0x6D, 0x73, 0x63, 0x6F, 0x72, 0x65, 0x65, 0x2E, 0x64, 0x6C, 0x6C, 0x00 };
								byte[] NameKeeper = new byte[mscoreeAscii.Length];
								isok = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + NameOffset), NameKeeper, (uint)mscoreeAscii.Length, ref BytesRead);
								if (isok && BytesEqual(NameKeeper, mscoreeAscii)) {
									_ = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + ImportDirectoryRva + current), Partkeep, 4, ref BytesRead);
									int OriginalFirstThunk = BitConverter.ToInt32(Partkeep, 0);  // OriginalFirstThunk;
									if (OriginalFirstThunk > 0 && OriginalFirstThunk < offset) {
										_ = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + OriginalFirstThunk), Partkeep, 4, ref BytesRead);
										ThunkData = BitConverter.ToInt32(Partkeep, 0);
										if (ThunkData > 0 && ThunkData < offset) {
											byte[] CorExeMain = { 0x5F, 0x43, 0x6F, 0x72, 0x45, 0x78, 0x65, 0x4D, 0x61, 0x69, 0x6E, 0x00 };
											byte[] CorDllMain = { 0x5F, 0x43, 0x6F, 0x72, 0x44, 0x6C, 0x6C, 0x4D, 0x61, 0x69, 0x6E, 0x00 };
											NameKeeper = new byte[CorExeMain.Length];
											isok = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + ThunkData + 2), NameKeeper,
											(uint)CorExeMain.Length, ref BytesRead);
											if (isok && (BytesEqual(NameKeeper, CorExeMain) || BytesEqual(NameKeeper, CorDllMain))) {
												_ = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + ImportDirectoryRva + current + 16), Partkeep, 4, ref BytesRead);
												ThunkToFix = BitConverter.ToInt32(Partkeep, 0);  // FirstThunk;
												break;
											}
										}
									}
								}

								current += 20; // 20 size of IMAGE_IMPORT_DESCRIPTOR
								isok = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + ImportDirectoryRva + current + 12), Partkeep, 4, ref BytesRead);
								NameOffset = BitConverter.ToInt32(Partkeep, 0);
							}

							if (ThunkToFix > 0 && ThunkToFix < offset) {
								BinaryWriter writer = new BinaryWriter(new MemoryStream(Dump));
								isok = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + ThunkToFix), Partkeep, 4, ref BytesRead);
								int ThunkValue = BitConverter.ToInt32(Partkeep, 0);
								if (isok && (ThunkValue < 0 || ThunkValue > offset)) {
									_ = BitConverter.ToInt32(Dump, PEOffset + 0x0108 + 08);
									int fvirtualAddress = BitConverter.ToInt32(Dump, PEOffset + 0x0108 + 012);
									int frawAddress = BitConverter.ToInt32(Dump, PEOffset + 0x0108 + 20);
									writer.BaseStream.Position = ThunkToFix - fvirtualAddress + frawAddress;
									writer.Write(ThunkData);
								}

								int EntryPoint = BitConverter.ToInt32(Dump, PEOffset + 0x028);
								if (EntryPoint <= 0 || EntryPoint > offset) {
									int ca = 0;
									do {
										isok = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + ThunkData + ca), Partkeep, 1, ref BytesRead);
										if (isok && Partkeep[0] == 0x0FF) {
											isok = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + ThunkData + ca + 1), Partkeep, 1, ref BytesRead);
											if (isok && Partkeep[0] == 0x025) {
												isok = ReadProcessMemory(hProcess, (IntPtr)(ImageBase + ThunkData + ca + 2), Partkeep, 4, ref BytesRead);
												if (isok) {
													int RealEntryPoint = ThunkData + ca;
													writer.BaseStream.Position = PEOffset + 0x028;
													writer.Write(RealEntryPoint);
												}

											}
										}
										ca++;
									}
									while (isok);
								}
								writer.Close();
							}

						}
					}
					if (Dump != null && Dump.Length > 0 && Dump.Length >= offset)
						using (var fout = new FileStream(filePath, FileMode.Create))
							fout.Write(Dump, 0, offset);
					else
						return false;
				}
				else
					return false;
				return true;
			}

			public static unsafe int DumpProcess(uint processId, string DirectoryName) {
				IntPtr processHandle;

				processHandle = OpenProcess(1080u, 0, processId);
				if (processHandle == IntPtr.Zero)
					return 0;
				try {
					MegaDumperHelper.CreateDirectories(DirectoryName);
					int num2 = 1;
					MEMORY_BASIC_INFORMATION memory_BASIC_INFORMATION;
					for (ulong num3 = minAddress; num3 < maxAddress; num3 = memory_BASIC_INFORMATION.BaseAddress + memory_BASIC_INFORMATION.RegionSize) {
						VirtualQueryEx(processHandle, (IntPtr)num3, out memory_BASIC_INFORMATION, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
						if (memory_BASIC_INFORMATION.State == 4096) {
							byte[] array = new byte[memory_BASIC_INFORMATION.RegionSize];
							uint num4 = 0u;
							byte[] array2 = new byte[8];
							bool flag = ReadProcessMemory(processHandle, (IntPtr)(long)num3, array, (uint)array.Length, ref num4);
							if (flag) {
								for (int i = 0; i < array.Length - 2; i++) {
									if (array[i] == 77 && array[i + 1] == 90) {
										if (ReadProcessMemory(processHandle, (IntPtr)(long)(num3 + (ulong)i + 60UL), array2, 4u, ref num4)) {
											int num5 = BitConverter.ToInt32(array2, 0);
											if (num5 > 0 && num5 + 288 < array.Length) {
												if (ReadProcessMemory(processHandle, (IntPtr)(long)(num3 + (ulong)i + (ulong)num5), array2, 2u, ref num4)) {
													if (array2[0] == 80 && array2[1] == 69) {
														long num6 = 0L;
														try {
															if (ReadProcessMemory(processHandle, (IntPtr)(long)(num3 + (ulong)i + (ulong)num5 + 248UL), array2, 8u, ref num4))
																num6 = BitConverter.ToInt64(array2, 0);
														}
														catch {
														}
														#region Dump Native
														byte[] array3 = new byte[PageSize];
														if (ReadProcessMemory(processHandle, (IntPtr)(long)(num3 + (ulong)i), array3, (uint)array3.Length, ref num4)) {
															int num7 = BitConverter.ToInt16(array3, num5 + 6);
															if (num7 > 0) {
																int num8 = BitConverter.ToInt32(array3, num5 + 56);
																int num9 = BitConverter.ToInt32(array3, num5 + 60);
																short num10 = BitConverter.ToInt16(array3, num5 + 20);
																bool isDll = false;
																if ((array3[num5 + 23] & 32) != 0) {
																	isDll = true;
																}
																IntPtr ptr = IntPtr.Zero;
																IMAGE_SECTION_HEADER[] array4 = new IMAGE_SECTION_HEADER[num7];
																ulong num11 = num3 + (ulong)i + (ulong)num5 + (ulong)num10 + 4UL + (ulong)Marshal.SizeOf(typeof(IMAGE_FILE_HEADER));
																for (int j = 0; j < num7; j++) {
																	byte[] array5 = new byte[Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER))];
																	ReadProcessMemory(processHandle, (IntPtr)(long)num11, array5, (uint)array5.Length, ref num4);
																	fixed (byte* ptr2 = array5) {
																		ptr = (IntPtr)(void*)ptr2;
																	}
																	array4[j] = (IMAGE_SECTION_HEADER)Marshal.PtrToStructure(ptr, typeof(IMAGE_SECTION_HEADER));
																	num11 += (ulong)Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER));
																}
																int num12 = 0;
																int size_of_raw_data = array4[num7 - 1].size_of_raw_data;
																int pointer_to_raw_data = array4[num7 - 1].pointer_to_raw_data;
																if (size_of_raw_data > 0 && pointer_to_raw_data > 0) {
																	num12 = size_of_raw_data + pointer_to_raw_data;
																}
																int num13 = BitConverter.ToInt32(array3, num5 + 80);
																int num14 = num13;
																int num15 = array4[0].pointer_to_raw_data;
																int num16 = 0;
																for (int j = 0; j < num7; j++) {
																	int virtual_size = array4[j].virtual_size;
																	int num17 = virtual_size % num8;
																	if (num17 != 0) {
																		num17 = num8 - num17;
																	}
																	num15 = num15 + virtual_size + num17;
																}
																if (num15 > num14) {
																	num14 = num15;
																}
																try {
																	byte[] array6 = new byte[num12];
																}
																catch {
																	num12 = num14;
																}
																if (num12 != 0) {
																	byte[] array7 = new byte[num12];
																	try {
																		if (ReadProcessMemory(processHandle, (IntPtr)(long)(num3 + (ulong)i), array7, (uint)array7.Length, ref num4)) {
																			string filePath1 = Path.Combine(DirectoryName, "rawdump_" + (num3 + (ulong)i).ToString("X16"));
																			if (File.Exists(filePath1))
																				filePath1 = Path.Combine(DirectoryName, "rawdump" + num2.ToString() + "_" + (num3 + (ulong)i).ToString("X16"));
																			if (isDll)
																				filePath1 += ".dll";
																			else
																				filePath1 += ".exe";
																			File.WriteAllBytes(filePath1, array7);
																			num2++;
																		}
																	}
																	catch {
																	}
																}
																byte[] array8 = new byte[num14];
																Array.Copy(array3, array8, (long)(ulong)PageSize);
																int num18 = 0;
																for (int k = 0; k < num7; k++) {
																	int num19 = array4[k].size_of_raw_data;
																	int num20 = array4[k].pointer_to_raw_data;
																	int virtual_size = array4[k].virtual_size;
																	num16 = array4[k].virtual_address;
																	#region Dump RAW
																	int num21 = virtual_size % num9;
																	if (num21 != 0) {
																		num21 = num9 - num21;
																	}
																	num21 = virtual_size + num21;
																	if ((num21 != 0 && num19 != num21 && num19 != virtual_size) || num20 < 0) {
																		num19 = virtual_size;
																		num20 = num16;
																		BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(array8));
																		binaryWriter.BaseStream.Position = num5 + 232 + (40 * k) + 20 + 28;
																		binaryWriter.Write(virtual_size);
																		binaryWriter.BaseStream.Position = num5 + 232 + (40 * k) + 24 + 28;
																		binaryWriter.Write(num16);
																		binaryWriter.Close();
																	}
																	#endregion
																	byte[] array9 = new byte[0];
																	try {
																		array9 = new byte[num19];
																	}
																	catch {
																		array9 = new byte[virtual_size];
																	}
																	int num22 = array9.Length;
																	flag = ReadProcessMemory(processHandle, (IntPtr)(long)(num3 + (ulong)i + (ulong)num16), array9, (uint)num19, ref num4);
																	if (!flag || num4 != (ulong)num19) {
																		num22 = 0;
																		byte[] array10 = new byte[PageSize];
																		for (int l = 0; l < num19; l += (int)PageSize) {
																			try {
																				flag = ReadProcessMemory(processHandle, (IntPtr)(long)(num3 + (ulong)i + (ulong)num16 + (ulong)l), array10, PageSize, ref num4);
																			}
																			catch {
																				break;
																			}
																			if (flag) {
																				num22 += (int)PageSize;
																				int j = 0;
																				while (j < (long)(ulong)PageSize) {
																					if (l + j < array9.Length)
																						array9[l + j] = array10[j];
																					j++;
																				}
																			}
																		}
																	}
																	try {
																		Array.Copy(array9, 0, array8, num20, num22);
																	}
																	catch {
																	}
																	if (k == num7 - 1)
																		num18 = num20 + num19;
																}
																string filePath2 = Path.Combine(DirectoryName, "vdump_" + (num3 + (ulong)i).ToString("X16"));
																if (File.Exists(filePath2))
																	filePath2 = Path.Combine(DirectoryName, "vdump" + num2.ToString() + "_" + (num3 + (ulong)i).ToString("X16"));
																if (isDll)
																	filePath2 += ".dll";
																else
																	filePath2 += ".exe";
																using (FileStream fileStream = new FileStream(filePath2, FileMode.Create))
																	fileStream.Write(array8, 0, Math.Min(num18, array8.Length));
																num2++;
															}
														}
														#endregion
													}
												}
											}
										}
									}
								}
							}
						}
					}
					#region 修复文件名
					foreach (FileInfo fileInfo in new DirectoryInfo(DirectoryName).GetFiles()) {
						FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(fileInfo.FullName);
						string validOriginalFilename = MegaDumperHelper.EnsureValidFileName(versionInfo.OriginalFilename);
						if (validOriginalFilename != "") {
							string filePath3 = Path.Combine(DirectoryName, validOriginalFilename);
							int repetition = 2;
							if (File.Exists(filePath3)) {
								string extension = Path.GetExtension(filePath3);
								if (extension == "")
									extension = ".dll";
								do {
									filePath3 = Path.Combine(DirectoryName, Path.GetFileNameWithoutExtension(validOriginalFilename) + "(" + repetition.ToString() + ")" + extension);
									repetition++;
								}
								while (File.Exists(filePath3));
							}
							File.Move(fileInfo.FullName, filePath3);
						}
					}
					MegaDumperHelper.Classify(DirectoryName);
					#endregion
					num2--;
					return num2;
				}
				finally {
					CloseHandle(processHandle);
				}
			}

			private struct SYSTEM_INFO {
				public ushort wProcessorArchitecture;

				public ushort wReserved;

				public uint dwPageSize;

				public IntPtr lpMinimumApplicationAddress;

				public IntPtr lpMaximumApplicationAddress;

				public UIntPtr dwActiveProcessorMask;

				public uint dwNumberOfProcessors;

				public uint dwProcessorType;

				public uint dwAllocationGranularity;

				public ushort wProcessorLevel;

				public ushort wProcessorRevision;
			}

			private struct MEMORY_BASIC_INFORMATION {
				public ulong BaseAddress;

				public ulong AllocationBase;

				public int AllocationProtect;

				public ulong RegionSize;

				public int State;

				public ulong Protect;

				public ulong Type;
			}

#pragma warning disable 0649
			private unsafe struct IMAGE_SECTION_HEADER {
				public fixed byte name[8];

				public int virtual_size;

				public int virtual_address;

				public int size_of_raw_data;

				public int pointer_to_raw_data;

				public int pointer_to_relocations;

				public int pointer_to_linenumbers;

				public short number_of_relocations;

				public short number_of_linenumbers;

				public int characteristics;
			}

			private struct IMAGE_FILE_HEADER {
				public short Machine;

				public short NumberOfSections;

				public int TimeDateStamp;

				public int PointerToSymbolTable;

				public int NumberOfSymbols;

				public short SizeOfOptionalHeader;

				public short Characteristics;
			}
#pragma warning restore 0649
		}

		private static class MegaDumperHelper {
			private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

			public static void CreateDirectories(string DirectoryName) {
				if (!Directory.Exists(Path.Combine(DirectoryName, ".Net Assemblies")))
					Directory.CreateDirectory(Path.Combine(DirectoryName, ".Net Assemblies"));
				if (!Directory.Exists(Path.Combine(DirectoryName, "Native Dlls")))
					Directory.CreateDirectory(Path.Combine(DirectoryName, "Native Dlls"));
				if (!Directory.Exists(Path.Combine(DirectoryName, "RawDumps")))
					Directory.CreateDirectory(Path.Combine(DirectoryName, "RawDumps"));
				if (!Directory.Exists(Path.Combine(DirectoryName, "VDumps")))
					Directory.CreateDirectory(Path.Combine(DirectoryName, "VDumps"));
				if (!Directory.Exists(Path.Combine(DirectoryName, "Unknowns")))
					Directory.CreateDirectory(Path.Combine(DirectoryName, "Unknowns"));
			}

			public static void Classify(string DirectoryName) {
				foreach (FileInfo fileInfo in new DirectoryInfo(DirectoryName).GetFiles()) {
					if (fileInfo.Name.StartsWith("rawdump_")) {
						File.Move(fileInfo.FullName, Path.Combine(fileInfo.DirectoryName, "RawDumps", fileInfo.Name));
						continue;
					}
					if (fileInfo.Name.StartsWith("vdump_")) {
						File.Move(fileInfo.FullName, Path.Combine(fileInfo.DirectoryName, "VDumps", fileInfo.Name));
						continue;
					}
					if (AssemblyDetector.IsAssembly(fileInfo.FullName)) {
						File.Move(fileInfo.FullName, Path.Combine(fileInfo.DirectoryName, ".Net Assemblies", fileInfo.Name));
						continue;
					}
					if (fileInfo.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) {
						File.Move(fileInfo.FullName, Path.Combine(fileInfo.DirectoryName, "Native Dlls", fileInfo.Name));
						continue;
					}
					File.Move(fileInfo.FullName, Path.Combine(fileInfo.DirectoryName, "Unknowns", fileInfo.Name));
				}
			}

			public static string EnsureValidFileName(string fileName) {
				if (string.IsNullOrEmpty(fileName))
					return string.Empty;

				StringBuilder newFileName;

				newFileName = new StringBuilder(fileName.Length);
				foreach (char chr in fileName)
					if (!InvalidFileNameChars.Contains(chr))
						newFileName.Append(chr);
				return newFileName.ToString();
			}
		}
		#endregion
	}
}
