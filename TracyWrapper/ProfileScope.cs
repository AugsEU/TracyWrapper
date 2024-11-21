using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TracyWrapper
{
	/// <summary>
	/// Object which can profile a scope automatically.
	/// Use inside a "using" statement.
	/// </summary>
	public class ProfileScope : IDisposable
	{
		/// <summary>
		/// Create a profile scope object.
		/// </summary>
		/// <param name="name">Display name</param>
		/// <param name="color">Display color</param>
		/// <param name="lineNumber">Override line number. Recommended to leave blank for caller's line number.</param>
		/// <param name="function">Override function name. Recommended to leave blank for caller's function name.</param>
		/// <param name="sourceFile">Override source file name. Recommended to leave blank for caller's source file name.</param>
		public ProfileScope(string name, uint color = ZoneC.DEFAULT, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "")
		{
			Profiler.PushProfileZone(name, color, lineNumber, function, sourceFile);
		}



		/// <summary>
		/// Create a profile scope object.
		/// </summary>
		/// <param name="name">Display name</param>
		/// <param name="lineNumber">Override line number. Recommended to leave blank for caller's line number.</param>
		/// <param name="function">Override function name. Recommended to leave blank for caller's function name.</param>
		/// <param name="sourceFile">Override source file name. Recommended to leave blank for caller's source file name.</param>
		public ProfileScope(string name, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "")
		{
			Profiler.PushProfileZone(name, ZoneC.DEFAULT, lineNumber, function, sourceFile);
		}



		/// <summary>
		/// Dispose of the profile scope and end the timing.
		/// </summary>
		public void Dispose()
		{
			Profiler.PopProfileZone();
		}
	}
}
