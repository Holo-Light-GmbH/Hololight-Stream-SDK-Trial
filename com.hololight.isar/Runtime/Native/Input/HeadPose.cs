/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

namespace HoloLight.Isar.Native.Input
{
	// TODO: this represents any pose (position + orientation) in 3d space
	// Vector3 + Quaternion would be preferred but hololens spatial API
	// doesn't expose direct access to the Quaternion of the Head, unfortunately
	public struct HeadPose
	{
		public Vector3 Position;
		public Vector3 ForwardDirection;
		public Vector3 UpDirection;

		public HeadPose(Vector3 position, Vector3 forwardDirection, Vector3 upDirection)
		{
			Position = position;
			ForwardDirection = forwardDirection;
			UpDirection = upDirection;
		}
	}
}