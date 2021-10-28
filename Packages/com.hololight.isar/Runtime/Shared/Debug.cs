#if UNITY_2018_4_OR_NEWER
#define USING_UNITY
#endif

//#define UNITY_ASSERTIONS

using System.Runtime.CompilerServices;

namespace HoloLight.Isar.Shared
{
	internal static class Fmt
	{
		public static readonly string LogFormat = "{0}({1}): [{2}] {3}";
		//public static readonly string LogFormatExtended = "{0}({1}): [{2}] {3}\n{4}";
	}

	internal static class Debug
	{
		public static void Log(string message,
		                       [CallerMemberName] string func = "",
		                       [CallerFilePath] string file = "",
		                       [CallerLineNumber] int line = 0)
		{
#if USING_UNITY
			UnityEngine.Debug.Log(message);
#endif
			//System.Diagnostics.Debug.WriteLine(message);
			//System.Diagnostics.Debug.WriteLine(message + "\n" + System.Environment.StackTrace);

			System.Diagnostics.Debug.WriteLine(Fmt.LogFormat, /*Path.GetFileName(*/file/*)*/, line, func, message);
		}

		public static void LogWarning(string message,
		                              [CallerMemberName] string func = "",
		                              [CallerFilePath] string file = "",
		                              [CallerLineNumber] int line = 0)
		{
			System.Diagnostics.Debug.WriteLine(Fmt.LogFormat, file, line, func, "Warning: " + message + "\n" + System.Environment.StackTrace);
#if USING_UNITY
			UnityEngine.Debug.LogWarning(message);
#endif
		}

		public static void LogError(string message,
	                                [CallerFilePath] string file = "",
	                                [CallerMemberName] string func = "",
	                                [CallerLineNumber] int line = 0)
		{
#if USING_UNITY
			System.Diagnostics.Debug.WriteLine(Fmt.LogFormat, "Error: " + message + "\n" + System.Environment.StackTrace);
			UnityEngine.Debug.LogError(message);
#else
			System.Diagnostics.Debug.Fail(System.String.Format(Fmt.LogFormat, file, line, func, "Error: " + message + "\n" + System.Environment.StackTrace));
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
			System.Diagnostics.Trace.Assert(condition);
			//System.Diagnostics.Debug.WriteLine("Assertion failed" + "\n" + System.Environment.StackTrace);
			//UnityEngine.Assertions.Assert.IsTrue(condition);
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

	internal static class Trace
	{
		public static void Log(string message,
		                       [CallerMemberName] string func = "",
		                       [CallerFilePath] string file = "",
		                       [CallerLineNumber] int line = 0)
		{
			System.Diagnostics.Trace.WriteLine(System.String.Format(Fmt.LogFormat, /*Path.GetFileName(*/file/*)*/, line, func, message + "\n" + System.Environment.StackTrace));
#if USING_UNITY
			UnityEngine.Debug.Log(message);
#endif
		}

		public static void LogWarning(string message,
		                       [CallerMemberName] string func = "",
		                       [CallerFilePath] string file = "",
		                       [CallerLineNumber] int line = 0)
		{
			System.Diagnostics.Trace.WriteLine(System.String.Format(Fmt.LogFormat, /*Path.GetFileName(*/file/*)*/, line, func, "Warning: " + message + "\n" + System.Environment.StackTrace));
#if USING_UNITY
			UnityEngine.Debug.Log(message);
#endif
		}

		public static void LogError(string message,
		                       [CallerMemberName] string func = "",
		                       [CallerFilePath] string file = "",
		                       [CallerLineNumber] int line = 0)
		{
#if USING_UNITY
			System.Diagnostics.Trace.WriteLine(System.String.Format(Fmt.LogFormat, /*Path.GetFileName(*/file/*)*/, line, func, "Error: " + message + "\n" + System.Environment.StackTrace));
			UnityEngine.Debug.LogError(message);
#else
			System.Diagnostics.Trace.Fail(System.String.Format(Fmt.LogFormat, /*Path.GetFileName(*/file/*)*/, line, func, "Fail: " + message));
#endif
		}

	}

}
