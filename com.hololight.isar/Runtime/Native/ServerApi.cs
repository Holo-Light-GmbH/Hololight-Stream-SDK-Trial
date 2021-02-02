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
	internal struct ServerApi
	{
		public Version Version;
		public SignalingApi SignalingApi;
		public ConnectionApi ConnectionApi;

		[DllImport(Constants.REMOTING_DLL_NAME, CallingConvention = CallingConvention.StdCall,
			EntryPoint = "hlr_sv_create_remoting_api")]
		internal static extern Error Create(
			ref /*out*/ ServerApi serverApi);
	}
}