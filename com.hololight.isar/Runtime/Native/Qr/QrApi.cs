
using System;
using AOT;
using UnityEngine;
using UnityEngine.Assertions;

namespace HoloLight.Isar.Native.Qr
{
	//[NonVersionable]
	//internal ref struct ByReference<T>
	//{
	//	private IntPtr _value;
	//}

	public static class QrApi
	{
		private static HlrHandle _connectionHandle;

		// ConnectionApi is a struct so maybe this should be a reference? but of course C# doesn't allow it
		//private static ByReference<ConnectionApi> _connectionApi;
		private static /*ref*/ HlrSvConnectionApi _connectionApi;

		internal static void Init(HlrSvConnectionApi connectionApi, HlrHandle connectionHandle)
		{
			_connectionApi = connectionApi;
			_connectionHandle = connectionHandle;
		}

		public static HlrError IsSupported()
		{
			Assert.IsNotNull(_connectionApi.QrIsSupported, "You didn't call ServerApi.Create!");
			return _connectionApi.QrIsSupported(_connectionHandle);
		}
		public delegate void IsSupportedEventHandler(in QrIsSupportedEventArgs args);
		public static event IsSupportedEventHandler IsSupportedReceived;
		[MonoPInvokeCallback(typeof(QrIsSupportedCallback))]
		internal static void OnIsSupported(in QrIsSupportedEventArgs args)
		{
			try
			{
				IsSupportedReceived?.Invoke(in args);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static HlrError RequestAccess()
		{
			Assert.IsNotNull(_connectionApi.QrRequestAccess, "You didn't call ServerApi.Create!");
			return _connectionApi.QrRequestAccess(_connectionHandle);
		}
		public delegate void QrAccessReceivedEventHandler(in QrRequestAccessEventArgs args);
		public static event QrAccessReceivedEventHandler AccessStatusReceived;
		[MonoPInvokeCallback(typeof(QrRequestAccessCallback))]
		internal static void OnRequestAccess(in QrRequestAccessEventArgs args)
		{
			try
			{
				AccessStatusReceived?.Invoke(in args);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static HlrError Start()
		{
			Assert.IsNotNull(_connectionApi.QrStart, "You didn't call ServerApi.Create!");
			return _connectionApi.QrStart(_connectionHandle);
		}

		public static HlrError Stop()
		{
			Assert.IsNotNull(_connectionApi.QrStop, "You didn't call ServerApi.Create!");
			return _connectionApi.QrStop(_connectionHandle);
		}

		// TODO: implement this
		//public static Error GetList()
		//{
		//	_connectionApi.QrCodeGetList(_connectionHandle);
		//}

		public delegate void QrAddedEventHandler(in QrAddedEventArgs args);
		public static event QrAddedEventHandler Added;
		[MonoPInvokeCallback(typeof(QrAddedCallback))]
		internal static void OnAdded(in QrAddedEventArgs args)
		{
			try
			{
				Added?.Invoke(in args);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public delegate void QrUpdatedEventHandler(in QrUpdatedEventArgs args);
		public static event QrUpdatedEventHandler Updated;
		[MonoPInvokeCallback(typeof(QrUpdatedCallback))]
		internal static void OnUpdated(in QrUpdatedEventArgs args)
		{
			try
			{
				Updated?.Invoke(in args);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public delegate void QrRemovedEventHandler(in QrRemovedEventArgs args);
		public static event QrRemovedEventHandler Removed;
		[MonoPInvokeCallback(typeof(QrRemovedCallback))]
		internal static void OnRemoved(in QrRemovedEventArgs args)
		{
			try
			{
				Removed?.Invoke(in args);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public delegate void QrEnumerationCompletedEventHandler();
		public static event QrEnumerationCompletedEventHandler EnumerationCompleted;
		[MonoPInvokeCallback(typeof(QrEnumerationCompletedCallback))]
		internal static void OnEnumerationCompleted()
		{
			try
			{
				EnumerationCompleted?.Invoke();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		public static void ProcessMessages()
		{
			Assert.IsNotNull(_connectionApi.ProcessMessages, "You didn't call ServerApi.Create!");
			_connectionApi.ProcessMessages(_connectionHandle);
		}
	}
}
