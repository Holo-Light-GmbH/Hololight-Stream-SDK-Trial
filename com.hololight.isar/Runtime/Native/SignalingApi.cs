/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System.Runtime.InteropServices;

namespace HoloLight.Isar.Native
{
	/// <summary>
	/// Signaling-related part of remoting API.
	/// </summary>
	internal struct HlrSvSignalingApi
	{
		public HlrSvCreateOffer CreateOffer;
		public HlrSvSetRemoteDescription SetRemoteAnswer;
		public HlrSvAddIceCandidate AddIceCandidate;
	}

	internal delegate HlrError HlrSvCreateOffer(HlrHandle connectionHandle);

	internal delegate HlrError HlrSvSetRemoteDescription(HlrHandle connectionHandle, string sdpAnswer);

	internal delegate HlrError HlrSvAddIceCandidate(
		HlrHandle connectionHandle,
		[MarshalAs(UnmanagedType.LPStr)] string mId,
		int mLineIndex,
		[MarshalAs(UnmanagedType.LPStr)] string candidate);
}