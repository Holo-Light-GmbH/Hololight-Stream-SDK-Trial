/*
* Copyright 2019 Holo-Light GmbH. All Rights Reserved.
*/

using System;

namespace HoloLight.Isar.Native
{
	// This is platform specific for now. Will need changes in representation
	// when more graphics apis are added maybe a union would fit?
	// Also, we don't specify StructLayout since according to MS docs the
	// C# compiler defaults to Sequential for structs.
	// See: https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.layoutkind?=netframework-4.8#remarks
	internal struct HlrGraphicsApiConfig
	{
		//ID3D11Device*
		public readonly IntPtr d3dDevice;

		//ID3D11Context*
		public readonly IntPtr d3dContext;

		//D3D11_TEXTURE2D_DESC*
		public readonly IntPtr renderTargetDesc;

		//D3D11_TEXTURE2D_DESC*
		public readonly IntPtr depthRenderTargetDesc;

		public HlrGraphicsApiConfig(IntPtr device, IntPtr context, IntPtr desc, IntPtr depthDesc)
		{
			d3dDevice = device;
			d3dContext = context;
			renderTargetDesc = desc;
			depthRenderTargetDesc = depthDesc;
		}
	}

	// this is platform specific for now. Will need changes in representation when more graphics apis are added.
	// maybe a union would fit?
	internal struct HlrGraphicsApiFrame
	{
		public readonly long timestamp;

		// ID3D11Texture2D*
		public readonly IntPtr d3dFrame;
		
		// ID3D11Texture2D*
		public readonly IntPtr d3dDepthFrame;

		public readonly uint subResourceIndex;

		public HlrGraphicsApiFrame(long timestamp, IntPtr d3dFrame, IntPtr depthFrame, uint subResourceIndex = 0)
		{
			this.timestamp = timestamp;
			this.d3dFrame = d3dFrame;
			this.d3dDepthFrame = depthFrame;
			this.subResourceIndex = subResourceIndex;
		}
	}

	public enum HlrSdpType : int
	{
		Offer = 0,
		Answer
	}

	public enum HlrConnectionState : int
	{
		Disconnected,
		Connected,
		Failed
	}

	//Keep in sync with native HlrError
	public enum HlrError : Int32
	{
		eNone = 0,                  // This should only be used at the API boundary.
		eAlreadyInitialized,        // Init has already been called
		eInvalidHandle,             // handle is invalid or doesn't match global_handle
		ePeerConnectionFactory,     // CreatePeerConnectionFactory failed
		ePeerConnection,            // CreatePeerConnection failed
		eDataChannel_Creation,      // CreateDataChannel failed
		eDataChannel_Send,          // Sending via DataChannel
		eDataChannel_Send_NotOpen,  // Sending via DataChannel while state is != open
		eAddTrack,                  // AddTrack failed
		eVideoSource,
		eVideoTrack,
		eStartRtcEventLog,  // StartRtcEventLog failed
		eConfig_UnsupportedOrMissingRole,
		eConfig_UnsupportedOrMissingEncoder,
		eConfig_UnsupportedOrMissingDecoder,
		eConfig_UnsupportedOrMissingVideoSource,
		eConfig_SignalingInvalidOrMissing,
		eConfig_SignalingIpInvalidOrMissing,
		eConfig_SignalingPortInvalidOrMissing,
		eNotConnected,        // user wants to do something but isn't connected
		eFileOpen,            // failed to open file
		eConfigParse,         // failed to parse config file
		eSdpParse,            // failed to parse session decription or ICE candidate
		eNoFrame,             // user tried to pull frame but there is none
		eUnsupportedVersion,  // user tried to create api with unsupported version
		eInvalidArgument,
		eMessageTooLong,      //custom message data too long
		eAudioTrack,          // failed to initialize audio track
		eAudioTrack_NotInitialized, // tried to perform an operation on audio track but there is none
	}
}