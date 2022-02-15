using System;
using System.Diagnostics.CodeAnalysis;

namespace ExtremeDumper.AntiAntiDump.Handlers;

sealed class GetAppDomainInfoHandler : ICommandHandler {
	public AADCommand Command => AADCommand.GetAppDomainInfo;

	public Type ParametersType => typeof(EmptySerializable);

	public Type ResultType => typeof(AppDomainInfo);

	public bool Execute(ISerializable parameters, [NotNullWhen(true)] out ISerializable? result) {
		var domain = AppDomain.CurrentDomain;
		result = new AppDomainInfo {
			Id = (uint)domain.Id,
			Name = domain.FriendlyName
		};
		return true;
	}
}
