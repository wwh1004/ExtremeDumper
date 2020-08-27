using System;
using System.Linq;
using dnlib.PE;
using dnlib.W32Resources;

namespace ExtremeDumper.Dumping {
	internal static unsafe class Extensions {
		public static string GetOriginalFilename(this IPEImage peImage) {
			if (peImage is null)
				throw new ArgumentNullException(nameof(peImage));

			var resourceData = peImage.Win32Resources?.Find(new ResourceName(16), new ResourceName(1))?.Data?.FirstOrDefault();
			if (resourceData is null)
				return null;
			byte[] data = resourceData.CreateReader().ReadRemainingBytes();
			fixed (byte* p = data)
				return new FileVersionInfo(p, data.Length).OriginalFilename;
		}

		/// <summary>
		/// FileVersionInfo reprents the extended version formation that is optionally placed in the PE file resource area.
		/// SOURCE FROM CLRMD!!!
		/// </summary>
		private sealed class FileVersionInfo {
			public string OriginalFilename { get; private set; }

			internal FileVersionInfo(byte* data, int dataLen) {
				OriginalFilename = "";
				if (dataLen <= 0x5c)
					return;

				// See http://msdn.microsoft.com/en-us/library/ms647001(v=VS.85).aspx
				byte* stringInfoPtr = data + 0x5c;   // Gets to first StringInfo

				// TODO search for FileVersion string ... 
				string dataAsString = new string((char*)stringInfoPtr, 0, (dataLen - 0x5c) / 2);

				OriginalFilename = GetDataString(dataAsString, "OriginalFilename");
			}

			private static string GetDataString(string dataAsString, string fileVersionKey) {
				int fileVersionIdx = dataAsString.IndexOf(fileVersionKey);
				if (fileVersionIdx >= 0) {
					int valIdx = fileVersionIdx + fileVersionKey.Length;
					for (; ; )
					{
						valIdx++;
						if (valIdx >= dataAsString.Length)
							return null;
						if (dataAsString[valIdx] != (char)0)
							break;
					}
					int varEndIdx = dataAsString.IndexOf((char)0, valIdx);
					if (varEndIdx < 0)
						return null;

					return dataAsString.Substring(valIdx, varEndIdx - valIdx);
				}

				return null;
			}
		}
	}
}
