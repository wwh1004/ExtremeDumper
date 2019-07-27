using System.IO;
using System.Text;

namespace ExtremeDumper.AntiAntiDump.Serialization {
	/// <summary>
	/// 对对象进行XML序列化/反序列化操作，对象访问级别至少为public
	/// </summary>
	public static class XmlSerializer {
		/// <summary>
		/// 序列化对象为XML
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj">被序列化的对象</param>
		/// <returns></returns>
		public static string Serialize<T>(T obj) {
			using (MemoryStream stream = new MemoryStream()) {
				Serializer<T>.Instance.Serialize(stream, obj);
				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}

		/// <summary>
		/// 反序列化XML为对象
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="xml">XML</param>
		/// <returns></returns>
		public static T Deserialize<T>(string xml) {
			using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
				return Deserialize<T>(stream);
		}

		/// <summary>
		/// 反序列化XML为对象
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="xml">XML</param>
		/// <returns></returns>
		public static T Deserialize<T>(Stream xml) {
			return (T)Serializer<T>.Instance.Deserialize(xml);
		}

		private static class Serializer<T> {
			public static readonly System.Xml.Serialization.XmlSerializer Instance = new System.Xml.Serialization.XmlSerializer(typeof(T));
		}
	}
}
