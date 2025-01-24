using bottlenoselabs.C2CS.Runtime;
using System.Drawing;
using System.Runtime.CompilerServices;
using Tracy;

namespace TracyWrapper
{
	/// <summary>
	/// Application's profiler. Use this to interact with Tracy
	/// </summary>
	public static class Profiler
	{
		#region rTypes

		struct ScopeInfo(string name, uint lineNum, string func, string sourceFile) : IEquatable<ScopeInfo>
		{
			public string mName = name;
			public uint mLineNumber = lineNum;
			public string mFunction = func;
			public string mSourceFile = sourceFile;

			public override int GetHashCode()
			{
				int hash = 17;
				hash = hash * 31 + mName.GetHashCode();
				hash = hash * 37 + mLineNumber.GetHashCode();
				hash = hash * 41 + mFunction.GetHashCode();
				hash = hash * 43 + mSourceFile.GetHashCode();
				return hash;
			}

			public bool Equals(ScopeInfo other)
			{
				return mName == other.mName &&
					   mLineNumber == other.mLineNumber &&
					   mFunction == other.mFunction &&
					   mSourceFile == other.mSourceFile;
			}

			public override bool Equals(object? obj)
			{
				return obj is ScopeInfo && Equals((ScopeInfo)obj);
			}

			public static bool operator ==(ScopeInfo left, ScopeInfo right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(ScopeInfo left, ScopeInfo right)
			{
				return !(left == right);
			}
		}

		struct ScopeInfoCStr(ref ScopeInfo info)
		{
			public CString mName = CString.FromString(info.mName);
			public ulong mNameLen = (ulong)info.mName.Length;

			public uint mLineNumber = info.mLineNumber;

			public CString mFunction = CString.FromString(info.mFunction);
			public ulong mFunctionLen = (ulong)info.mFunction.Length;

			public CString mSourceFile = CString.FromString(info.mSourceFile);
			public ulong mSourceFileLen = (ulong)info.mSourceFile.Length;

			public ulong AllocSrcloc()
			{
				return PInvoke.TracyAllocSrclocName(
								mLineNumber,
								mSourceFile,
								mSourceFileLen,
								mFunction,
								mFunctionLen,
								mName,
								mNameLen);
			}
		}

		enum ConnectionStatus
		{
			Connected,
			Disconnected
		}

		#endregion rTypes





		#region rMembers

		// These are threadlocal. But we add an initialiser to stop a warning. But! The initialiser is ignored, calling InitThread is required for each thread.
		[ThreadStatic]
		private static Stack<PInvoke.TracyCZoneCtx> mScopeStack = new();

		[ThreadStatic]
		private static bool mEnabled;

		[ThreadStatic]
		private static ConnectionStatus mConnectionStatus;

		[ThreadStatic]
		private static Dictionary<ScopeInfo, ScopeInfoCStr> mScopeInfoToStrAllocs = new();

		#endregion rMembers





		#region rInit

		/// <summary>
		/// Call this once per thread before starting the profiler.
		/// </summary>
		/// <param name="threadName">Set this for a custom thread display name.</param>
		public static void InitThread(string? threadName = null)
		{
			// Init
			mScopeStack = new();
			mScopeInfoToStrAllocs = new();
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





		#region rUtils

		/// <summary>
		/// This needs to be called once every frame.
		/// </summary>
		/// <param name="name">Display name</param>
		public static void HeartBeat(string name = "Frame")
		{
			if (!mEnabled) return;

			PInvoke.TracyEmitFrameMark(CString.FromString(name));
		}



		/// <summary>
		/// Check if we are connected.
		/// </summary>
		private static void RefreshConnectionStatus()
		{
			mConnectionStatus = PInvoke.TracyConnected() != 0 ? ConnectionStatus.Connected : ConnectionStatus.Disconnected;
		}

		public static void SleepUntilConnected()
		{
			while(mConnectionStatus == ConnectionStatus.Disconnected)
			{
				RefreshConnectionStatus();
			}
		}

		#endregion rUtils





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

			if (mScopeStack.Count == 0)
			{
				// We only refresh this when the scope is zero. Otherwise it could connect halfway through an active block.
				RefreshConnectionStatus();
			}

			switch (mConnectionStatus)
			{
				case ConnectionStatus.Connected:
				{
					ScopeInfo info = new ScopeInfo(name, (uint)lineNumber, function, sourceFile);

					ScopeInfoCStr infoCStr;
					if (!mScopeInfoToStrAllocs.TryGetValue(info, out infoCStr))
					{
						infoCStr = new ScopeInfoCStr(ref info);
						mScopeInfoToStrAllocs.Add(info, infoCStr);
					}
					ulong srcLocAlloc = infoCStr.AllocSrcloc();

					PInvoke.TracyCZoneCtx ctx = PInvoke.TracyEmitZoneBeginAlloc(srcLocAlloc, 1);

					if (color != ZoneC.DEFAULT)
					{
						PInvoke.TracyEmitZoneColor(ctx, color);
					}

					mScopeStack.Push(ctx);
					break;
				}
				case ConnectionStatus.Disconnected:
				{
					// Push dummy data.
					mScopeStack.Push(new PInvoke.TracyCZoneCtx());
					break;
				}
			}
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

			PInvoke.TracyCZoneCtx ctx = mScopeStack.Pop();

			switch (mConnectionStatus)
			{
				case ConnectionStatus.Connected:
					PInvoke.TracyEmitZoneEnd(ctx);
					break;
				case ConnectionStatus.Disconnected:
					// Do nothing with dummy data.
					break;
			}
		}

		#endregion rCPUZones
	}
}
