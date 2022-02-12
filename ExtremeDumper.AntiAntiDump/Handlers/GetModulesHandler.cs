using System;

namespace ExtremeDumper.AntiAntiDump.Handlers;

sealed class GetModulesHandler : ICommandHandler {
	public AADCommand Command => AADCommand.GetModules;

	public Type ParametersType => typeof(EmptySerializable);

	public Type ResultType => typeof(ModuleInfos);

	public bool Execute(ISerializable parameters, out ISerializable result) {
		var moduleInfos = new ModuleInfos();
		var domain = AppDomain.CurrentDomain;
		uint domainId = (uint)domain.Id;
		var domainName = domain.FriendlyName;
		foreach (var assembly in domain.GetAssemblies()) {
			var assemblyName = assembly.GetName().Name;
			foreach (var module in assembly.GetLoadedModules()) {
				var moduleInfo = new ModuleInfo {
					Name = module.ScopeName,
					AssemblyName = assemblyName,
					DomainId = domainId,
					DomainName = domainName
				};
				moduleInfos.Add(moduleInfo);
			}
		}
		result = moduleInfos;
		return true;
	}
}
