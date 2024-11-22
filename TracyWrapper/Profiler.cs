using bottlenoselabs.C2CS.Runtime;
using System.Runtime.CompilerServices;
using System.Drawing;
using Tracy;

namespace TracyWrapper
{
	/// <summary>
	/// Application's profiler. Use this to interact with Tracy
	/// </summary>
	public static class Profiler
	{
		#region rMembers

		// These are threadlocal. But we add an initialiser to stop a warning. But! The initialiser is ignored, calling InitThread is required for each thread.
		[ThreadStatic] 
		private static Stack<PInvoke.TracyCZoneContext> mScopeStack = new Stack<PInvoke.TracyCZoneContext>();

		[ThreadStatic]
		private static bool mEnabled;

		#endregion rMembers





		#region rInit

		/// <summary>
		/// Call this once per thread before starting the profiler.
		/// </summary>
		/// <param name="threadName">Set this for a custom thread display name.</param>
		public static void InitThread(string? threadName = null)
		{
			// Init
			mScopeStack = new Stack<PInvoke.TracyCZoneContext>();
			mEnabled = true;

			// Set thread name
			if (threadName is null)
			{
				threadName = Thread.CurrentThread.Name;

				if (threadName is null)
				{
					threadName = string.Format("Thread_{0}", Thread.CurrentThread.ManagedThreadId);
				}
			}

			SetThreadName(threadName);
		}



		/// <summary>
		/// Inform tracy of custom thread name
		/// </summary>
		/// <param name="name"></param>
		private static void SetThreadName(string name)
		{
			PInvoke.TracySetThreadName(CString.FromString(name));
		}



		/// <summary>
		/// Turn profiler on/off.
		/// </summary>
		/// <param name="enabled">Set to true to enable profiler.</param>
		/// <exception cref="Exception">Cannot disable profiler while profiling scopes are pushed.</exception>
		public static void SetEnabled(bool enabled)
		{
			if (!enabled && mScopeStack.Count > 0)
			{
				throw new Exception("Cannot disable profiler while profiling scopes are pushed. Consider turning this off between frames");
			}

			mEnabled = enabled;
		}

		#endregion rInit





		#region rFrame

		/// <summary>
		/// This needs to be called once every frame.
		/// </summary>
		/// <param name="name">Display name</param>
		public static void HeartBeat(string name = "Frame")
		{
			if (!mEnabled) return;

			PInvoke.TracyEmitFrameMark(CString.FromString(name));
		}

		#endregion rFrame





		#region rCPUZones

		/// <summary>
		/// Begin profile region.
		/// </summary>
		/// <param name="name">Display name</param>
		/// <param name="color">Display color</param>
		/// <param name="lineNumber">Override line number. Recommended to leave blank for caller's line number.</param>
		/// <param name="function">Override function name. Recommended to leave blank for caller's function name.</param>
		/// <param name="sourceFile">Override source file name. Recommended to leave blank for caller's source file name.</param>
		public static void PushProfileZone(string name, uint color = ZoneC.DEFAULT, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "")
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

			if(color != ZoneC.DEFAULT)
			{
				PInvoke.TracyEmitZoneColor(ctx, color);
			}

			mScopeStack.Push(ctx);
		}



		/// <summary>
		/// Begin profile region.
		/// </summary>
		/// <param name="name">Display name</param>
		/// <param name="color">Display color</param>
		/// <param name="lineNumber">Override line number. Recommended to leave blank for caller's line number.</param>
		/// <param name="function">Override function name. Recommended to leave blank for caller's function name.</param>
		/// <param name="sourceFile">Override source file name. Recommended to leave blank for caller's source file name.</param>
		public static void PushProfileZone(string name, Color color, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string function = "", [CallerFilePath] string sourceFile = "")
		{
			if (!mEnabled) return;

			PushProfileZone(name, (uint)color.ToArgb(), lineNumber, function, sourceFile);
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

		#endregion rCPUZones
	}
}
