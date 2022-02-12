using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace ExtremeDumper.AntiAntiDump.Handlers;

sealed class EnableMultiDomainHandler : ICommandHandler {
	public sealed class PipeNames : ISerializable {
		public string[] Values = Array2.Empty<string>();

		bool ISerializable.Serialize(Stream destination) {
			return SimpleSerializer.Write(destination, this);
		}

		bool ISerializable.Deserialize(Stream source) {
			return SimpleSerializer.Read(source, this);
		}
	}

	sealed class AADServerCreator : MarshalByRefObject {
		public bool Create(string pipeName) {
			Injection.RunAADServerAsync(pipeName);
			return true;
		}
	}

	public AADCommand Command => AADCommand.EnableMultiDomain;

	public Type ParametersType => typeof(EmptySerializable);

	public Type ResultType => typeof(PipeNames);

	public bool Execute(ISerializable parameters, [NotNullWhen(true)] out ISerializable? result) {
		result = null;
		if (!MultiDomainHelper.IsSupported)
			return false;

		var assemblyLocation = typeof(AADServer).Assembly.Location;
		if (string.IsNullOrEmpty(assemblyLocation))
			return false;

		var pipeNames = new List<string>();
		foreach (var domain in MultiDomainHelper.EnumerateDomains()) {
			if (domain == AppDomain.CurrentDomain)
				continue;
			var creator = (AADServerCreator)domain.CreateInstanceFromAndUnwrap(assemblyLocation, typeof(AADServerCreator).FullName);
			var name = Guid.NewGuid().ToString();
			if (!creator.Create(name))
				return false;
			// TODO: cleanup
			pipeNames.Add(name);
		}
		result = new PipeNames { Values = pipeNames.ToArray() };
		return true;
	}

	static class MultiDomainHelper {
		/// <summary>
		/// Does current runtime support multi application domains
		/// </summary>
		public static bool IsSupported { get; } = MetadataLocator.RuntimeEnvironment.Flavor == MetadataLocator.RuntimeFlavor.Framework;

		/// <summary>
		/// Enumerate all <see cref="AppDomain"/>s
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<AppDomain> EnumerateDomains() {
			if (!IsSupported) {
				yield return AppDomain.CurrentDomain;
				yield break;
			}

			var host = (ICorRuntimeHost)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("CB2F6723-AB3A-11D2-9C40-00C04FA30A3E")));
			host.EnumDomains(out var enumHandle);
			try {
				while (true) {
					host.NextDomain(enumHandle, out var obj);
					if (obj is AppDomain domain)
						yield return domain;
					else
						yield break;
				}
			}
			finally {
				host.CloseEnum(enumHandle);
			}
		}

		[ComImport, Guid("CB2F6722-AB3A-11D2-9C40-00C04FA30A3E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		interface ICorRuntimeHost {
			void _VtblGap1_11();
			void EnumDomains(out nuint hEnum);
			void NextDomain(nuint hEnum, [MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
			void CloseEnum(nuint hEnum);
		}
	}
}
