using System;
using System.Collections.Generic;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// Find all <see cref="AADCommand"/> handler
/// </summary>
static class CommandHandlerManager {
	/// <summary>
	/// All <see cref="AADCommand"/> handlers
	/// </summary>
	public static IDictionary<AADCommand, ICommandHandler> Handlers { get; } = GetHandlers();

	static Dictionary<AADCommand, ICommandHandler> GetHandlers() {
		var handlers = new Dictionary<AADCommand, ICommandHandler>();
		foreach (var type in typeof(ICommandHandler).Module.GetTypes()) {
			if (type.IsAbstract)
				continue;
			foreach (var interfaceType in type.GetInterfaces()) {
				if (interfaceType.IsAssignableFrom(typeof(ICommandHandler))) {
					var handler = (ICommandHandler)Activator.CreateInstance(type, true);
					handlers.Add(handler.Command, handler);
				}
			}
		}
		return handlers;
	}
}
