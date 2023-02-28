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
		public HlrInit2 Init2;
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

		public HlrSvUnregisterConnectionStateHandler2 UnregisterConnectionStateHandler2;
		public HlrSvUnregisterViewPoseHandler2 UnregisterViewPoseHandler2;
		public HlrSvUnregisterInputEventHandler2 UnregisterInputEventHandler2;
		public HlrSvUnregisterSpatialInputHandler2 UnregisterSpatialInputHandler2;
		public HlrUnregisterAudioDataHandler2 UnregisterAudioDataHandler2;

		public HlrRegisterStatsCallbackHandler RegisterStatsCallbackHandler;
		public HlrUnregisterStatsCallbackHandler UnregisterStatsCallbackHandler;
		public HlrGetStats GetStats;

		public HlrSvRegisterAnchorAddCallback RegisterAnchorAddCallback;
		public HlrSvRegisterAnchorDeleteCallback RegisterAnchorDeleteCallback;
		public HlrSvRegisterAnchorUpdateCallback RegisterAnchorUpdateCallback;
		public HlrSvRegisterAnchorImportCallback RegisterAnchorImportCallback;
		public HlrSvRegisterAnchorExportCallback RegisterAnchorExportCallback;
		public HlrSvRegisterAnchorCreateStoreCallback RegisterAnchorCreateStoreCallback;
		public HlrSvRegisterAnchorDestroyStoreCallback RegisterAnchorDestroyStoreCallback;
		public HlrSvRegisterAnchorClearStoreCallback RegisterAnchorClearStoreCallback;
		public HlrSvRegisterAnchorPersistCallback RegisterAnchorPersistCallback;
		public HlrSvRegisterAnchorEnumeratePersistedAnchorNamesCallback RegisterAnchorEnumeratePersistedAnchorCallback;
		public HlrSvRegisterAnchorCreateSpatialAnchorFromPersistedNameCallback RegisterAnchorCreateSpatialAnchorFromPersistedNameCallback;
		public HlrSvRegisterAnchorUnpersistSpatialAnchor RegisterAnchorUnpersistSpatialAnchorCallback;

		public HlrSvUnregisterAnchorAddCallback UnregisterAnchorAddCallback;
		public HlrSvUnregisterAnchorDeleteCallback UnregisterAnchorDeleteCallback;
		public HlrSvUnregisterAnchorUpdateCallback UnregisterAnchorUpdateCallback;
		public HlrSvUnregisterAnchorImportCallback UnregisterAnchorImportCallback;
		public HlrSvUnregisterAnchorExportCallback UnregisterAnchorExportCallback;
		public HlrSvUnregisterAnchorCreateStoreCallback UnregisterAnchorCreateStoreCallback;
		public HlrSvUnregisterAnchorDestroyStoreCallback UnregisterAnchorDestroyStoreCallback;
		public HlrSvUnregisterAnchorClearStoreCallback UnregisterAnchorClearStoreCallback;
		public HlrSvUnregisterAnchorPersistCallback UnregisterAnchorPersistCallback;
		public HlrSvUnregisterAnchorEnumeratePersistedAnchorNamesCallback UnregisterAnchorEnumeratePersistedAnchorCallback;
		public HlrSvUnregisterAnchorCreateSpatialAnchorFromPersistedNameCallback UnregisterAnchorCreateSpatialAnchorFromPersistedNameCallback;
		public HlrSvUnregisterAnchorUnpersistSpatialAnchor UnregisterAnchorUnpersistSpatialAnchorCallback;
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

		private readonly IntPtr _userData;

		public HlrConnectionCallbacks(
			HlrConnectionStateChangedCallback connectionStateChangedCallback,
			HlrSdpCreatedCallback sdpCreatedCallback,
			HlrLocalIceCandidateCreatedCallback localIceCandidateCreatedCallback,
			IntPtr userData)
		{
			//Assert.AreNotEqual(null, connectionStateChangedCallback,   "null is not a valid callback");
			//Assert.AreNotEqual(null, sdpCreatedCallback,               "null is not a valid callback");
			//Assert.AreNotEqual(null, localIceCandidateCreatedCallback, "null is not a valid callback");
			_connectionStateChangedCallback = connectionStateChangedCallback;
			_sdpCreatedCallback = sdpCreatedCallback;
			_localIceCandidateCreatedCallback = localIceCandidateCreatedCallback;
			_userData = userData;
		}
	}

	internal delegate HlrError HlrInit(
		[MarshalAs(UnmanagedType.LPStr)] string configPath, HlrGraphicsApiConfig gfxConfig,
		HlrConnectionCallbacks callbacks, ref /*out*/ HlrHandle connectionHandle);
	internal delegate HlrError HlrInit2(
		RemotingConfigStruct remotingConfig, HlrGraphicsApiConfig gfxConfig,
		HlrConnectionCallbacks callbacks, ref /*out*/ HlrHandle connectionHandle);
	internal delegate HlrError HlrClose(ref IntPtr connectionHandle);
	internal delegate HlrError HlrSvReset(HlrHandle connectionHandle);
	internal delegate HlrError HlrSvSetAudioTrackEnabled(HlrHandle connectionHandle, int enabled);

	#endregion Connection

	#region Audio
	// TODO: use nuint from C# 9 once we drop Unity 2019
	// ref: https://stackoverflow.com/questions/32906774/what-is-equal-to-the-c-size-t-in-c-sharp
	// ref: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nint-nuint
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

	// All the delegates in the original code are unsafe.
	// Then again, it could be good for performance.
	// Also, ref returns could be useful.
	// https://blogs.msdn.microsoft.com/mazhou/2017/12/12/c-7-series-part-7-ref-returns/
	// https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke
	internal delegate void HlrConnectionStateChangedCallback(HlrConnectionState newState, IntPtr userData);

	internal delegate void HlrSvRegisterConnectionStateHandler(HlrHandle handle, HlrConnectionStateChangedCallback cb, IntPtr userData);
	internal delegate void HlrSvUnregisterConnectionStateHandler(HlrHandle handle, HlrConnectionStateChangedCallback cb);
	internal delegate void HlrSvUnregisterConnectionStateHandler2(HlrHandle handle, HlrConnectionStateChangedCallback cb, IntPtr userData);

	internal delegate void HlrSvRegisterViewPoseHandler(HlrHandle handle, HlrSvViewPoseReceivedCallback cb, IntPtr userData);
	internal delegate void HlrSvUnregisterViewPoseHandler(HlrHandle handle, HlrSvViewPoseReceivedCallback cb);
	internal delegate void HlrSvUnregisterViewPoseHandler2(HlrHandle handle, HlrSvViewPoseReceivedCallback cb, IntPtr userData);

	internal delegate void HlrSvRegisterInputEventHandler(HlrHandle handle, HlrSvInputEventReceivedCallback cb, IntPtr userData);
	internal delegate void HlrSvUnregisterInputEventHandler(HlrHandle handle, HlrSvInputEventReceivedCallback cb);
	internal delegate void HlrSvUnregisterInputEventHandler2(HlrHandle handle, HlrSvInputEventReceivedCallback cb, IntPtr userData);

	internal delegate void HlrSvRegisterSpatialInputHandler(HlrHandle handle, HlrSvSpatialInputReceivedCallback cb, IntPtr userData);
	internal delegate void HlrSvUnregisterSpatialInputHandler(HlrHandle handle, HlrSvSpatialInputReceivedCallback cb);
	internal delegate void HlrSvUnregisterSpatialInputHandler2(HlrHandle handle, HlrSvSpatialInputReceivedCallback cb, IntPtr userData);

	internal delegate void HlrRegisterCustomMessageHandler(HlrHandle handle, HlrCustomMessageCallback cb, IntPtr userData);
	internal delegate void HlrUnregisterCustomMessageHandler(HlrHandle handle, HlrCustomMessageCallback cb, IntPtr userData);

	internal delegate void HlrRegisterAudioDataHandler(HlrHandle handle, HlrSvAudioDataReceivedCallback cb, IntPtr userData);
	internal delegate void HlrUnregisterAudioDataHandler(HlrHandle handle, HlrSvAudioDataReceivedCallback cb);
	internal delegate void HlrUnregisterAudioDataHandler2(HlrHandle handle, HlrSvAudioDataReceivedCallback cb, IntPtr userData);

	internal delegate void HlrSvRegisterAnchorAddCallback(HlrHandle handle, HlrSvAnchorAddCallback cb, IntPtr userData);
	internal delegate void HlrSvRegisterAnchorDeleteCallback(HlrHandle handle, HlrSvAnchorDeleteCallback cb, IntPtr userData);
	internal delegate void HlrSvRegisterAnchorExportCallback(HlrHandle handle, HlrSvAnchorExportCallback cb, IntPtr userData);
	internal delegate void HlrSvRegisterAnchorImportCallback(HlrHandle handle, HlrSvAnchorImportCallback cb, IntPtr userData);
	internal delegate void HlrSvRegisterAnchorUpdateCallback(HlrHandle handle, HlrSvAnchorUpdateCallback cb, IntPtr userData);
	internal delegate void HlrSvRegisterAnchorCreateStoreCallback(HlrHandle handle, HlrSvAnchorCreateStoreCallback cb, IntPtr userData);
	internal delegate void HlrSvRegisterAnchorDestroyStoreCallback(HlrHandle handle, HlrSvAnchorDestroyStoreCallback cb, IntPtr userData);
	internal delegate void HlrSvRegisterAnchorClearStoreCallback(HlrHandle handle, HlrSvAnchorClearStoreCallback cb, IntPtr userData);
	internal delegate void HlrSvRegisterAnchorPersistCallback(HlrHandle handle, HlrSvAnchorPersistSpatialAnchorCallback cb, IntPtr userData);
	internal delegate void HlrSvRegisterAnchorEnumeratePersistedAnchorNamesCallback(HlrHandle handle, HlrSvAnchorEnumeratePersistedAnchorNamesCallback cb, IntPtr userData);
	internal delegate void HlrSvRegisterAnchorCreateSpatialAnchorFromPersistedNameCallback(HlrHandle handle, HlrSvAnchorCreateSpatialAnchorFromPersistedNameCallback cb, IntPtr userData);
	internal delegate void HlrSvRegisterAnchorUnpersistSpatialAnchor(HlrHandle handle, HlrSvAnchorUnpersistSpatialAnchorCallback cb, IntPtr userData);

	internal delegate void HlrSvUnregisterAnchorAddCallback();
	internal delegate void HlrSvUnregisterAnchorDeleteCallback();
	internal delegate void HlrSvUnregisterAnchorUpdateCallback();
	internal delegate void HlrSvUnregisterAnchorImportCallback();
	internal delegate void HlrSvUnregisterAnchorExportCallback();
	internal delegate void HlrSvUnregisterAnchorCreateStoreCallback();
	internal delegate void HlrSvUnregisterAnchorDestroyStoreCallback();
	internal delegate void HlrSvUnregisterAnchorClearStoreCallback();
	internal delegate void HlrSvUnregisterAnchorPersistCallback();
	internal delegate void HlrSvUnregisterAnchorEnumeratePersistedAnchorNamesCallback();
	internal delegate void HlrSvUnregisterAnchorCreateSpatialAnchorFromPersistedNameCallback();
	internal delegate void HlrSvUnregisterAnchorUnpersistSpatialAnchor();

	// Callback delegates

	// About string marshalling: SDP spec says it's UTF-8 unless there's a "charset=X" in the desc.
	// There is UnmanagedType.LPUTF8Str, but not in this version of .NET standard, so we can't use it.
	// Default marshaling is LPStr, i.e. const char* to ANSI. I guess it should be ok, but in case it's not,
	// this friendly comment reminds you that string encoding is hard.
	internal delegate void HlrSdpCreatedCallback(HlrSdpType type, string sdp, IntPtr userData);
	internal delegate void HlrLocalIceCandidateCreatedCallback(string sdpMline, int mlineIndex, string sdpizedCandidate, IntPtr userData);

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

	internal delegate void HlrSvAnchorAddCallback(in HlrAnchor anchorData, IntPtr userData);
	internal delegate void HlrSvAnchorDeleteCallback(in HlrAnchorId anchorId, IntPtr userData);
	internal delegate void HlrSvAnchorUpdateCallback(in HlrAnchor [] anchorData, UInt64 num_anchors, IntPtr userData);
	internal delegate void HlrSvAnchorImportCallback(in HlrAnchor [] anchorData, UInt64 num_anchors, IntPtr userData);
	internal delegate void HlrSvAnchorExportCallback(in byte[] data, UInt64 num_bytes, IntPtr userData);

	internal delegate void HlrSvAnchorCreateStoreCallback(bool is_succeeded, IntPtr userData);
	internal delegate void HlrSvAnchorDestroyStoreCallback(bool is_succeeded, IntPtr userData);
	internal delegate void HlrSvAnchorClearStoreCallback(bool is_succeeded, IntPtr userData);
	internal delegate void HlrSvAnchorEnumeratePersistedAnchorNamesCallback(UInt64 num_names, IntPtr names, IntPtr userData);
	internal delegate void HlrSvAnchorCreateSpatialAnchorFromPersistedNameCallback(in HlrAnchor anchorData, IntPtr userData);
	internal delegate void HlrSvAnchorPersistSpatialAnchorCallback(in HlrAnchorId anchorId, IntPtr userData);
	internal delegate void HlrSvAnchorUnpersistSpatialAnchorCallback(bool is_succeeded, IntPtr userData);


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

	#region Stats
	internal delegate void HlrRegisterStatsCallbackHandler(HlrHandle handle, HlrStatsCallback cb, IntPtr userData);
	internal delegate void HlrUnregisterStatsCallbackHandler(HlrHandle handle, HlrStatsCallback cb, IntPtr userData);
	internal delegate void HlrGetStats(HlrHandle handle);

	internal delegate void HlrStatsCallback(IntPtr statsData, IntPtr userData);
	#endregion

}