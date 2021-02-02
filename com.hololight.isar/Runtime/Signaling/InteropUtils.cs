#if UNITY_2018_4_OR_NEWER
#define USING_UNITY
#endif

namespace HoloLight.Isar.Signaling
{
	internal static class Debug
	{
		public static void Log(string message)
		{
#if USING_UNITY
			UnityEngine.Debug.Log(message);
#endif
			System.Diagnostics.Debug.WriteLine(message);
			//System.Diagnostics.Debug.WriteLine(message + "\n" + System.Environment.StackTrace);
		}

		public static void LogError(string message)
		{
#if USING_UNITY
			UnityEngine.Debug.LogError(message);
			System.Diagnostics.Debug.WriteLine(message + "\n" + System.Environment.StackTrace);
#else
			System.Diagnostics.Debug.Fail(message);
#endif
		}

		public static void Assert(bool condition)
		{
#if USING_UNITY
			if (condition) return;
			LogError("Assertion failed");
			//UnityEngine.Debug.Assert(condition);
			if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
#else
			System.Diagnostics.Debug.Assert(condition);
#endif
		}

		public static void Assert(bool condition, string message)
		{
#if USING_UNITY
			if (condition) return;
			LogError(message);
			if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
#else
			System.Diagnostics.Debug.Assert(condition, message);
#endif
		}
	}

	public static class Assertions
	{
		public static void Assert(bool condition)
		{
#if USING_UNITY
			System.Diagnostics.Debug.WriteLine("Assertion failed" + "\n" + System.Environment.StackTrace);
			UnityEngine.Assertions.Assert.IsTrue(condition);
#else
			System.Diagnostics.Trace.Assert(condition);
#endif
		}

		public static void Assert(bool condition, string message)
		{
#if USING_UNITY
			if (condition) return;
			//UnityEngine.Assertions.Assert.Fail(UnityEngine.Assertions.AssertionMessageUtil.BooleanFailureMessage(true), message);
			System.Diagnostics.Debug.WriteLine(message + "\n" + System.Environment.StackTrace);
			UnityEngine.Assertions.Assert.IsTrue(condition, message);
#else
			System.Diagnostics.Trace.Assert(condition, message);
#endif
		}
	}

}
