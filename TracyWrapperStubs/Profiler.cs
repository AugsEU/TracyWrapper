using System.Drawing;
using System.Runtime.CompilerServices;
namespace TracyWrapper 
{

	public static class Profiler
	{
		/// <summary> This method is a stub that does nothing. </summary>
		public static void SetEnabled(bool enabled) { }

		/// <summary> This method is a stub that does nothing. </summary>
		public static void HeartBeat(string name = "Frame") { }

		/// <summary> This method is a stub that does nothing. </summary>
		public static void PushProfileZone(string name, Color? color = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "") { }

		/// <summary> This method is a stub that does nothing. </summary>
		public static void PopProfileZone() { }

	}

	public class ProfileScope : IDisposable
	{
		/// <summary> This method is a stub that does nothing. </summary>
		public ProfileScope(string name, Color color, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "") { }

		/// <summary> This method is a stub that does nothing. </summary>
		public ProfileScope(string name, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "") { }

		/// <summary> This method is a stub that does nothing. </summary>
		public void Dispose() { }

	}
}