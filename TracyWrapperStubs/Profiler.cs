using System.Drawing;
using System.Runtime.CompilerServices;
namespace TracyWrapper
{

	public static class Profiler
	{

		/// <summary> This method is a stub that does nothing. </summary>
		public static void InitThread(string? threadName = null) { }

		/// <summary> This method is a stub that does nothing. </summary>
		public static void SetEnabled(bool enabled) { }

		/// <summary> This method is a stub that does nothing. </summary>
		public static void HeartBeat(string name = "Frame") { }

		/// <summary> This method is a stub that does nothing. </summary>
		public static void PushProfileZone(string name, uint color = ZoneC.DEFAULT, int lineNumber = 0, string function = "", string sourceFile = "") { }

		/// <summary> This method is a stub that does nothing. </summary>
		public static void PushProfileZone(string name, Color color, int lineNumber = 0, string function = "", string sourceFile = "") { }

		/// <summary> This method is a stub that does nothing. </summary>
		public static void PopProfileZone() { }

	}
}