using System;
using dndbg.COM.MetaData;
using dndbg.DotNet;
using dnlib.DotNet.Writer;
using Microsoft.Diagnostics.Runtime;

namespace ExtremeDumper.MetadataDumper
{
    public class PassiveDumper : IDumper
    {
        private uint _processId;

        public PassiveDumper(uint processId) => _processId = processId;

        public bool DumpModule(IntPtr moduleHandle, string filePath)
        {
            if (moduleHandle == IntPtr.Zero)
                throw new ArgumentException();

            ClrModule clrModule;
            IMetaDataImport mdi;
            CorModuleDef corModuleDef;

            clrModule = GetClrModule(moduleHandle);
            if (clrModule == null)
                return false;
            mdi = (IMetaDataImport)clrModule.MetadataImport;
            if (mdi == null)
                return false;
            //corModuleDef = new CorModuleDef((IMetaDataImport)clrModule.MetadataImport, new PassiveCorModuleDefHelper(_processId, mdi, clrModule) { IsManifestModule = true });
            //corModuleDef.Initialize();
            //corModuleDef.Write(".[IsManifestModule=True].dll");
            corModuleDef = new CorModuleDef(mdi, new MiniCorModuleDefHelper(_processId, mdi, clrModule));
            corModuleDef.Initialize();
            corModuleDef.Write(".[IsManifestModule=False].dll", new ModuleWriterOptions(corModuleDef) { MetaDataOptions = new MetaDataOptions(MetaDataFlags.KeepOldMaxStack) });
            //corModuleDef.Write(filePath);
            return true;
        }

        private ClrModule GetClrModule(IntPtr moduleHandle)
        {
            DataTarget dataTarget;

            using (dataTarget = DataTarget.AttachToProcess((int)_processId, 10000, AttachFlag.Passive))
                foreach (ClrInfo clrInfo in dataTarget.ClrVersions)
                    foreach (ClrModule clrModule in clrInfo.CreateRuntime().Modules)
                        if ((IntPtr)clrModule.ImageBase == moduleHandle)
                        {
                            dataTarget.SelectedClrModule = clrModule;
                            return clrModule;
                        }
            dataTarget.SelectedClrModule = null;
            return null;
        }

        public int DumpProcess(string directoryPath)
        {
            throw new NotImplementedException();
        }
    }
}
