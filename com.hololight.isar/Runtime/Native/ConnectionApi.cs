/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;
using System.Runtime.InteropServices;
using HoloLight.Isar.Native.Input;
using HoloLight.Isar.Native.Qr;
using UnityEngine.Assertions;

namespace HoloLight.Isar.Native
{
	public enum PluginEventId
	{
		kPushFrame = 0,             // TODO(viktor): = 2
		kInitVideo = 1,             // TODO(viktor): = 1
		kGetGraphicsDeviceInfo = 2  // TODO(viktor): = 0
	}

	//KL: add Hlr prefix to all types that represent raw C bindings so they have the same names on both C and C# sides.
	internal struct ConnectionApi
	{
		/* Connection */

		public Init Init;
		public Close Close;
		public Reset Reset;
		public InitVideoTrack InitVideoTrack;
		public PushFrame PushFrame;

		public HlrSvRegisterConnectionStateHandler RegisterConnectionStateHandler;
		public HlrSvUnregisterConnectionStateHandler UnregisterConnectionStateHandler;
		public HlrSvRegisterViewPoseHandler RegisterViewPoseHandler;
		public HlrSvUnregisterViewPoseHandler UnregisterViewPoseHandler;
		public HlrSvRegisterInputEventHandler RegisterInputEventHandler;
		public HlrSvUnregisterInputEventHandler UnregisterInputEventHandler;

		/* Data */
		//KL: make this a struct QrApi because that's essentially what it is
		public RegisterMessageCallbacks RegisterMessageCallbacks;
		public ProcessMessages ProcessMessages;
		public QrIsSupported QrIsSupported;
		public QrRequestAccess QrRequestAccess;
		public QrStart QrStart;
		public QrStop QrStop;
		//public QrGetList QrGetList;
	}

	#region Connection

	internal struct ConnectionCallbacks
	{
		// All the delegates in the original code are unsafe.
		// Then again, it could be good for performance.
		// Also, ref returns could be useful.
		// https://blogs.msdn.microsoft.com/mazhou/2017/12/12/c-7-series-part-7-ref-returns/
		// https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke
		internal delegate void ConnectionStateChangedCallback(ConnectionState newState, IntPtr userData);
		private readonly ConnectionStateChangedCallback _connectionStateChangedCallback;

		// About string marshalling: SDP spec says it's UTF-8 unless there's a "charset=X" in the desc.
		// There is UnmanagedType.LPUTF8Str, but not in this version of .NET Standard, so we can't use it.
		// Default marshaling is LPStr, i.e. const char* to ANSI. I guess it should be ok, but in case it's not,
		// this friendly comment reminds you that string encoding is hard.
		internal delegate void SdpCreatedCallback(SdpType type, string sdp);
		private readonly SdpCreatedCallback _sdpCreatedCallback;

		internal delegate void LocalIceCandidateCreatedCallback(string mId, int mLineIndex, string candidate);
		private readonly LocalIceCandidateCreatedCallback _localIceCandidateCreatedCallback;

		public ConnectionCallbacks(
			ConnectionStateChangedCallback connectionStateChangedCallback,
			SdpCreatedCallback sdpCreatedCallback,
			LocalIceCandidateCreatedCallback localIceCandidateCreatedCallback)
		{
			//Assert.AreNotEqual(null, connectionStateChangedCallback,   "null is not a valid callback");
			//Assert.AreNotEqual(null, sdpCreatedCallback,               "null is not a valid callback");
			//Assert.AreNotEqual(null, localIceCandidateCreatedCallback, "null is not a valid callback");
			_connectionStateChangedCallback = connectionStateChangedCallback;
			_sdpCreatedCallback = sdpCreatedCallback;
			_localIceCandidateCreatedCallback = localIceCandidateCreatedCallback;
		}
	}
	internal delegate Error Init(
		[MarshalAs(UnmanagedType.LPStr)] string configPath, GraphicsApiConfig gfxConfig,
		ConnectionCallbacks callbacks, ref /*out*/ ConnectionHandle connectionHandle);
	internal delegate Error Close(ref IntPtr connectionHandle);
	internal delegate Error Reset(ConnectionHandle connectionHandle);

	#endregion Connection

	#region Video

	internal delegate void GraphicsDeviceInfoReceivedCallback(GraphicsDeviceInfo deviceInfo);
	internal delegate IntPtr GetGraphicsDeviceInfo(IntPtr renderTarget, GraphicsDeviceInfoReceivedCallback callback);

