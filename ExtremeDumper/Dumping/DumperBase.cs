using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using dnlib.PE;
using NativeSharp;

namespace ExtremeDumper.Dumping;

abstract class DumperBase : IDumper {
	static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

	protected readonly NativeProcess process;

	protected DumperBase(uint processId) {
		process = NativeProcess.Open(processId, ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
	}

	public abstract bool DumpModule(nuint moduleHandle, ImageLayout imageLayout, string filePath);

	public abstract int DumpProcess(string directoryPath);

	protected static string EnsureValidFileName(string fileName) {
		if (string.IsNullOrEmpty(fileName))
			return string.Empty;

		var newFileName = new StringBuilder(fileName.Length);
		foreach (char chr in fileName) {
			if (!InvalidFileNameChars.Contains(chr))
				newFileName.Append(chr);
		}
		return newFileName.ToString();
	}

	protected static bool IsSameFile(string directoryPath, string fileName, byte[] data, ConcurrentDictionary<string, byte[]> originalFileCache) {
		string filePath = Path.Combine(directoryPath, fileName);
		if (!File.Exists(filePath)) {
			originalFileCache[fileName] = data;
			return false;
		}

		if (!originalFileCache.TryGetValue(fileName, out byte[] originalData)) {
			originalData = File.ReadAllBytes(filePath);
			originalFileCache.TryAdd(fileName, originalData);
		}

		if (data.Length != originalData.Length)
			return false;

		for (int i = 0; i < data.Length; i++) {
			if (data[i] != originalData[i])
				return false;
		}

		return true;
	}

	protected static string EnsureNoRepeatFileName(string directoryPath, string fileName) {
		int count = 1;
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
		string extension = Path.GetExtension(fileName);
		string filePath;
		while (File.Exists(filePath = Path.Combine(directoryPath, fileName))) {
			count++;
			fileName = $"{fileNameWithoutExtension} ({count}){extension}";
		}
		return fileName;
	}

	public virtual void Dispose() {
		process.Dispose();
	}
}
