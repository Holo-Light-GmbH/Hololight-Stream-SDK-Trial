
namespace HoloLight.Isar.Utils
{
	static class Debug
	{
		// print as row-major (scientific notation)
		public static void LogMatrixScientific(string name, in UnityEngine.Matrix4x4 matrix)
		{
			UnityEngine.Debug.LogWarning($"{name}\n" +
			                 $"{matrix.m00:F6}\t{matrix.m01:F6}\t{matrix.m02:F6}\t{matrix.m03:F6}\n" +
			                 $"{matrix.m10:F6}\t{matrix.m11:F6}\t{matrix.m12:F6}\t{matrix.m13:F6}\n" +
			                 $"{matrix.m20:F6}\t{matrix.m21:F6}\t{matrix.m22:F6}\t{matrix.m23:F6}\n" +
			                 $"{matrix.m30:F6}\t{matrix.m31:F6}\t{matrix.m32:F6}\t{matrix.m33:F6}\n");
		}

		public static void LogMatrix(string name, in UnityEngine.Matrix4x4 matrix)
		{
			UnityEngine.Debug.LogWarning($"{name}\n (data is column-major in memory)\n" +
			                             $"{matrix.m00:F6}\t{matrix.m10:F6}\t{matrix.m20:F6}\t{matrix.m30:F6}\n" +
			                             $"{matrix.m01:F6}\t{matrix.m11:F6}\t{matrix.m21:F6}\t{matrix.m31:F6}\n" +
			                             $"{matrix.m02:F6}\t{matrix.m12:F6}\t{matrix.m22:F6}\t{matrix.m32:F6}\n" +
			                             $"{matrix.m03:F6}\t{matrix.m13:F6}\t{matrix.m23:F6}\t{matrix.m33:F6}\n");
		}

		public static void LogMatrixScientific(string name, in Native.HlrMatrix4x4 matrix)
		{
			UnityEngine.Debug.LogWarning($"{name}\n" +
			                             $"{matrix.M00:F6}\t{matrix.M01:F6}\t{matrix.M02:F6}\t{matrix.M03:F6}\n" +
			                             $"{matrix.M10:F6}\t{matrix.M11:F6}\t{matrix.M12:F6}\t{matrix.M13:F6}\n" +
			                             $"{matrix.M20:F6}\t{matrix.M21:F6}\t{matrix.M22:F6}\t{matrix.M23:F6}\n" +
			                             $"{matrix.M30:F6}\t{matrix.M31:F6}\t{matrix.M32:F6}\t{matrix.M33:F6}\n");
		}

		public static void LogMatrix(string name, in Native.HlrMatrix4x4 matrix)
		{
			UnityEngine.Debug.LogWarning($"{name}\n (data is column-major in memory)\n" +
			                             $"{matrix.M00:F6}\t{matrix.M10:F6}\t{matrix.M20:F6}\t{matrix.M30:F6}\n" +
			                             $"{matrix.M01:F6}\t{matrix.M11:F6}\t{matrix.M21:F6}\t{matrix.M31:F6}\n" +
			                             $"{matrix.M02:F6}\t{matrix.M12:F6}\t{matrix.M22:F6}\t{matrix.M32:F6}\n" +
			                             $"{matrix.M03:F6}\t{matrix.M13:F6}\t{matrix.M23:F6}\t{matrix.M33:F6}\n");
		}

	}
}
