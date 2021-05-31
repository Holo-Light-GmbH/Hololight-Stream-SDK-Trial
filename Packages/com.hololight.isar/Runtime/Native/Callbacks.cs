/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;
using AOT;
using HoloLight.Isar.Native.Input;
using UnityEngine;

namespace HoloLight.Isar.Native
{
	/// <summary>
	/// Callbacks that come from native code; these need to be static because
	/// IL2CPP does not support instance delegates, i.e. the callee invoked by native code
	/// needs to be a static method.
	/// These methods also need to be marked with MonoPInvokeCallback for IL2CPP to correctly
	/// work and it is *very* important to wrap everything inside a try/catch block,
	/// otherwise a C# exception might cause undefined behavior.
	/// If you're curious, here's more info:
	/// https://www.mono-project.com/docs/advanced/pinvoke/#runtime-exception-propagation
	/// </summary>
	// TODO: Callbacks should be a wrapper around ServerApi and handle passing the connectionHandle to the native side by delegating all api functions
	// including static (and non static?) event handlers etc.
	public static class Callbacks
	{
		internal static event Action<HlrConnectionState> ConnectionStateChanged;
		[MonoPInvokeCallback(typeof(HlrConnectionStateChangedCallback))]
		internal static void OnConnectionStateChanged(HlrConnectionState newState, IntPtr userData)
		{
			try
			{
				// TODO: dispatch on unity thread eg. using UnityMainThreadDispatcher?
				ConnectionStateChanged?.Invoke(newState);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Fired when the local session description is created.
		/// </summary>
		internal static event HlrSdpCreatedCallback SdpCreated;
		[MonoPInvokeCallback(typeof(HlrSdpCreatedCallback))]
		internal static void OnSdpCreated(HlrSdpType type, string sdp, IntPtr userData)
		{
			try
			{
				SdpCreated?.Invoke(type, sdp, userData);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Fired when a local ICE candidate is created.
		/// </summary>
		internal static event HlrLocalIceCandidateCreatedCallback LocalIceCandidateCreated;
		[MonoPInvokeCallback(typeof(HlrLocalIceCandidateCreatedCallback))]
		internal static void OnLocalIceCandidateCreated(string mId, int mLineIndex, string candidate, IntPtr userData)
		{
			try
			{
				LocalIceCandidateCreated?.Invoke(mId, mLineIndex, candidate, userData);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Fired when we receive a new XR pose from the client.
		/// </summary>
		internal delegate void ViewPoseHandler(in HlrXrPose viewPose);
		internal static event ViewPoseHandler ViewPoseReceived;
		[MonoPInvokeCallback(typeof(HlrSvViewPoseReceivedCallback))]
		internal static void OnViewPoseReceived(in HlrXrPose pose, IntPtr userData)
		{
			try
			{
				ViewPoseReceived?.Invoke(in pose);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Fired when we receive a new XR pose from the client.
		/// </summary>
		internal delegate void InputEventHandler(in HlrInputEvent inputEvent);
		internal static event InputEventHandler InputEventReceived;
		[MonoPInvokeCallback(typeof(HlrSvInputEventReceivedCallback))]
		internal static void OnInputEventReceived(in HlrInputEvent input, IntPtr userData)
		{
			try
			{
				InputEventReceived?.Invoke(in input);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Fired when we receive a custom message over DataChannel.
		/// </summary>
		internal static event HlrCustomMessageCallback CustomMessageReceived;
		[MonoPInvokeCallback(typeof(HlrCustomMessageCallback))]
		internal static void OnCustomMessageReceived(in HlrCustomMessage message, IntPtr userData)
		{
			try
			{
				CustomMessageReceived?.Invoke(message, userData);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Fired when we receive audio data from the client.
		/// </summary>
		internal static event HlrSvAudioDataReceivedCallback AudioDataReceived;
		[MonoPInvokeCallback(typeof(HlrSvAudioDataReceivedCallback))]
		internal static void OnAudioDataReceived(in HlrAudioData audioData, IntPtr userData)
		{
			try
			{
				AudioDataReceived?.Invoke(audioData, userData);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
	}

	// All the delegates in the original code are unsafe.
	// Then again, it could be good for performance.
	// Also, ref returns could be useful.
	// https://blogs.msdn.microsoft.com/mazhou/2017/12/12/c-7-series-part-7-ref-returns/
	// https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke
	internal delegate void HlrConnectionStateChangedCallback(HlrConnectionState newState, IntPtr userData);

	// using ref instead of pointers.
	// https://manski.net/2012/06/pinvoke-tutorial-passing-parameters-part-3/#marshalling-structs
	// [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	// internal delegate void ViewPoseReceivedCallback(ref StereoViewPose pose, IntPtr userData);

	// [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	// internal delegate void InputEventReceivedCallback(ref InputEvent pose, IntPtr userData);

	// About string marshalling: SDP spec says it's UTF-8 unless there's a "charset=X" in the desc.
	// There is UnmanagedType.LPUTF8Str, but not in this version of .NET standard, so we can't use it.
	// Default marshaling is LPStr, i.e. const char* to ANSI. I guess it should be ok, but in case it's not,
	// this friendly comment reminds you that string encoding is hard.
	internal delegate void HlrSdpCreatedCallback(HlrSdpType type, string sdp, IntPtr userData);

	internal delegate void HlrLocalIceCandidateCreatedCallback(string sdpMline, int mlineIndex, string sdpizedCandidate, IntPtr userData);
}