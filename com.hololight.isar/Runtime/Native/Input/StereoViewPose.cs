/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

namespace HoloLight.Isar.Native.Input
{
	public struct StereoViewPose
	{
		public long timestamp;

		public Matrix4x4 viewLeft;
		public Matrix4x4 viewRight;

		public Matrix4x4 projLeft;
		public Matrix4x4 projRight;
	}
}