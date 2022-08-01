using HoloLight.Isar.Native;

namespace HoloLight.Isar.Utils
{
	public static class Convert
	{
		// TODO: this should be made internal and should never need to be called by users/event listeners
		// TODO: this will be moved into the unity <-> .Net bindings layer
		// TODO: should we actually support .Net in general? maybe that's just unnecessary bloat
		// TODO: if anyone needs .Net they can duplicate our c# code and replace UnityEngine types (Matrix4x4, Vector3, etc)
		public static void FlipHandedness(ref UnityEngine.Vector2 value)
		{
			value.y *= -1;
		}

		public static UnityEngine.Vector2 FlipHandedness(UnityEngine.Vector2 value)
		{
			var result = value;
			FlipHandedness(ref result);
			return result;
		}

		public static UnityEngine.Vector3 ToUnity(/*this*/ HlrVector3 value)
		{
			return new UnityEngine.Vector3(value.X, value.Y, value.Z);
		}

		public static void FlipHandedness(ref UnityEngine.Vector3 value)
		{
			value.z *= -1;
		}

		public static UnityEngine.Vector3 FlipHandedness(UnityEngine.Vector3 value)
		{
			var result = value;
			FlipHandedness(ref result);
			return result;
		}

		// quaternions have no coord system handedness
		// the conversion from a quaternion to a matrix implies a specific coordinate system
		public static UnityEngine.Quaternion ToUnity(HlrQuaternion value)
		{
			return new UnityEngine.Quaternion(value.X, value.Y, value.Z, value.W);
		}

		public static void FlipHandedness(ref UnityEngine.Quaternion value)
		{
			value.x *= -1;
			value.y *= -1;
		}

		public static UnityEngine.Quaternion FlipHandedness(UnityEngine.Quaternion value)
		{
			var result = value;
			FlipHandedness(ref result);
			return result;
		}

		// This does no coordinate system conversion
		public static UnityEngine.Matrix4x4 ToUnity(/*this*/ HlrMatrix4x4 value)
		{
			var matrix = new UnityEngine.Matrix4x4
			{
				m00 = value.M00, m10 = value.M10, m20 = value.M20, m30 = value.M30,
				m01 = value.M01, m11 = value.M11, m21 = value.M21, m31 = value.M31,
				m02 = value.M02, m12 = value.M12, m22 = value.M22, m32 = value.M32,
				m03 = value.M03, m13 = value.M13, m23 = value.M23, m33 = value.M33
			};
			return matrix;
		}

		public static void FlipHandednessColumnMajor(ref /*this*/ UnityEngine.Matrix4x4 value)
		{
			// TODO: flip sign bits or SIMD if possible
			value.m02 *= -1;
			value.m12 *= -1;
			value.m22 *= -1;
			value.m32 *= -1;
		}

		public static void FlipHandednessRowMajor(ref /*this*/ UnityEngine.Matrix4x4 value)
		{
			// TODO: flip sign bits or SIMD if possible
			value.m20 *= -1;
			value.m21 *= -1;
			value.m22 *= -1;
			value.m23 *= -1;
		}

		//public static UnityEngine.Matrix4x4 FlippedHandednessColumnMajor( /*this*/ UnityEngine.Matrix4x4 value)
		//{
		//	var result = value;
		//	FlipHandednessColumnMajor(ref result);
		//	return result;

		//	//var matrix = new UnityEngine.Matrix4x4
		//	//{
		//	//	m00 = value.m00, m10 = value.m10, m20 = value.m20, m30 = value.m30, // column 0
		//	//	m01 = value.m01, m11 = value.m11, m21 = value.m21, m31 = value.m31, // column 1
		//	//	// we need to invert z axis, but i dont know how the imaginary part affects this
		//	//	m02 = value.m02, m12 = value.m12, m22 = -value.m22, m32 = value.m32, // column 2
		//	//	// TODO: until then we just flip the position
		//	//	m03 = value.m03, m13 = value.m13, m23 = -value.m23, m33 = value.m33 // column 3
		//	//};
		//	//return matrix;
		//	// TODO: we might not need this?
		//	//return matrix.transpose;
		//}

		//public static UnityEngine.Matrix4x4 FlippedHandednessRowMajor( /*this*/ UnityEngine.Matrix4x4 value)
		//{
		//	var result = value;
		//	FlipHandednessRowMajor(ref result);
		//	return result;

		//	//var matrix = new UnityEngine.Matrix4x4
		//	//{
		//	//	m00 = value.m00, m10 = value.m10, m20 = value.m20, m30 = value.m30, // column 0
		//	//	m01 = value.m01, m11 = value.m11, m21 = value.m21, m31 = value.m31, // column 1
		//	//	// we need to invert z axis, but i dont know how the imaginary part affects this
		//	//	m02 = value.m02, m12 = value.m12, m22 = -value.m22, m32 = value.m32, // column 2
		//	//	// TODO: until then we just flip the position
		//	//	m03 = value.m03, m13 = value.m13, m23 = -value.m23, m33 = value.m33 // column 3
		//	//};
		//	//return matrix;
		//	// TODO: we might not need this?
		//	//return matrix.transpose;
		//}
	}
}