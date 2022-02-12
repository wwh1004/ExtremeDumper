using System.Reflection;
using System.Text;

namespace System.Extensions;

static class ExceptionExtensions {
	public static Exception GetInmostException(this Exception exception) {
		if (exception is null)
			throw new ArgumentNullException(nameof(exception));

		return exception.InnerException is null ? exception : exception.InnerException.GetInmostException();
	}

	public static string ToFullString(this Exception exception) {
		if (exception is null)
			throw new ArgumentNullException(nameof(exception));

		var sb = new StringBuilder();
		DumpException(exception, sb);
		return sb.ToString();
	}

	static void DumpException(Exception exception, StringBuilder sb) {
		exception ??= new ArgumentNullException(nameof(exception), "<No exception object>");
		sb.AppendLine($"Type: {Environment.NewLine}{exception.GetType().FullName}");
		sb.AppendLine($"Message: {Environment.NewLine}{exception.Message}");
		sb.AppendLine($"Source: {Environment.NewLine}{exception.Source}");
		sb.AppendLine($"StackTrace: {Environment.NewLine}{exception.StackTrace}");
		sb.AppendLine($"TargetSite: {Environment.NewLine}{exception.TargetSite}");
		sb.AppendLine("----------------------------------------");
		if (exception.InnerException is not null)
			DumpException(exception.InnerException, sb);
		if (exception is ReflectionTypeLoadException reflectionTypeLoadException) {
			foreach (var loaderException in reflectionTypeLoadException.LoaderExceptions)
				DumpException(loaderException, sb);
		}
	}
}
