/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;
using HoloLight.Isar.Native;
using HoloLight.Isar.Native.Qr;
using Debug = HoloLight.Isar.Shared.Debug;

namespace HoloLight.Isar
{
	/// <summary>
	/// Provides basic access to Isar library functionality, such as initializing/closing and connection state events.
	/// </summary>
	public class Isar : IDisposable
	{
		public event Action<HlrConnectionState> ConnectionStateChanged;

		private object _lockObj = new object();
		private HlrConnectionState _connectionState = HlrConnectionState.Disconnected;
		private protected HlrHandle _handle = new HlrHandle();
		private protected HlrSvApi _serverApi;

		public bool IsInitialized
		{
			get
			{
				// HOTFIX: IAR-262
				// TODO: move this into IsarXRSettings and instantiate a static persistent instance similar to XRGeneralSettings & XRManagerSettings
				UnityEngine.XR.Management.XRManagerSettings settings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
				if (!settings.isInitializationComplete || settings.ActiveLoaderAs<Unity.XR.Isar.IsarXRLoader>() == null)
				{
					Debug.Log("ISAR XR plugin is disabled.");
					return false;
				}

				return true;
			}
		}

		public bool IsConnected
		{
			get
			{
				HlrConnectionState state;
				lock (_lockObj)
				{
					state = _connectionState;
				}
				return state == HlrConnectionState.Connected;
			}
		}

		public Isar()
		{
			if (!IsInitialized) return;

			_serverApi = new HlrSvApi();
			// TODO: ensure that this is called from the main thread
			HlrError err = HlrSvApi.Create(ref _serverApi);

			if (err != HlrError.eNone)
			{
				throw new Exception($"ISAR initialization failed with {err}");
			}

			//KL: use native get_handle as soon as we have it instead of calling init with garbage args
			HlrGraphicsApiConfig gfx = new HlrGraphicsApiConfig();
			HlrConnectionCallbacks cb = new HlrConnectionCallbacks();
			err = _serverApi.Connection.Init(null, gfx, cb, ref _handle);

			if (err != HlrError.eNone)
			{
				throw new Exception("Failed to init ISAR library");
			}

			//Kinda awful that we have to do this because it's easy to forget and other safehandle types don't do this.
			//Good thing nobody else has to do this now that this class exists :)
			_handle.ConnectionApi = _serverApi.Connection;

			Callbacks.ConnectionStateChanged += OnConnectionStateChanged;
		}

		public virtual void Dispose()
		{
			//if (!IsInitialized) return;
			if (!HlrVersion.IsValid(_serverApi.Version)) return;

			Callbacks.ConnectionStateChanged -= OnConnectionStateChanged;
			if (IsConnected) OnConnectionStateChanged(HlrConnectionState.Disconnected);
			_handle?.Dispose();
		}

		private void OnConnectionStateChanged(HlrConnectionState state)
		{
			lock (_lockObj)
			{
				_connectionState = state;
			}

			ConnectionStateChanged?.Invoke(state);
		}
	}

	public class IsarAudio : Isar
	{
		public delegate void IsarAudioDataCallback(in HlrAudioData audioData);
		public event IsarAudioDataCallback AudioDataReceived;

		public IsarAudio() : base()
		{
			if (!IsInitialized) return;

			_serverApi.Connection.RegisterAudioDataHandler(_handle, Callbacks.OnAudioDataReceived, IntPtr.Zero);
			Callbacks.AudioDataReceived += OnAudioDataReceived;
		}

		public void SetAudioTrackEnabled(bool enable)
		{
			if (!IsConnected) return;

			HlrError err = _serverApi.Connection.SetAudioTrackEnabled(_handle, Convert.ToInt32(enable));
			if (err != HlrError.eNone)
			{
				throw new InvalidOperationException($"Failed to set audio track to {enable}");
			}
		}

		private void OnAudioDataReceived(in HlrAudioData audioData, IntPtr userData)
		{
			AudioDataReceived?.Invoke(audioData);
		}

		public override void Dispose()
		{
			//if (!IsInitialized) return;
			if (!HlrVersion.IsValid(_serverApi.Version)) return;

			Callbacks.AudioDataReceived -= OnAudioDataReceived;
			_serverApi.Connection.UnregisterAudioDataHandler(_handle, Callbacks.OnAudioDataReceived);

			base.Dispose();
		}
	}

	public class IsarCustomSend : Isar
	{
		public delegate void IsarCustomMessageCallback(in HlrCustomMessage message);
		public event IsarCustomMessageCallback CustomMessageReceived;

		public IsarCustomSend() : base()
		{
			if (!IsInitialized) return;

			_serverApi.Connection.RegisterCustomMessageHandler(_handle, Callbacks.OnCustomMessageReceived, IntPtr.Zero);
			Callbacks.CustomMessageReceived += OnCustomMessageReceived;
		}

		public override void Dispose()
		{
			//if (!IsInitialized) return;
			if (!HlrVersion.IsValid(_serverApi.Version)) return;

			Callbacks.CustomMessageReceived -= OnCustomMessageReceived;
			_serverApi.Connection.UnregisterCustomMessageHandler(_handle, Callbacks.OnCustomMessageReceived, IntPtr.Zero);

			base.Dispose();
		}

