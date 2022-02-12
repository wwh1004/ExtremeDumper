using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// Represent a serializable object to provider a general interface for <see cref="AADPipe"/> to transfer object
/// </summary>
interface ISerializable {
	/// <summary>
	/// Serialize to <paramref name="destination"/>
	/// </summary>
	/// <param name="destination"></param>
	/// <returns></returns>
	/// <remarks>Guarantee no exception thrown</remarks>
	bool Serialize(Stream destination);

	/// <summary>
	/// Deserialize from <paramref name="source"/>
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	/// <remarks>Guarantee no exception thrown</remarks>
	bool Deserialize(Stream source);
}

sealed class EmptySerializable : ISerializable {
	public static readonly EmptySerializable Instance = new();

	private EmptySerializable() { }

	public bool Serialize(Stream destination) {
		return true;
	}

	public bool Deserialize(Stream source) {
		return true;
	}
}

/// <summary>
/// Serialize and deserialize basic types
/// </summary>
/// <remarks>
/// <para>Guarantee no exception thrown.</para>
/// <para>Should only called by <see cref="ISerializable"/> implement. Other classes should always call methods in <see cref="ISerializable"/> interface.</para>
/// </remarks>
static class Serializer {
	public static bool WriteBoolean(Stream destination, bool value) {
		byte[] buffer = new byte[] { value ? (byte)1 : (byte)0 };
		return destination.Write(buffer);
	}

	public static bool ReadBoolean(Stream source, out bool value) {
		value = false;
		var buffer = new byte[1];
		if (!source.Read(buffer))
			return false;

		value = buffer[0] != 0;
		return true;
	}

	public static bool WriteByte(Stream destination, byte value) {
		byte[] buffer = new byte[] { value };
		return destination.Write(buffer);
	}

	public static bool ReadByte(Stream source, out byte value) {
		value = 0;
		var buffer = new byte[1];
		if (!source.Read(buffer))
			return false;

		value = buffer[0];
		return true;
	}

	public static bool WriteInt32(Stream destination, int value) {
		byte[] buffer = BitConverter.GetBytes(value);
		return destination.Write(buffer);
	}

	public static bool ReadInt32(Stream source, out int value) {
		value = 0;
		var buffer = new byte[4];
		if (!source.Read(buffer))
			return false;

		value = BitConverter.ToInt32(buffer, 0);
		return true;
	}

	public static bool WriteUInt32(Stream destination, uint value) {
		byte[] buffer = BitConverter.GetBytes(value);
		return destination.Write(buffer);
	}

	public static bool ReadUInt32(Stream source, out uint value) {
		value = 0;
		var buffer = new byte[4];
		if (!source.Read(buffer))
			return false;

		value = BitConverter.ToUInt32(buffer, 0);
		return true;
	}

	public static bool WriteUInt64(Stream destination, ulong value) {
		byte[] buffer = BitConverter.GetBytes(value);
		return destination.Write(buffer);
	}

	public static bool ReadUInt64(Stream source, out ulong value) {
		value = 0;
		var buffer = new byte[8];
		if (!source.Read(buffer))
			return false;

		value = BitConverter.ToUInt64(buffer, 0);
		return true;
	}

	public static bool WriteInt64(Stream destination, long value) {
		byte[] buffer = BitConverter.GetBytes(value);
		return destination.Write(buffer);
	}

	public static bool ReadInt64(Stream source, out long value) {
		value = 0;
		var buffer = new byte[8];
		if (!source.Read(buffer))
			return false;

		value = BitConverter.ToInt64(buffer, 0);
		return true;
	}

	public static bool WriteString(Stream destination, string value) {
		if (value is null)
			return false;

		var buffer = Encoding.UTF8.GetBytes(value);
		return WriteBytes(destination, buffer);
	}

	public static bool ReadString(Stream source, out string value) {
		value = string.Empty;
		if (!ReadBytes(source, out var buffer))
			return false;

		value = Encoding.UTF8.GetString(buffer);
		return true;
	}

	public static bool WriteBytes(Stream destination, byte[] value) {
		if (value is null)
			return false;

		WriteInt32(destination, value.Length);
		return destination.Write(value);
	}

	public static bool ReadBytes(Stream source, out byte[] value) {
		value = Array2.Empty<byte>();
		if (!ReadInt32(source, out int length))
			return false;

		value = new byte[length];
		return source.Read(value);
	}

	public static bool WriteInt32Array(Stream destination, int[] array) {
		if (array is null)
			return false;

		if (!WriteInt32(destination, array.Length))
			return false;
		foreach (int element in array) {
			if (!WriteInt32(destination, element))
				return false;
		}
		return true;
	}

