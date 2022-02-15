namespace ExtremeDumper.AntiAntiDump;

enum AADCommand : uint {
	Invalid,

	Disconnect,
	EnableMultiDomain,
	GetAppDomainInfo,
	GetRuntimeInfo,
	GetModules,
	GetMetadataInfo,
	GetPEInfo,
	// Client -> Server

	Success = ushort.MaxValue + 1,
	Failure,
	UnhandledException,
	// Server -> Client
}
