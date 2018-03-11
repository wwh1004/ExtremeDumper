using System;
using System.IO;
using dndbg.DotNet;
using dndbg.Engine;
using dnlib.DotNet;
using Microsoft.Diagnostics.Runtime;

namespace ExtremeDumper.MetadataDumper
{
    public class DbgDumper : IDumper
    {
        private uint _processId;

        public DbgDumper(uint processId) => _processId = processId;

        public bool DumpModule(IntPtr moduleHandle, string filePath)
        {
            string clrVersion;
            DnDebugger dnDebugger;
            CorModuleDef corModuleDef;

            clrVersion = GetModuleCLRVersion(moduleHandle);
            if (string.IsNullOrEmpty(clrVersion))
                return false;
            using (dnDebugger = DnDebugger.Attach(new AttachProcessOptions(new DesktopCLRTypeAttachInfo(clrVersion)) { DebugMessageDispatcher = EmptyDebugMessageDispatcher.Instance, ProcessId = (int)_processId }))
            {
                corModuleDef = dnDebugger.GetCorModuleDef(moduleHandle);
                if (corModuleDef == null)
                    return false;
                corModuleDef.Write(string.IsNullOrEmpty(Path.GetExtension(filePath)) ? filePath + (corModuleDef.Kind == ModuleKind.Console || corModuleDef.Kind == ModuleKind.Windows ? ".exe" : ".dll") : filePath);
                dnDebugger.TryDetach();
            }
            return true;
        }

        private string GetModuleCLRVersion(IntPtr moduleHandle)
        {
            DataTarget dataTarget;

            using (dataTarget = DataTarget.AttachToProcess((int)_processId, 10000, AttachFlag.Passive))
                foreach (ClrInfo clrInfo in dataTarget.ClrVersions)
                    foreach (ClrModule clrModule in clrInfo.CreateRuntime().Modules)
                        if ((IntPtr)clrModule.ImageBase == moduleHandle)
                            return Path.GetFileName(Path.GetDirectoryName(clrInfo.ModuleInfo.FileName));
            return null;
        }

        public int DumpProcess(string directoryPath)
        {
            throw new NotImplementedException();
        }
    }
}
