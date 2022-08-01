/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;
using System.Runtime.InteropServices;
using HoloLight.Isar.Native;
using HoloLight.Isar.Native.CustomMessage;
using HoloLight.Isar.Native.Qr;
using UnityEngine.XR.Management;

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
		protected HlrConnectionState _previousConnectionState = HlrConnectionState.Disconnected; // HACK
		private protected HlrHandle _handle = new HlrHandle();
		private protected HlrSvApi _serverApi;

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
			// ISAR XR loader check
			var loader = XRGeneralSettings.Instance.Manager.activeLoader as BaseIsarLoader;
			if (loader == null)
			{
				throw new InvalidOperationException("There is no ISAR loader active." +
					" ISAR cannot be used. " +
					"Make sure to enable an ISAR Loader " +
					"through Edit -> Project Settings ->" +
					"XR Plug-in Management before using this class.");
			}

			_serverApi = new HlrSvApi();
			// TODO: ensure that this is called from the main thread
			HlrError err = HlrSvApi.Create(ref _serverApi);

			if (err != HlrError.eNone)
			{
				throw new Exception($"Isar initialization failed with {err}");
			}

			//KL: use native get_handle as soon as we have it instead of calling init with garbage args
			HlrGraphicsApiConfig gfx = new HlrGraphicsApiConfig();
			HlrConnectionCallbacks cb = new HlrConnectionCallbacks();
			err = _serverApi.Connection.Init(null, gfx, cb, ref _handle);

			if (err != HlrError.eNone)
			{
				throw new Exception("Failed to init remoting lib");
			}

			//Kinda awful that we have to do this because it's easy to forget and other safehandle types don't do this.
			//Good thing nobody else has to do this now that this class exists :)
			_handle.ConnectionApi = _serverApi.Connection;

			_connectionState = loader.ConnectionState;

			Callbacks.ConnectionStateChanged += OnConnectionStateChanged;
		}

		public virtual void Dispose()
		{
			Callbacks.ConnectionStateChanged -= OnConnectionStateChanged;
			_handle?.Dispose();
		}

		private void OnConnectionStateChanged(HlrConnectionState state)
		{
			lock (_lockObj)
			{
				_previousConnectionState = _connectionState;
				_connectionState = state;
				ConnectionStateChanged?.Invoke(state);
			}
		}
	}

	public class IsarViewPose : Isar
	{
		public delegate void IsarViewPoseCallback(in Native.Input.HlrXrPose viewPose);
		public event IsarViewPoseCallback ViewPoseReceived;

		public IsarViewPose() : base()
		{
			_serverApi.Connection.RegisterViewPoseHandler(_handle, Callbacks.OnViewPoseReceived, IntPtr.Zero);
			Callbacks.ViewPoseReceived += OnViewPoseReceived;
		}

		private void OnViewPoseReceived(in Native.Input.HlrXrPose viewPose)
		{
			ViewPoseReceived?.Invoke(viewPose);
		}

		public override void Dispose()
		{
			Callbacks.ViewPoseReceived -= OnViewPoseReceived;
			_serverApi.Connection.UnregisterViewPoseHandler(_handle, Callbacks.OnViewPoseReceived);

			base.Dispose();
		}
	}

	public class IsarAudio : Isar
	{
		public delegate void IsarAudioDataCallback(in HlrAudioData audioData);
		public event IsarAudioDataCallback AudioDataReceived;

		public IsarAudio() : base()
		{
			_serverApi.Connection.RegisterAudioDataHandler(_handle, Callbacks.OnAudioDataReceived, IntPtr.Zero);
			Callbacks.AudioDataReceived += OnAudioDataReceived;
		}

		public void SetAudioTrackEnabled(bool enable)
		{
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
			_serverApi.Connection.RegisterCustomMessageHandler(_handle, Callbacks.OnCustomMessageReceived, IntPtr.Zero);
			Callbacks.CustomMessageReceived += Callbacks_OnCustomMessageReceived;
		}

		public override void Dispose()
		{
			Callbacks.CustomMessageReceived -= Callbacks_OnCustomMessageReceived;
			_serverApi.Connection.UnregisterCustomMessageHandler(_handle, Callbacks.OnCustomMessageReceived, IntPtr.Zero);

			base.Dispose();
		}

		public void Send(MessageType type, byte[] bytes = null)
		{
			if (!IsConnected) throw new ArgumentException("Trying to send a custom message but the object is not currently connected to the client");

			byte[] typeBytes = BitConverter.GetBytes((int)type);
			int messageLength = typeBytes.Length + (bytes != null ? bytes.Length : 0);

			IntPtr unmanagedPtr = Marshal.AllocHGlobal(messageLength);
			Marshal.Copy(typeBytes, 0, unmanagedPtr, typeBytes.Length);
			if (bytes != null)
			{
				Marshal.Copy(bytes, 0, unmanagedPtr + typeBytes.Length, bytes.Length);
			}

			HlrCustomMessage message = new HlrCustomMessage();
			message.Length = (uint)messageLength;
			message.Data = unmanagedPtr;

			PushCustomMessage(message);

			Marshal.FreeHGlobal(unmanagedPtr);
		}

		public void PushCustomMessage(HlrCustomMessage message)
		{
			HlrError err = _serverApi.Connection.PushCustomMessage(_handle, message);
			if (err != HlrError.eNone)
			{
				throw new Exception($"Failed to send custom message with error {err}");
			}
		}

		private void Callbacks_OnCustomMessageReceived(in HlrCustomMessage message, IntPtr userData)
		{
			CustomMessageReceived?.Invoke(message);
		}
	}

	public class IsarCustomAudioSource : Isar
	{
		private float[] _carryDataBuffer;
		private long _carryIndex;

		private int _sampleRate;
		private int _bufferSize;
		private int _numChannels;

		private int _mix256Order;
		private float [] _mix256Buffer;

		private const int KHZ48_VAL = 48000;
		private const int KHZ_48_NUM_SAMPLES = 480;
		private const int BUFFER_SIZE_512 = 512;
		private const int STEREO_NUM_CHANNELS = 2;
		private const int BITS_16_PER_SAMPLE = 16;
		private double PCM_FLOAT_INT_CONVERSION_CONST = 32768.0;

		public IsarCustomAudioSource(int dspSampleRate, int dspBufferSize) : base()
		{
			_bufferSize = dspBufferSize;
			_sampleRate = dspSampleRate;
			_numChannels = STEREO_NUM_CHANNELS;
			_carryDataBuffer = new float[BUFFER_SIZE_512 * _numChannels];
			_mix256Buffer = new float[BUFFER_SIZE_512 * _numChannels];
			_mix256Order = 0;
			_carryIndex = 0;
		}

		private short[] ConvertToIntPCM(in float[] data)
		{
			short[] sampleData = new short[KHZ_48_NUM_SAMPLES * _numChannels];

			for (int i = 0; i < KHZ_48_NUM_SAMPLES; i++)
			{
				for (int j = 0; j < _numChannels; j++)
				{
					int sample = (int)(data[i * _numChannels + j] * (PCM_FLOAT_INT_CONVERSION_CONST));
					sample = Math.Min(Math.Max(sample, -(int)PCM_FLOAT_INT_CONVERSION_CONST), (int)(PCM_FLOAT_INT_CONVERSION_CONST-1));
					sampleData[_numChannels * i + j] = (short)sample;
				}
			}

			return sampleData;
		}

		public void Reset()
		{
			_carryDataBuffer = new float[BUFFER_SIZE_512 * _numChannels];
			_mix256Buffer = new float[BUFFER_SIZE_512 * _numChannels];
			_mix256Order = 0;
			_carryIndex = 0;
		}

		private void PushAudioFrames(in float[] data)
		{
			float[] slice = new float[KHZ_48_NUM_SAMPLES * _numChannels];
			Array.Copy(_carryDataBuffer, 0, slice, 0, _carryIndex * _numChannels);
			Array.Copy(data, 0, slice, _carryIndex * _numChannels, KHZ_48_NUM_SAMPLES * _numChannels - _numChannels * _carryIndex);
			Array.Clear(_carryDataBuffer, 0, _carryDataBuffer.Length);
			_carryIndex += BUFFER_SIZE_512 - KHZ_48_NUM_SAMPLES;
			Array.Copy(data, BUFFER_SIZE_512 * _numChannels - _numChannels * _carryIndex, _carryDataBuffer, 0, _carryIndex * _numChannels);
			SendHlrAudioData(ConvertToIntPCM(slice));

			if (_carryIndex == KHZ_48_NUM_SAMPLES)
			{
				float[] carrySlice = new float[KHZ_48_NUM_SAMPLES * _numChannels];
				Array.Copy(_carryDataBuffer, 0, carrySlice, 0, KHZ_48_NUM_SAMPLES * _numChannels);
				SendHlrAudioData(ConvertToIntPCM(carrySlice));
				_carryIndex = 0;
			}
		}

		private void SendHlrAudioData(in short[] audioBuffer)
		{
			HlrAudioData audioData;
			audioData = new HlrAudioData();
			audioData.SamplesPerChannel = (System.UIntPtr)KHZ_48_NUM_SAMPLES;
			audioData.BitsPerSample = BITS_16_PER_SAMPLE;
			audioData.NumberOfChannels = (System.UIntPtr)STEREO_NUM_CHANNELS;
			audioData.SampleRate = KHZ48_VAL;

			GCHandle handle = GCHandle.Alloc(audioBuffer, GCHandleType.Pinned);
			try
			{
				audioData.Data = handle.AddrOfPinnedObject();
				SendAudioData(audioData);
			}
			finally
			{
				if (handle.IsAllocated)
				{
					handle.Free();
				}
			}
		}

		public void PushAudioData(float[] data)
		{
			if (_bufferSize == BUFFER_SIZE_512)
			{
				PushAudioFrames(data);
			}
			else
			{
				if ( _bufferSize < BUFFER_SIZE_512)
				{
					Array.Copy(data, 0, _mix256Buffer, _mix256Order * (BUFFER_SIZE_512 / 2) * _numChannels, (BUFFER_SIZE_512 / 2 ) * _numChannels);
					_mix256Order++;

					if (_mix256Order > 1 )
					{
						_mix256Order = 0;
						PushAudioFrames(_mix256Buffer);
					}
				}
				else
				{
					for (int i = 0; i < (_bufferSize / BUFFER_SIZE_512); i++)
					{
						float[] slice = new float[BUFFER_SIZE_512 * _numChannels];
						Array.Copy(data, i * BUFFER_SIZE_512 * _numChannels, slice, 0, BUFFER_SIZE_512 * _numChannels);
						PushAudioFrames(slice);
					}
				}
			}
		}

		public void SendAudioData(HlrAudioData audioData)
		{
			_serverApi.Connection.PushAudioData(_handle, audioData);
		}

		public static bool IsAudioSettingsSupported(int sampleRate, int speakerModeNumChannels)
		{
			if (sampleRate != KHZ48_VAL ||
				speakerModeNumChannels != STEREO_NUM_CHANNELS)
			{
				throw new Exception($"Error creating Unity audio source for ISAR: " +
					$"We currently support only stereo 48KHZ audio settings." +
					$"Check Edit->Project Settings->Audio");
			}

			return true;
		}

		public override void Dispose()
		{
			base.Dispose();
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
			HlrError err = QrApi.Start();
			if (err != HlrError.eNone)
			{
				throw new Exception($"Failed to start QR code API, error: {err}");
			}
		}

		public void Stop()
		{
			HlrError err = QrApi.Stop();
			if (err != HlrError.eNone)
			{
				throw new Exception($"Failed to stop QR code API, error: {err}");
			}
		}

		public void IsSupported()
		{
			HlrError err = QrApi.IsSupported();
			if (err != HlrError.eNone)
			{
				throw new Exception($"IsSupported failed with error: {err}");
			}
		}

		public void RequestAccess()
		{
			HlrError err = QrApi.RequestAccess();
			if (err != HlrError.eNone)
			{
				throw new Exception($"RequestAccess failed with error: {err}");
			}
		}

		public void ProcessMessages()
		{
			QrApi.ProcessMessages();
		}

		public override void Dispose()
		{
			QrApi.EnumerationCompleted -= QrApi_OnEnumerationCompleted;
			QrApi.IsSupportedReceived -= QrApi_OnIsSupportedReceived;
			QrApi.AccessStatusReceived -= QrApi_OnAccessStatusReceived;
			QrApi.Added -= QrApi_OnAdded;
			QrApi.Updated -= QrApi_OnUpdated;
			QrApi.Removed -= QrApi_OnRemoved;

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

	public class IsarClientCamera : IsarCustomSend
	{
		public struct CameraIntrinsics
		{
			public UInt32 width;
			public UInt32 height;
			public float focal_length_x;
			public float focal_length_y;
			public float camera_model_principal_point_x;
			public float camera_model_principal_point_y;
			public float distortion_model_radial_k1;
			public float distortion_model_radial_k2;
			public float distortion_model_radial_k3;
			public float distortion_model_tangential_p1;
			public float distortion_model_tangential_p2;
		}

		public struct CameraMetadata
		{
			public CameraIntrinsics intrinsics; // 44
			public HlrMatrix4x4 extrinsics;    // 64
		} // 108


		public delegate void IsarClientCameraMetadataCallback(Int64 captureTimeHNS, in CameraMetadata metadata);
		public event IsarClientCameraMetadataCallback MetadataReceived;

		public IsarClientCamera() : base()
		{
			ConnectionStateChanged += OnConnectionStateChanged;
			CustomMessageReceived += OnCustomMessageReceived;
		}

		public override void Dispose()
		{
			if (IsConnected && _toggle) {
					Send(MessageType.CAMERA_DISABLE);
			}
			CustomMessageReceived -= OnCustomMessageReceived;
			ConnectionStateChanged -= OnConnectionStateChanged;

			base.Dispose();
		}

		private void OnCustomMessageReceived(in HlrCustomMessage message)
		{
			Shared.Assertions.Assert(message.Length >= Marshal.SizeOf<Int32>());
			IntPtr dataPtr = message.Data;
			MessageType messageType = (MessageType)Marshal.ReadInt32(dataPtr);
			dataPtr = IntPtr.Add(dataPtr, Marshal.SizeOf<Int32>());
			if (messageType != MessageType.CAMERA_METADATA)
				return;

			Shared.Assertions.Assert(message.Length == Marshal.SizeOf<Int32>() + Marshal.SizeOf<CameraMetadata>() + Marshal.SizeOf<Int64>());

			var cameraMetadata = Marshal.PtrToStructure<CameraMetadata>(dataPtr);
			dataPtr = IntPtr.Add(dataPtr, Marshal.SizeOf<CameraMetadata>());

			Int64 captureTimeNS = Marshal.ReadInt64(dataPtr);

			MetadataReceived?.Invoke(captureTimeNS, cameraMetadata);
		}

		// We currently only support these hardcoded frame attributes
		public static readonly int WIDTH = 1504;
		public static readonly int HEIGHT = 846;
		//public static readonly int BIT_COUNT = 12; // nv12
		public static readonly int BIT_COUNT = 32; // rgba
		public static readonly int SIZE = (WIDTH * HEIGHT * BIT_COUNT) / sizeof(byte);
		public static readonly UnityEngine.TextureFormat FORMAT = UnityEngine.TextureFormat.RGBA32;

		public bool TryDequeueFrame(ref Int64 captureTimeHNS, IntPtr unmanagedData)
		{
			if (!IsConnected) return false;

			// i420
			return Convert.ToBoolean(HlrSvApi.CameraDequeue(unmanagedData, SIZE, ref captureTimeHNS));
		}

		private bool _toggle;
		public bool Toggle
		{
			get => _toggle;
			set
			{
				if(_toggle == value) return;
				_toggle = value;
				if (IsConnected)
				{
					Send(value ? MessageType.CAMERA_ENABLE : MessageType.CAMERA_DISABLE);
				}
			}
		}

		private void OnConnectionStateChanged(HlrConnectionState state)
		{
			if (state == HlrConnectionState.Connected)
			{
				if (_toggle)
				{
					Send(MessageType.CAMERA_ENABLE);
				}
			}
		}
	}
}
