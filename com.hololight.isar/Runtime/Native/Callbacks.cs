/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;
using System.Runtime.InteropServices;
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
	internal static class Callbacks
	{
		internal static event Action<ConnectionState> ConnectionStateChanged;
		[MonoPInvokeCallback(typeof(ConnectionCallbacks.ConnectionStateChangedCallback))]
		internal static void OnConnectionStateChanged(ConnectionState newState, IntPtr userData)
		{
			try
			{
				Debug.Log($"Remoting connection state changed to {newState}");
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
		internal static event SdpCreatedCallback SdpCreated;
		[MonoPInvokeCallback(typeof(ConnectionCallbacks.SdpCreatedCallback))]
		internal static void OnSdpCreated(SdpType type, string sdp)
		{
			try
			{
				SdpCreated?.Invoke(type, sdp);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Fired when a local ICE candidate is created.
		/// </summary>
		internal static event LocalIceCandidateCreatedCallback LocalIceCandidateCreated;
		[MonoPInvokeCallback(typeof(ConnectionCallbacks.LocalIceCandidateCreatedCallback))]
		internal static void OnLocalIceCandidateCreated(string mId, int mLineIndex, string candidate)
		{
			try
			{
				LocalIceCandidateCreated?.Invoke(mId, mLineIndex, candidate);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		/// <summary>
		/// Fired when we receive a new XR pose from the client.
		/// </summary>
		internal delegate void ViewPoseHandler(in StereoViewPose viewPose);
		internal static event ViewPoseHandler ViewPoseReceived;
		[MonoPInvokeCallback(typeof(ViewPoseReceivedCallback))]
		internal static void OnViewPoseReceived(in StereoViewPose pose)
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
		internal delegate void InputEventHandler(in InputEvent inputEvent);
		internal static event InputEventHandler InputEventReceived;
		[MonoPInvokeCallback(typeof(InputEventReceivedCallback))]
		internal static void OnInputEventReceived(in InputEvent input)
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
	}

	// All the delegates in the original code are unsafe.
	// Then again, it could be good for performance.
	// Also, ref returns could be useful.
	// https://blogs.msdn.microsoft.com/mazhou/2017/12/12/c-7-series-part-7-ref-returns/
	// https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ConnectionStateChangedCallback(ConnectionState newState, IntPtr userData);

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
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void SdpCreatedCallback(SdpType type, string sdp);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void LocalIceCandidateCreatedCallback(string sdpMline, int mlineIndex, string sdpizedCandidate);
}