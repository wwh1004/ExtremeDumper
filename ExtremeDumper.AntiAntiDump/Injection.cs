using System;
using System.Extensions;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace ExtremeDumper.AntiAntiDump;

public static class Injection {
	public const int Success = 0;
	public const int Failure = -1;
	public const int InvalidArgument = -2;
	public const int UnhandledException = -3;

	public static int Main(string arg) {
		RunAADServerAsync(arg);
		return Success;
	}

	public static void RunAADServerAsync(string pipeName) {
		new Thread(() => RunAADServer(pipeName)) { IsBackground = true }.Start();
	}

	[HandleProcessCorruptedStateExceptions]
	public static int RunAADServer(string pipeName) {
		try {
			var server = AADServer.Create(pipeName);
			if (server is null) {
				TryMessageBox("Can't create AADServer instance.");
				return InvalidArgument;
			}

			server.Listen();
			return Success;
		}
		catch (Exception ex) {
			TryMessageBox(ex.ToFullString());
			return UnhandledException;
		}
	}

	static void Server_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
		if (e.ExceptionObject is Exception exception)
			TryMessageBox(exception.ToFullString());
	}

	static bool TryMessageBox(string message) {
		var type = Type.GetType("System.Windows.Forms.MessageBox, System.Windows.Forms, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false);
		if (type is null)
			return false;

		var method = type.GetMethod("Show", new[] { typeof(string) });
		if (method is null)
			return false;

		method.Invoke(null, new object[] { message });
		return true;
	}
}
