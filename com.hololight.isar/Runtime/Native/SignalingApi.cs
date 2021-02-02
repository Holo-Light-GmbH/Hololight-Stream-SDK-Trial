/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System.Runtime.InteropServices;

namespace HoloLight.Isar.Native
{
	/// <summary>
	/// Signaling-related part of remoting API.
	/// </summary>
	internal struct SignalingApi
	{
		public CreateOffer CreateOffer;
		public SetRemoteDescription SetRemoteAnswer;
		public AddIceCandidate AddIceCandidate;
	}

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate Error CreateOffer(ConnectionHandle connectionHandle);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate Error SetRemoteDescription(ConnectionHandle connectionHandle, string sdpAnswer);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate Error AddIceCandidate(
		ConnectionHandle connectionHandle,
		[MarshalAs(UnmanagedType.LPStr)] string mId,
		int mLineIndex,
		[MarshalAs(UnmanagedType.LPStr)] string candidate);
}