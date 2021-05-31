/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

namespace HoloLight.Isar.Native.Input
{
	public struct HlrXrPose
	{
		public long timestamp;

		public HlrMatrix4x4 viewLeft;
		public HlrMatrix4x4 viewRight;

		public HlrMatrix4x4 projLeft;
		public HlrMatrix4x4 projRight;
	}
}