	public static bool ReadInt32Array(Stream source, out int[] array) {
		array = Array2.Empty<int>();
		if (!ReadInt32(source, out int length))
			return false;

		array = new int[length];
		for (int i = 0; i < length; i++) {
			if (!ReadInt32(source, out array[i]))
				return false;
		}
		return true;
	}

	public static bool WriteUInt32Array(Stream destination, uint[] array) {
		if (array is null)
			return false;

		if (!WriteInt32(destination, array.Length))
			return false;
		foreach (uint element in array) {
			if (!WriteUInt32(destination, element))
				return false;
		}
		return true;
	}

	public static bool ReadUInt32Array(Stream source, out uint[] array) {
		array = Array2.Empty<uint>();
		if (!ReadInt32(source, out int length))
			return false;

		array = new uint[length];
		for (int i = 0; i < length; i++) {
			if (!ReadUInt32(source, out array[i]))
				return false;
		}
		return true;
	}

	public static bool WriteInt64Array(Stream destination, long[] array) {
		if (array is null)
			return false;

		if (!WriteInt32(destination, array.Length))
			return false;
		foreach (long element in array) {
			if (!WriteInt64(destination, element))
				return false;
		}
		return true;
	}

	public static bool ReadInt64Array(Stream source, out long[] array) {
		array = Array2.Empty<long>();
		if (!ReadInt32(source, out int length))
			return false;

		array = new long[length];
		for (int i = 0; i < length; i++) {
			if (!ReadInt64(source, out array[i]))
				return false;
		}
		return true;
	}

	public static bool WriteUInt64Array(Stream destination, ulong[] array) {
		if (array is null)
			return false;

		if (!WriteInt32(destination, array.Length))
			return false;
		foreach (ulong element in array) {
			if (!WriteUInt64(destination, element))
				return false;
		}
		return true;
	}

	public static bool ReadUInt64Array(Stream source, out ulong[] array) {
		array = Array2.Empty<ulong>();
		if (!ReadInt32(source, out int length))
			return false;

		array = new ulong[length];
		for (int i = 0; i < length; i++) {
			if (!ReadUInt64(source, out array[i]))
				return false;
		}
		return true;
	}

	public static bool WriteStringArray(Stream destination, string[] array) {
		if (array is null)
			return false;

		if (!WriteInt32(destination, array.Length))
			return false;
		foreach (string element in array) {
			if (!WriteString(destination, element))
				return false;
		}
		return true;
	}

	public static bool ReadStringArray(Stream source, out string[] array) {
		array = Array2.Empty<string>();
		if (!ReadInt32(source, out int length))
			return false;

		array = new string[length];
		for (int i = 0; i < length; i++) {
			if (!ReadString(source, out array[i]))
				return false;
		}
		return true;
	}

	static bool Write(this Stream stream, byte[] data) {
		try {
			stream.Write(data, 0, data.Length);
			return true;
		}
		catch {
			return false;
		}
	}

	static bool Read(this Stream stream, byte[] data) {
		bool b;
		try {
			b = stream.Read(data, 0, data.Length) == data.Length;
		}
		catch {
			b = false;
		}
		return b;
	}
}

/// <summary>
/// A simple <see cref="ISerializable"/> object serializer to quickly implement <see cref="ISerializable"/> interface
/// </summary>
/// <remarks>Should only called by <see cref="ISerializable"/> implement. Other classes should always call methods in <see cref="ISerializable"/> interface.</remarks>
static class SimpleSerializer {
	/// <summary>
	/// Write <see cref="ISerializable"/> instance which has simple internal layout
	/// </summary>
	/// <param name="destination"></param>
	/// <param name="obj"></param>
	/// <returns></returns>
	/// <remarks>Should only called by <see cref="ISerializable.Serialize(Stream)"/> to quick serialize itself</remarks>
	public static bool Write(Stream destination, ISerializable obj) {
		var fields = obj.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
		foreach (var field in fields) {
			var value = field.GetValue(obj);
			switch (Type.GetTypeCode(field.FieldType)) {
			case TypeCode.Boolean:
				if (!Serializer.WriteBoolean(destination, (bool)value))
					return false;
				continue;
			case TypeCode.Byte:
				if (!Serializer.WriteByte(destination, (byte)value))
					return false;
				continue;
			case TypeCode.Int32:
				if (!Serializer.WriteInt32(destination, (int)value))
					return false;
				continue;
			case TypeCode.UInt32:
				if (!Serializer.WriteUInt32(destination, (uint)value))
					return false;
				continue;
			case TypeCode.Int64:
				if (!Serializer.WriteInt64(destination, (long)value))
					return false;
				continue;
			case TypeCode.UInt64:
				if (!Serializer.WriteUInt64(destination, (ulong)value))
					return false;
				continue;
			case TypeCode.String:
				if (!Serializer.WriteString(destination, (string)value))
					return false;
				continue;
			}
			if (value is int[] i4s) {
				if (!Serializer.WriteInt32Array(destination, i4s))
					return false;
				continue;
			}
			if (value is uint[] u4s) {
				if (!Serializer.WriteUInt32Array(destination, u4s))
					return false;
				continue;
			}
			if (value is long[] i8s) {
				if (!Serializer.WriteInt64Array(destination, i8s))
					return false;
				continue;
			}
			if (value is ulong[] u8s) {
				if (!Serializer.WriteUInt64Array(destination, u8s))
					return false;
				continue;
			}
			if (value is string[] ss) {
				if (!Serializer.WriteStringArray(destination, ss))
					return false;
				continue;
			}
			if (value is byte[] bin) {
				if (!Serializer.WriteBytes(destination, bin))
					return false;
				continue;
			}
			if (value is ISerializable o) {
				if (!Write(destination, o))
					return false;
				continue;
			}
			Debug2.Assert(false);
			return false;
		}
		return true;
	}

