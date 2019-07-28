using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using ExtremeDumper.AntiAntiDump.Serialization;
using InternalMetadataInfo = MetadataLocator.MetadataInfo;

namespace ExtremeDumper.AntiAntiDump {
	/// <summary>
	/// Provide metadata info service
	/// </summary>
	public sealed unsafe class MetadataInfoService : MarshalByRefObject {
		private static readonly Dictionary<IntPtr, Module> _cachedModules = new Dictionary<IntPtr, Module>();

		public void Start(string portName, string objectName) {
			if (string.IsNullOrEmpty(portName))
				throw new ArgumentNullException(nameof(portName));
			if (string.IsNullOrEmpty(objectName))
				throw new ArgumentNullException(nameof(objectName));

			ChannelServices.RegisterChannel(new IpcServerChannel(null, portName), false);
			RemotingServices.Marshal(this, objectName);
		}

		/// <summary>
		/// Get metadata info
		/// </summary>
		/// <param name="moduleHandle"></param>
		/// <returns></returns>
		public string GetMetadataInfo(IntPtr moduleHandle) {
			Module module;
			MetadataInfo metadataInfo;

			module = GetModuleFromNativeModuleHandle(moduleHandle);
			metadataInfo = new MetadataInfo(InternalMetadataInfo.GetMetadataInfo(module));
			return XmlSerializer.Serialize(metadataInfo);
		}

		private static Module GetModuleFromNativeModuleHandle(IntPtr moduleHandle) {
			Module module;

			if (!_cachedModules.TryGetValue(moduleHandle, out module)) {
				module = EnumerateLoadedModules().FirstOrDefault(t => Marshal.GetHINSTANCE(t) == moduleHandle);
				if (!(module is null))
					// 如果获取成功，缓存结果，EnumerateLoadedModules()是很耗时的操作
					_cachedModules.Add(moduleHandle, module);
			}
			if (module is null)
				throw new InvalidOperationException($"Can't get System.Reflection.Module from a native module handle (0x{moduleHandle.ToString(IntPtr.Size == 4 ? "X8" : "X16")}).");
			return module;
		}

		private static IEnumerable<Module> EnumerateLoadedModules() {
			return AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetLoadedModules());
		}
	}
}
