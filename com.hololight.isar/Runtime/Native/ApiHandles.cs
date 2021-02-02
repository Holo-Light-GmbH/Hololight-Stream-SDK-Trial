/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Security;
using UnityEngine.Assertions;

using static HoloLight.Isar.Native.HandleConstants;

namespace HoloLight.Isar.Native
{
	// TODO(viktor): get rid of ApiHandle and move the const into ConnectionHandle
	static class HandleConstants
	{
		internal const int API_HANDLE_INVALID = -1;
	}

	// SafeHandles for simplifying IDisposable implementation.
	// Find more info and samples:
	// https://www.meziantou.net/stop-using-intptr-for-dealing-with-system-handles.htm
	// https://blog.benoitblanchon.fr/safehandle/
	internal class ApiHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public ApiHandle() : base(true)
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

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		protected override bool ReleaseHandle()
		{
			Assert.AreNotEqual(handle, IntPtr.Zero);
			Assert.AreNotEqual(handle, new IntPtr(API_HANDLE_INVALID));

			int tmpHandle = (int)handle;

			//Error error = ServerApi.Destroy(ref handle);

			//if (error != Error.eNone)
			//{
			//	UnityEngine.Debug.LogError(
			//		$"ServerApi.Destroy({nameof(ApiHandle)}: {tmpHandle}) failed with {error}.");
			//}

			//return error == Error.eNone;
			return true;
		}
	}

	/// <summary>
	/// Takes care of calling connect and disconnect on peer connection.
	/// </summary>
	internal class ConnectionHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		public ConnectionHandle() : base(true)
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

		public ConnectionApi ConnectionApi
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

			Error error = ConnectionApi.Close(ref handle);

			if (error != Error.eNone)
			{
				UnityEngine.Debug.LogError(
					$"ConnectionApi.Close({nameof(ConnectionHandle)}: {tmpHandle}) failed with {error}.");
			}

			return error == Error.eNone;
		}
	}
}