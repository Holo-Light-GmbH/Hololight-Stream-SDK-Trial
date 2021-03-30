/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Security;
using UnityEngine.Assertions;

namespace HoloLight.Isar.Native
{
	// SafeHandles for simplifying IDisposable implementation.
	// Find more info and samples:
	// https://www.meziantou.net/stop-using-intptr-for-dealing-with-system-handles.htm
	// https://blog.benoitblanchon.fr/safehandle/

	/// <summary>
	/// Takes care of calling init and close to initialize and release all library resources.
	/// </summary>
	internal class HlrHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		internal const int API_HANDLE_INVALID = -1;

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public HlrHandle() : base(true)
		{
		}

		public override bool IsInvalid
		{
			[SecurityCritical]
			get
			{
				return this.handle == IntPtr.Zero ||
				       this.handle == new IntPtr(API_HANDLE_INVALID);
			}
		}

		public HlrSvConnectionApi ConnectionApi
		{
			private get;
			set;
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		protected override bool ReleaseHandle()
		{
			Assert.AreNotEqual(handle, IntPtr.Zero);
			Assert.AreNotEqual(handle, new IntPtr(API_HANDLE_INVALID));

			int tmpHandle = (int)handle;

			HlrError error = ConnectionApi.Close(ref handle);

			if (error != HlrError.eNone)
			{
				UnityEngine.Debug.LogError(
					$"ConnectionApi.Close({nameof(HlrHandle)}: {tmpHandle}) failed with {error}.");
			}

			return error == HlrError.eNone;
		}
	}
}