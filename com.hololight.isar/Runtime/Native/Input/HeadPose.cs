/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

namespace HoloLight.Isar.Native.Input
{
	// TODO: this represents any pose (position + orientation) in 3d space
	// Vector3 + Quaternion would be preferred but hololens spatial API
	// doesn't expose direct access to the Quaternion of the Head, unfortunately
	public struct HlrHeadPose
	{
		public HlrVector3 Position;
		public HlrVector3 ForwardDirection;
		public HlrVector3 UpDirection;

		public HlrHeadPose(HlrVector3 position, HlrVector3 forwardDirection, HlrVector3 upDirection)
		{
			Position = position;
			ForwardDirection = forwardDirection;
			UpDirection = upDirection;
		}
	}
}