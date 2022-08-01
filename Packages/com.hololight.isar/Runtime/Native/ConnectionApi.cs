/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;
using System.Runtime.InteropServices;
using HoloLight.Isar.Native.Input;
using HoloLight.Isar.Native.Qr;

namespace HoloLight.Isar.Native
{
	//KL: add Hlr prefix to all types that represent raw C bindings so they have the same names on both C and C# sides.
	internal struct HlrSvConnectionApi
	{
		/* Connection */
		public HlrInit Init;
		public HlrClose Close;
		public HlrSvReset Reset;
		public HlrSvInitVideoTrack InitVideoTrack;
		public HlrSvPushFrame PushFrame;
		public HlrSvPushAudioData PushAudioData;
		public HlrPushCustomMessage PushCustomMessage;
		public HlrSvSetAudioTrackEnabled SetAudioTrackEnabled;

		public HlrSvRegisterConnectionStateHandler RegisterConnectionStateHandler;
		public HlrSvUnregisterConnectionStateHandler UnregisterConnectionStateHandler;
		public HlrSvRegisterViewPoseHandler RegisterViewPoseHandler;
		public HlrSvUnregisterViewPoseHandler UnregisterViewPoseHandler;
		public HlrSvRegisterInputEventHandler RegisterInputEventHandler;
		public HlrSvUnregisterInputEventHandler UnregisterInputEventHandler;
		public HlrSvRegisterSpatialInputHandler RegisterSpatialInputHandler;
		public HlrSvUnregisterSpatialInputHandler UnregisterSpatialInputHandler;
		public HlrRegisterCustomMessageHandler RegisterCustomMessageHandler;
		public HlrUnregisterCustomMessageHandler UnregisterCustomMessageHandler;
		public HlrRegisterAudioDataHandler RegisterAudioDataHandler;
		public HlrUnregisterAudioDataHandler UnregisterAudioDataHandler;

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

	internal struct HlrConnectionCallbacks
	{
		// All the delegates in the original code are unsafe.
		// Then again, it could be good for performance.
		// Also, ref returns could be useful.
		// https://blogs.msdn.microsoft.com/mazhou/2017/12/12/c-7-series-part-7-ref-returns/
		// https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke
		private readonly HlrConnectionStateChangedCallback _connectionStateChangedCallback;

		// About string marshaling: SDP spec says it's UTF-8 unless there's a "charset=X" in the desc.
		// There is UnmanagedType.LPUTF8Str, but not in this version of .NET Standard, so we can't use it.
		// Default marshaling is LPStr, i.e. const char* to ANSI. I guess it should be ok, but in case it's not,
		// this friendly comment reminds you that string encoding is hard.
		private readonly HlrSdpCreatedCallback _sdpCreatedCallback;

		private readonly HlrLocalIceCandidateCreatedCallback _localIceCandidateCreatedCallback;

		public HlrConnectionCallbacks(
			HlrConnectionStateChangedCallback connectionStateChangedCallback,
			HlrSdpCreatedCallback sdpCreatedCallback,
			HlrLocalIceCandidateCreatedCallback localIceCandidateCreatedCallback)
		{
			//Assert.AreNotEqual(null, connectionStateChangedCallback,   "null is not a valid callback");
			//Assert.AreNotEqual(null, sdpCreatedCallback,               "null is not a valid callback");
			//Assert.AreNotEqual(null, localIceCandidateCreatedCallback, "null is not a valid callback");
			_connectionStateChangedCallback = connectionStateChangedCallback;
			_sdpCreatedCallback = sdpCreatedCallback;
			_localIceCandidateCreatedCallback = localIceCandidateCreatedCallback;
		}
	}

	internal delegate HlrError HlrInit(
		[MarshalAs(UnmanagedType.LPStr)] string configPath, HlrGraphicsApiConfig gfxConfig,
		HlrConnectionCallbacks callbacks, ref /*out*/ HlrHandle connectionHandle);
	internal delegate HlrError HlrClose(ref IntPtr connectionHandle);
	internal delegate HlrError HlrSvReset(HlrHandle connectionHandle);
	internal delegate HlrError HlrSvSetAudioTrackEnabled(HlrHandle connectionHandle, int enabled);

	#endregion Connection

	#region Audio
	// TODO: use nuint from C# 9 once we drop Unity 2019
	// ref: https://stackoverflow.com/questions/32906774/what-is-equal-to-the-c-size-t-in-c-sharp
	// ref: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nint-nuint
	// ref:
	public struct HlrAudioData
	{
		public IntPtr Data;
		public int BitsPerSample;
		public int SampleRate;
		public UIntPtr /*size_t*/ NumberOfChannels;
		public UIntPtr /*size_t*/ SamplesPerChannel;
	}

	internal delegate HlrError HlrSvPushAudioData(HlrHandle connectionHandle, HlrAudioData audioData);

	#endregion

	#region Video

	internal delegate HlrError HlrSvInitVideoTrack(HlrHandle connectionHandle, HlrGraphicsApiConfig gfxConfig);

