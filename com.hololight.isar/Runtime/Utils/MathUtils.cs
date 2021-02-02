/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using UnityEngine;

namespace HoloLight.Isar.Utils
{
	internal static class MathUtils
	{
		// ref: from WindowsNumerics.inl
		public static Quaternion QuaternionFromMatrix(in Matrix4x4 matrix)
		{
			if (matrix.m00 + matrix.m11 + matrix.m22 > 0.0f)
			{
				float s = Mathf.Sqrt(1.0f + matrix.m00 + matrix.m11 + matrix.m22);
				float invS = 0.5f / s;

				return new Quaternion((matrix.m21 - matrix.m12) * invS,
					(matrix.m02 - matrix.m20) * invS,
					(matrix.m10 - matrix.m01) * invS,
					s* 0.5f);
			}
			else if (matrix.m00 >= matrix.m11 && matrix.m00 >= matrix.m22)
			{
				float s = Mathf.Sqrt(1.0f + matrix.m00 - matrix.m11 - matrix.m22);
				float invS = 0.5f / s;

				return new Quaternion(0.5f * s,
					(matrix.m10 + matrix.m01) * invS,
					(matrix.m20 + matrix.m02) * invS,
					(matrix.m21 - matrix.m12) * invS);
			}
			else if (matrix.m11 > matrix.m22)
			{
				float s = Mathf.Sqrt(1.0f + matrix.m11 - matrix.m00 - matrix.m22);
				float invS = 0.5f / s;

				return new Quaternion((matrix.m01 + matrix.m10) * invS,
					0.5f * s,
					(matrix.m12 + matrix.m21) * invS,
					(matrix.m02 - matrix.m20) * invS);
			}
			else
			{
				float s = Mathf.Sqrt(1.0f + matrix.m22 - matrix.m00 - matrix.m11);
				float invS = 0.5f / s;

				return new Quaternion((matrix.m02 + matrix.m20) * invS,
					(matrix.m12 + matrix.m21) * invS,
					0.5f * s,
					(matrix.m10 - matrix.m01) * invS);
			}
		}
	}
}