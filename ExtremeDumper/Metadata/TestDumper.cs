using System;
using dndbg.COM.MetaData;
using dndbg.DotNet;
using Microsoft.Diagnostics.Runtime;

namespace ExtremeDumper.Metadata
{
    public class TestDumper : MetadataDumper
    {
        private uint _processId;

        public TestDumper(uint processId) => _processId = processId;

        public override bool DumpModule(IntPtr moduleHandle, string filePath)
        {
            ClrModule clrModule;

            clrModule = GetClrModule(moduleHandle);
            if (clrModule == null)
                return false;
            CorModuleDef corModuleDef = new CorModuleDef((IMetaDataImport)clrModule.MetadataImport, new CorModuleDefHelper(_processId, clrModule));
            corModuleDef.Initialize();
            //corModuleDef.CorLibTypes=new CorLibTypes()
            corModuleDef.Write("ss");
            throw new NotImplementedException();
        }

        private ClrModule GetClrModule(IntPtr moduleHandle)
        {
            DataTarget dataTarget;

            using (dataTarget = DataTarget.AttachToProcess((int)_processId, 10000, AttachFlag.Passive))
                foreach (ClrInfo clrVersion in dataTarget.ClrVersions)
                    foreach (ClrModule clrModule in clrVersion.CreateRuntime().Modules)
                        if ((IntPtr)clrModule.ImageBase == moduleHandle)
                            return clrModule;
            return null;
        }

        public override int DumpProcess(string directoryPath)
        {
            throw new NotImplementedException();
        }
    }
}