	internal delegate HlrError HlrSvPushFrame(HlrHandle connectionHandle, HlrGraphicsApiFrame videoFrame);

	#endregion Video

	#region Data

	internal delegate HlrError HlrPushCustomMessage(HlrHandle handle, HlrCustomMessage message);

	public struct HlrCustomMessage
	{
		public uint Length;
		public IntPtr Data;
	}

	// Callback registration delegates
	internal delegate void HlrSvRegisterConnectionStateHandler(HlrHandle handle, HlrConnectionStateChangedCallback cb, IntPtr userData);
	internal delegate void HlrSvUnregisterConnectionStateHandler(HlrHandle handle, HlrConnectionStateChangedCallback cb);
	internal delegate void HlrSvRegisterViewPoseHandler(HlrHandle handle, HlrSvViewPoseReceivedCallback cb, IntPtr userData);
	internal delegate void HlrSvUnregisterViewPoseHandler(HlrHandle handle, HlrSvViewPoseReceivedCallback cb);
	internal delegate void HlrSvRegisterInputEventHandler(HlrHandle handle, HlrSvInputEventReceivedCallback cb, IntPtr userData);
	internal delegate void HlrSvUnregisterInputEventHandler(HlrHandle handle, HlrSvInputEventReceivedCallback cb);
	internal delegate void HlrSvRegisterSpatialInputHandler(HlrHandle handle, HlrSvSpatialInputReceivedCallback cb, IntPtr userData);
	internal delegate void HlrSvUnregisterSpatialInputHandler(HlrHandle handle, HlrSvSpatialInputReceivedCallback cb);
	internal delegate void HlrRegisterCustomMessageHandler(HlrHandle handle, HlrCustomMessageCallback cb, IntPtr userData);
	internal delegate void HlrUnregisterCustomMessageHandler(HlrHandle handle, HlrCustomMessageCallback cb, IntPtr userData);
	internal delegate void HlrRegisterAudioDataHandler(HlrHandle handle, HlrSvAudioDataReceivedCallback cb, IntPtr userData);
	internal delegate void HlrUnregisterAudioDataHandler(HlrHandle handle, HlrSvAudioDataReceivedCallback cb);

	// Callback delegates
	internal delegate void HlrSvViewPoseReceivedCallback(in HlrXrPose pose, IntPtr userData);
	internal delegate void HlrSvInputEventReceivedCallback(in HlrInputEvent pose, IntPtr userData);
	internal delegate void HlrSvSpatialInputReceivedCallback(in HlrSvSpatialInput spatialInput, IntPtr userData);
	internal delegate void HlrCustomMessageCallback(in HlrCustomMessage message, IntPtr userData);
	internal delegate void HlrSvAudioDataReceivedCallback(in HlrAudioData audioData, IntPtr userData);

	internal delegate void QrIsSupportedCallback(in QrIsSupportedEventArgs args/*, IntPtr userData*/);
	internal delegate void QrRequestAccessCallback(in QrRequestAccessEventArgs args/*, IntPtr userData*/);
	internal delegate void QrAddedCallback(in QrAddedEventArgs args/*, IntPtr userData*/);
	internal delegate void QrUpdatedCallback(in QrUpdatedEventArgs args/*, IntPtr userData*/);
	internal delegate void QrRemovedCallback(in QrRemovedEventArgs args/*, IntPtr userData*/);
	internal delegate void QrEnumerationCompletedCallback(/*IntPtr userData*/);

	// Hololens 2
	internal readonly struct HlrSvMessageCallbacks
	{
		private readonly HlrSvViewPoseReceivedCallback _viewPoseReceivedCallback;
		private readonly HlrSvInputEventReceivedCallback _inputEventReceivedCallback;

		private readonly QrIsSupportedCallback _qrIsSupportedCallback;
		private readonly QrRequestAccessCallback _qrRequestAccessCallback;
		private readonly QrAddedCallback _qrAddedCallback;
		private readonly QrUpdatedCallback _qrUpdatedCallback;
		private readonly QrRemovedCallback _qrRemovedCallback;
		private readonly QrEnumerationCompletedCallback _qrEnumerationCompletedCallback;

		public HlrSvMessageCallbacks(
			HlrSvViewPoseReceivedCallback viewPoseReceivedCallback,
			HlrSvInputEventReceivedCallback inputEventReceivedCallback,
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

	internal delegate void RegisterMessageCallbacks(HlrHandle connectionHandle, ref HlrSvMessageCallbacks callbacks);
	internal delegate void ProcessMessages(HlrHandle connectionHandle);
	internal delegate HlrError QrIsSupported(HlrHandle connectionHandle);
	internal delegate HlrError QrRequestAccess(HlrHandle connectionHandle);
	internal delegate HlrError QrStart(HlrHandle connectionHandle);
	internal delegate HlrError QrStop(HlrHandle connectionHandle);
	//internal delegate Error QrGetList(ConnectionHandle connectionHandle);

	#endregion Data
}