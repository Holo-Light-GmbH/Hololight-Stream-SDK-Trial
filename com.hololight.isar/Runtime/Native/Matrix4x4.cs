/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

namespace HoloLight.Isar.Native
{
	public struct HlrMatrix4x4
	{
		// TODO: this does not represent the current layout (which is that of directx)
		// this will be fixed once we move all the conversion crap to remoting_unity
		public float M00, M10, M20, M30;
		public float M01, M11, M21, M31;
		public float M02, M12, M22, M32;
		public float M03, M13, M23, M33;

		public HlrMatrix4x4(
			float m00, float m10, float m20, float m30,
			float m01, float m11, float m21, float m31,
			float m02, float m12, float m22, float m32,
			float m03, float m13, float m23, float m33)
		{
			M00 = m00; M10 = m10; M20 = m20; M30 = m30;
			M01 = m01; M11 = m11; M21 = m21; M31 = m31;
			M02 = m02; M12 = m12; M22 = m22; M32 = m32;
			M03 = m03; M13 = m13; M23 = m23; M33 = m33;
		}
	}
}