	/// <summary>
	/// Read <see cref="ISerializable"/> instance which has simple internal layout
	/// </summary>
	/// <param name="destination"></param>
	/// <param name="obj"></param>
	/// <returns></returns>
	/// <remarks>Should only called by <see cref="ISerializable.Deserialize(Stream)(Stream)"/> to quick deserialize itself</remarks>
	public static bool Read(Stream source, ISerializable obj) {
		var fields = obj.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
		foreach (var field in fields) {
			object? value;
			var fieldType = field.FieldType;
			switch (Type.GetTypeCode(fieldType)) {
			case TypeCode.Boolean:
				if (!Serializer.ReadBoolean(source, out bool b))
					return false;
				value = b;
				goto next;
			case TypeCode.Byte:
				if (!Serializer.ReadByte(source, out byte u1))
					return false;
				value = u1;
				goto next;
			case TypeCode.Int32:
				if (!Serializer.ReadInt32(source, out int i4))
					return false;
				value = i4;
				goto next;
			case TypeCode.UInt32:
				if (!Serializer.ReadUInt32(source, out uint u4))
					return false;
				value = u4;
				goto next;
			case TypeCode.Int64:
				if (!Serializer.ReadInt64(source, out long i8))
					return false;
				value = i8;
				goto next;
			case TypeCode.UInt64:
				if (!Serializer.ReadUInt64(source, out ulong u8))
					return false;
				value = u8;
				goto next;
			case TypeCode.String:
				if (!Serializer.ReadString(source, out var s))
					return false;
				value = s;
				goto next;
			}
			if (fieldType == typeof(byte[])) {
				if (!Serializer.ReadBytes(source, out var bin))
					return false;
				value = bin;
				goto next;
			}
			if (fieldType == typeof(int[])) {
				if (!Serializer.ReadInt32Array(source, out var i4s))
					return false;
				value = i4s;
				goto next;
			}
			if (fieldType == typeof(uint[])) {
				if (!Serializer.ReadUInt32Array(source, out var u4s))
					return false;
				value = u4s;
				goto next;
			}
			if (fieldType == typeof(long[])) {
				if (!Serializer.ReadInt64Array(source, out var i8s))
					return false;
				value = i8s;
				goto next;
			}
			if (fieldType == typeof(ulong[])) {
				if (!Serializer.ReadUInt64Array(source, out var u8s))
					return false;
				value = u8s;
				goto next;
			}
			if (fieldType == typeof(string[])) {
				if (!Serializer.ReadStringArray(source, out var ss))
					return false;
				value = ss;
				goto next;
			}
			if (typeof(ISerializable).IsAssignableFrom(fieldType)) {
				var o = (ISerializable)Activator.CreateInstance(fieldType, true);
				if (!Read(source, o))
					return false;
				value = o;
				goto next;
			}
			Debug2.Assert(false);
			return false;

		next:
			field.SetValue(obj, value);
		}
		return true;
	}

	/// <summary>
	/// Write list of <see cref="ISerializable"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="destination"></param>
	/// <param name="list"></param>
	/// <returns></returns>
	public static bool WriteList<T>(Stream destination, IList<T> list) where T : class, ISerializable {
		if (list is null)
			return false;

		if (!Serializer.WriteInt32(destination, list.Count))
			return false;
		foreach (var element in list) {
			if (element is null)
				return false;
			if (!element.Serialize(destination))
				return false;
		}
		return true;
	}

	/// <summary>
	/// Read list of <see cref="ISerializable"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="source"></param>
	/// <param name="list"></param>
	/// <returns></returns>
	public static bool ReadList<T>(Stream source, IList<T> list) where T : class, ISerializable, new() {
		list.Clear();
		if (!Serializer.ReadInt32(source, out int length))
			return false;

		for (int i = 0; i < length; i++) {
			var element = new T();
			if (!element.Deserialize(source))
				return false;
			list.Add(element);
		}
		return true;
	}
}
