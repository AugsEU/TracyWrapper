using bottlenoselabs.C2CS.Runtime;
using System.Drawing;
using System.Runtime.CompilerServices;
using Tracy;

namespace TracyWrapper
{
	public static class Profiler
	{
		private static Stack<PInvoke.TracyCZoneContext> mScopeStack = new Stack<PInvoke.TracyCZoneContext>();
		private static bool mEnabled = true;


		/// <summary>
		/// Turn profiler on/off.
		/// </summary>
		/// <param name="enabled">Set to true to enable profiler.</param>
		/// <exception cref="Exception">Cannot disable profiler while profiling scopes are pushed.</exception>
		public static void SetEnabled(bool enabled)
		{
			if(!enabled && mScopeStack.Count > 0)
			{
				throw new Exception("Cannot disable profiler while profiling scopes are pushed. Consider turning this off between frames");
			}

			mEnabled = enabled;
		}



		/// <summary>
		/// This needs to be called once every frame.
		/// </summary>
		/// <param name="name">Display name</param>
		public static void HeartBeat(string name = "Frame")
		{
			if (!mEnabled) return;

			PInvoke.TracyEmitFrameMarkStart(CString.FromString(name));
		}



		/// <summary>
		/// Begin profile region.
		/// </summary>
		/// <param name="name">Display name</param>
		/// <param name="color">Display color</param>
		/// <param name="lineNumber">Override line number. Recommended to leave blank for caller's line number.</param>
		/// <param name="function">Override function name. Recommended to leave blank for caller's function name.</param>
		/// <param name="sourceFile">Override source file name. Recommended to leave blank for caller's source file name.</param>
		public static void PushProfileZone(string name, Color? color = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "")
		{
			if (!mEnabled) return;

			ulong srcloc = PInvoke.TracyAllocSrclocName(
								(uint)lineNumber,
								CString.FromString(sourceFile),
								(ulong)sourceFile.Length,
								CString.FromString(function),
								(ulong)function.Length,
								CString.FromString(name),
								(ulong)name.Length);

			PInvoke.TracyCZoneContext ctx = PInvoke.TracyEmitZoneBeginAlloc(srcloc, 1);

			if (color.HasValue)
			{
				PInvoke.TracyEmitZoneColor(ctx, GetTracyColorUInt(color.Value));
			}

			mScopeStack.Push(ctx);
		}



		/// <summary>
		/// End previous profile region.
		/// </summary>
		public static void PopProfileZone()
		{
			if (!mEnabled) return;

			PInvoke.TracyCZoneContext ctx = mScopeStack.Pop();

			PInvoke.TracyEmitZoneEnd(ctx);
		}



		/// <summary>
		/// Utility function to get tracy color uint format from Microsoft Color
		/// </summary>
		private static uint GetTracyColorUInt(Color color)
		{
			return (uint)color.ToArgb();
		}
	}



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
		public ProfileScope(string name, Color color, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "")
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
			Profiler.PushProfileZone(name, null, lineNumber, function, sourceFile);
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