	internal delegate void VideoTrackInitializedCallback(Error err);
	internal delegate IntPtr InitVideoTrack(
		ConnectionHandle connectionHandle, GraphicsApiConfig gfxConfig, VideoTrackInitializedCallback callback);

	internal delegate IntPtr PushFrame(ConnectionHandle connectionHandle, GraphicsApiFrame videoFrame);

	#endregion Video

	#region Data

	//[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	//internal delegate IntPtr InitVideoTrack(
	//	ConnectionHandle connectionHandle,
	//	GraphicsApiConfig gfxConfig,
	//	VideoTrackInitializedCallback callback);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void HlrSvRegisterConnectionStateHandler(ConnectionHandle handle, ConnectionStateChangedCallback cb);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void HlrSvUnregisterConnectionStateHandler(ConnectionHandle handle, ConnectionStateChangedCallback cb);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void HlrSvRegisterViewPoseHandler(ConnectionHandle handle, ViewPoseReceivedCallback cb);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void HlrSvUnregisterViewPoseHandler(ConnectionHandle handle, ViewPoseReceivedCallback cb);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void HlrSvRegisterInputEventHandler(ConnectionHandle handle, InputEventReceivedCallback cb);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate void HlrSvUnregisterInputEventHandler(ConnectionHandle handle, InputEventReceivedCallback cb);

	internal delegate void ViewPoseReceivedCallback(in StereoViewPose pose/*, IntPtr userData*/);
	internal delegate void InputEventReceivedCallback(in InputEvent pose/*, IntPtr userData*/);

	internal delegate void QrIsSupportedCallback(in QrIsSupportedEventArgs args/*, IntPtr userData*/);
	internal delegate void QrRequestAccessCallback(in QrRequestAccessEventArgs args/*, IntPtr userData*/);
	internal delegate void QrAddedCallback(in QrAddedEventArgs args/*, IntPtr userData*/);
	internal delegate void QrUpdatedCallback(in QrUpdatedEventArgs args/*, IntPtr userData*/);
	internal delegate void QrRemovedCallback(in QrRemovedEventArgs args/*, IntPtr userData*/);
	internal delegate void QrEnumerationCompletedCallback(/*IntPtr userData*/);

	// Hololens 2
	internal readonly struct MessageCallbacks
	{
		private readonly ViewPoseReceivedCallback _viewPoseReceivedCallback;
		private readonly InputEventReceivedCallback _inputEventReceivedCallback;

		private readonly QrIsSupportedCallback _qrIsSupportedCallback;
		private readonly QrRequestAccessCallback _qrRequestAccessCallback;
		private readonly QrAddedCallback _qrAddedCallback;
		private readonly QrUpdatedCallback _qrUpdatedCallback;
		private readonly QrRemovedCallback _qrRemovedCallback;
		private readonly QrEnumerationCompletedCallback _qrEnumerationCompletedCallback;

		public MessageCallbacks(
			ViewPoseReceivedCallback viewPoseReceivedCallback,
			InputEventReceivedCallback inputEventReceivedCallback,
			QrIsSupportedCallback qrIsSupportedCallback,
			QrRequestAccessCallback qrRequestAccessCallback,
			QrAddedCallback qrAddedCallback,
			QrUpdatedCallback qrUpdatedCallback,
			QrRemovedCallback qrRemovedCallback,
			QrEnumerationCompletedCallback qrEnumerationCompletedCallback
		)
		{
			_viewPoseReceivedCallback = viewPoseReceivedCallback;
			_inputEventReceivedCallback = inputEventReceivedCallback;
			_qrIsSupportedCallback = qrIsSupportedCallback;
			_qrRequestAccessCallback = qrRequestAccessCallback;
			_qrAddedCallback = qrAddedCallback;
			_qrUpdatedCallback = qrUpdatedCallback;
			_qrRemovedCallback = qrRemovedCallback;
			_qrEnumerationCompletedCallback = qrEnumerationCompletedCallback;
		}
	}

	internal delegate void RegisterMessageCallbacks(ConnectionHandle connectionHandle, ref MessageCallbacks callbacks);
	internal delegate void ProcessMessages(ConnectionHandle connectionHandle);
	internal delegate Error QrIsSupported(ConnectionHandle connectionHandle);
	internal delegate Error QrRequestAccess(ConnectionHandle connectionHandle);
	internal delegate Error QrStart(ConnectionHandle connectionHandle);
	internal delegate Error QrStop(ConnectionHandle connectionHandle);
	//internal delegate Error QrGetList(ConnectionHandle connectionHandle);


	#endregion Data
}