using System;
using dndbg.Engine;

namespace ExtremeDumper.MetadataDumper
{
    internal class EmptyDebugMessageDispatcher : IDebugMessageDispatcher
    {
        public static readonly EmptyDebugMessageDispatcher Instance = new EmptyDebugMessageDispatcher();

        private EmptyDebugMessageDispatcher()
        {
        }

        public void CancelDispatchQueue(object result)
        {
        }

        public object DispatchQueue(TimeSpan waitTime, out bool timedOut)
        {
            timedOut = false;
            return null;
        }

        public void ExecuteAsync(Action action)
        {
        }
    }
}
