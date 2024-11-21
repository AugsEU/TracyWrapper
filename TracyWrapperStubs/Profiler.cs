using System.Drawing;
using System.Runtime.CompilerServices;
namespace TracyWrapper 
{

	public static class Profiler
	{
		public static void SetEnabled(bool enabled) { }

		public static void HeartBeat(string name = "Frame") { }

		public static void PushProfileZone(string name, Color? color = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "") { }

		public static void PopProfileZone() { }

	}

	public class ProfileScope : IDisposable
	{
		public ProfileScope(string name, Color color, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "") { }

		public ProfileScope(string name, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "") { }

		public void Dispose() { }

	}
}