		public void PushCustomMessage(HlrCustomMessage message)
		{
			if (!IsConnected) return;

			HlrError err = _serverApi.Connection.PushCustomMessage(_handle, message);
			if (err != HlrError.eNone)
			{
				throw new Exception($"Failed to send custom message with error {err}");
			}
		}

		private void OnCustomMessageReceived(in HlrCustomMessage message, IntPtr userData)
		{
			CustomMessageReceived?.Invoke(message);
		}
	}

	public class IsarQr : Isar
	{
		public delegate void QrCodeAddedCallback(in QrAddedEventArgs args);
		public delegate void QrCodeUpdatedCallback(in QrUpdatedEventArgs args);
		public delegate void QrCodeRemovedCallback(in QrRemovedEventArgs args);
		public delegate void QrCodeEnumerationCompletedCallback();
		public delegate void QrCodeIsSupportedReceivedCallback(in QrIsSupportedEventArgs args);
		public delegate void QrCodeAccessStatusReceivedCallback(in QrRequestAccessEventArgs args);

		public event QrCodeAddedCallback QrCodeAdded;
		public event QrCodeUpdatedCallback QrCodeUpdated;
		public event QrCodeRemovedCallback QrCodeRemoved;
		public event QrCodeEnumerationCompletedCallback QrCodeEnumerationCompleted;
		public event QrCodeIsSupportedReceivedCallback QrCodeIsSupportedReceived;
		public event QrCodeAccessStatusReceivedCallback QrCodeAccessStatusReceived;

		public IsarQr() : base()
		{
			if (!IsInitialized) return;

			//Subscribe to C callbacks
			QrApi.Init(_serverApi.Connection, _handle);
			QrApi.EnumerationCompleted += QrApi_OnEnumerationCompleted;
			QrApi.IsSupportedReceived += QrApi_OnIsSupportedReceived;
			QrApi.AccessStatusReceived += QrApi_OnAccessStatusReceived;
			QrApi.Added += QrApi_OnAdded;
			QrApi.Updated += QrApi_OnUpdated;
			QrApi.Removed += QrApi_OnRemoved;

			var msgCallbacks = new HlrSvMessageCallbacks(null, null, QrApi.OnIsSupported, QrApi.OnRequestAccess, QrApi.OnAdded, QrApi.OnUpdated, QrApi.OnRemoved, QrApi.OnEnumerationCompleted);
			_serverApi.Connection.RegisterMessageCallbacks(_handle, ref msgCallbacks);
		}

		public void Start()
		{
			if (!IsConnected) return;

			HlrError err = QrApi.Start();
			if (err != HlrError.eNone)
			{
				throw new Exception($"Failed to start QR code watcher with error: {err}");
			}
		}

		public void Stop()
		{
			if (!IsConnected) return;

			HlrError err = QrApi.Stop();
			if (err != HlrError.eNone)
			{
				throw new Exception($"Failed to stop QR code watcher with error: {err}");
			}
		}

		public void IsSupported()
		{
			if (!IsConnected) return;

			HlrError err = QrApi.IsSupported();
			if (err != HlrError.eNone)
			{
				throw new Exception($"IsSupported failed with error: {err}");
			}
		}

		public void RequestAccess()
		{
			if (!IsConnected) return;

			HlrError err = QrApi.RequestAccess();
			if (err != HlrError.eNone)
			{
				throw new Exception($"RequestAccess failed with error: {err}");
			}
		}

		public void ProcessMessages()
		{
			if (!IsConnected) return;

			QrApi.ProcessMessages();
		}

		public override void Dispose()
		{
			//if (!IsInitialized) return;
			if (!HlrVersion.IsValid(_serverApi.Version)) return;

			QrApi.EnumerationCompleted -= QrApi_OnEnumerationCompleted;
			QrApi.IsSupportedReceived -= QrApi_OnIsSupportedReceived;
			QrApi.AccessStatusReceived -= QrApi_OnAccessStatusReceived;
			QrApi.Added -= QrApi_OnAdded;
			QrApi.Updated -= QrApi_OnUpdated;
			QrApi.Removed -= QrApi_OnRemoved;
			// TODO: we currently have a lot of state duplication, this has the benefit of less resource contention but creates a synchronization problem
			// we should think about ways to make synchronization fool proof so that people can't forget about it
			QrApi.Init(new HlrSvConnectionApi(), new HlrHandle());

			base.Dispose();
		}

		private void QrApi_OnRemoved(in QrRemovedEventArgs args)
		{
			QrCodeRemoved?.Invoke(args);
		}

		private void QrApi_OnUpdated(in QrUpdatedEventArgs args)
		{
			QrCodeUpdated?.Invoke(args);
		}

		private void QrApi_OnAdded(in QrAddedEventArgs args)
		{
			QrCodeAdded?.Invoke(args);
		}

		private void QrApi_OnAccessStatusReceived(in QrRequestAccessEventArgs args)
		{
			QrCodeAccessStatusReceived?.Invoke(args);
		}

		private void QrApi_OnIsSupportedReceived(in QrIsSupportedEventArgs args)
		{
			QrCodeIsSupportedReceived?.Invoke(args);
		}

		private void QrApi_OnEnumerationCompleted()
		{
			QrCodeEnumerationCompleted?.Invoke();
		}
	}
}
