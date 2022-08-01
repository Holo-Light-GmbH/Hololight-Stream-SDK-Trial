/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;
using System.Runtime.InteropServices;

namespace HoloLight.Isar.Native
{
#pragma warning disable IDE1006 // Naming Styles
	// ReSharper disable InconsistentNaming

	internal static class Constants
	{
		internal const string REMOTING_DLL_NAME = "remoting_unity";
	}

	// ReSharper restore InconsistentNaming
#pragma warning restore IDE1006 // Naming Styles

	//https://docs.microsoft.com/en-us/cpp/dotnet/how-to-marshal-structures-using-pinvoke?view=vs-2019
	internal struct HlrSvApi
	{
		public HlrVersion Version;
		public HlrSvSignalingApi Signaling;
		public HlrSvConnectionApi Connection;

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "hlr_sv_create_remoting_api")]
		internal static extern HlrError Create(ref /*out*/ HlrSvApi serverApi);

		[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "isar_dequeue_camera_frame")]
		internal static extern int CameraDequeue(IntPtr pData, long lDataLen, ref long frame_id);

		//[DllImport(Constants.REMOTING_DLL_NAME, EntryPoint = "isar_camera_get_native_frame")]
		//internal static extern HlrError Camera_GetNativeFrame(ref byte pData, long lDataLen);




		//HLR_API(bool) isar_camera_get_native_frame(void** gfx_context, void **texture_ptr, int64_t* metadata)

	}
}