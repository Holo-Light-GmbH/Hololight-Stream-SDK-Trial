/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;
using System.Runtime.InteropServices;
using HoloLight.Isar.Native;
using HoloLight.Isar.Native.CustomMessage;
using HoloLight.Isar.Native.Qr;
using UnityEngine.XR.Management;
using UnityEngine;

namespace HoloLight.Isar
{
	/// <summary>
	/// Provides basic access to Isar library functionality, such as initializing/closing and connection state events.
	/// </summary>
	public class Isar : IDisposable
	{
		public event BaseIsarLoader.ConnectionStateChangedCallback ConnectionStateChanged; // TODO: get rid of this and use loaders events directly
		public BaseIsarLoader loader;
		private object _lockObj = new object();
		private HlrConnectionState _connectionState = HlrConnectionState.Disconnected;
		protected HlrConnectionState _previousConnectionState = HlrConnectionState.Disconnected; // HACK
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

		// TODO: maybe don't throw exceptions in ctors and create an init method
		// ref: https://stackoverflow.com/questions/77639/when-is-it-right-for-a-constructor-to-throw-an-exception
		public Isar()
		{
			// ISAR XR loader check
			loader = XRGeneralSettings.Instance.Manager.activeLoader as BaseIsarLoader;
			if (loader == null)
			{
				throw new NullReferenceException("There is no ISAR loader active." +
					" ISAR cannot be used. " +
					"Make sure to enable an ISAR Loader " +
					"through Edit -> Project Settings ->" +
					"XR Plug-in Management before using this class.");
			}


			_serverApi = new HlrSvApi();
			// TODO: ensure that this is called from the main thread
			HlrError err = HlrSvApi.Create(ref _serverApi);

			// TODO: improve exception handling; don't throw base "Exception"s
			// ref: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/creating-and-throwing-exceptions#things-to-avoid-when-throwing-exceptions
			if (err != HlrError.eNone)
			{
				throw new Exception($"Isar initialization failed with {err}");
			}

			if (err != HlrError.eNone)
			{
				throw new Exception("Failed to init remoting lib");
			}

			_connectionState = loader.ConnectionState;
			loader.ConnectionStateChanged += OnConnectionStateChanged;
		}

		public virtual void Dispose()
		{
			loader.ConnectionStateChanged -= OnConnectionStateChanged;
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
			loader.ViewPoseReceived += OnViewPoseReceived;
		}

		private void OnViewPoseReceived(in Native.Input.HlrXrPose viewPose)
		{
			ViewPoseReceived?.Invoke(viewPose);
		}

		public override void Dispose()
		{
			loader.ViewPoseReceived -= OnViewPoseReceived;
			base.Dispose();
		}
	}

	public class IsarAudio : Isar
	{
		public event BaseIsarLoader.AudioDataReceivedCallback AudioDataReceived;

		public IsarAudio() : base()
		{
			loader.AudioDataReceived += OnAudioDataReceived;
		}

		public void SetAudioTrackEnabled(bool enable)
		{
			HlrError err = _serverApi.Connection.SetAudioTrackEnabled(loader.HlrHandle, Convert.ToInt32(enable));
			if (err != HlrError.eNone)
			{
				throw new InvalidOperationException($"Failed to set audio track to {enable}");
			}
		}

		private void OnAudioDataReceived(in HlrAudioData audioData)
		{
			AudioDataReceived?.Invoke(audioData);
		}

		public override void Dispose()
		{
			loader.AudioDataReceived -= OnAudioDataReceived;
			base.Dispose();
		}
	}

	public class IsarCustomSend : Isar
	{
		public event BaseIsarLoader.CustomMessageCallback CustomMessageReceived;

		public IsarCustomSend() : base()
		{
			loader.CustomMessageReceived += OnCustomMessageReceived;
		}

		public override void Dispose()
		{
			loader.CustomMessageReceived -= OnCustomMessageReceived;

			base.Dispose();
		}

		public void Send(MessageType type, byte[] bytes = null)
		{
			if (!IsConnected) throw new ArgumentException("Trying to send a custom message but the object is not currently connected to the client");

			byte[] typeBytes = BitConverter.GetBytes((int)type);
			int messageLength = typeBytes.Length + (bytes != null ? bytes.Length : 0);

			IntPtr unmanagedPtr = IntPtr.Zero;
			try
			{
				unmanagedPtr = Marshal.AllocHGlobal(messageLength);
				Marshal.Copy(typeBytes, 0, unmanagedPtr, typeBytes.Length);
				if (bytes != null)
				{
					Marshal.Copy(bytes, 0, unmanagedPtr + typeBytes.Length, bytes.Length);
				}

				HlrCustomMessage message = new HlrCustomMessage();
				message.Length = (uint)messageLength;
				message.Data = unmanagedPtr;

				PushCustomMessage(message);
			}
			finally
			{
				Marshal.FreeHGlobal(unmanagedPtr);
			}
		}

		public void PushCustomMessage(HlrCustomMessage message)
		{
			HlrError err = _serverApi.Connection.PushCustomMessage(loader.HlrHandle, message);
			if (err != HlrError.eNone)
			{
				throw new Exception($"Failed to send custom message with error {err}");
			}
		}

		private void OnCustomMessageReceived(in HlrCustomMessage message)
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
			audioData.SamplesPerChannel = (UIntPtr)KHZ_48_NUM_SAMPLES;
			audioData.BitsPerSample = BITS_16_PER_SAMPLE;
			audioData.NumberOfChannels = (UIntPtr)STEREO_NUM_CHANNELS;
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
			if (!IsConnected)
				return;

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
			_serverApi.Connection.PushAudioData(loader.HlrHandle, audioData);
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
			QrApi.Init(_serverApi.Connection, loader.HlrHandle);
			QrApi.EnumerationCompleted += QrApi_OnEnumerationCompleted;
			QrApi.IsSupportedReceived += QrApi_OnIsSupportedReceived;
			QrApi.AccessStatusReceived += QrApi_OnAccessStatusReceived;
			QrApi.Added += QrApi_OnAdded;
			QrApi.Updated += QrApi_OnUpdated;
			QrApi.Removed += QrApi_OnRemoved;

			var msgCallbacks = new HlrSvMessageCallbacks(null, null, QrApi.OnIsSupported, QrApi.OnRequestAccess, QrApi.OnAdded, QrApi.OnUpdated, QrApi.OnRemoved, QrApi.OnEnumerationCompleted);
			_serverApi.Connection.RegisterMessageCallbacks(loader.HlrHandle, ref msgCallbacks);
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

	public class IsarStatsCollector : Isar
	{
		public delegate void IsarStatsCallback(in IntPtr stats);
		public event IsarStatsCallback StatsDelivered;

		//public class AsyncOperationBase : CustomYieldInstruction
		//{
		//	public bool IsError { get; internal set; }
		//	public bool IsDone { get; internal set; }

		//	public override bool keepWaiting
		//	{
		//		get
		//		{
		//			if (IsDone)
		//			{
		//				return false;
		//			}
		//			else
		//			{
		//				return true;
		//			}
		//		}
		//	}

		//	internal void Done()
		//	{
		//		IsDone = true;
		//	}
		//}

		//public class RTCStatsReportAsyncOperation : AsyncOperationBase
		//{
		//	public IntPtr Value { get; private set; }

		//	internal RTCStatsReportAsyncOperation(IsarStatsCollector collector)
		//	{
		//		collector.OnStatsDelivered = ptr =>
		//		{
		//			Value = ptr;
		//			IsError = false;
		//			this.Done();
		//		};
		//	}
		//}

		public IsarStatsCollector() : base()
		{
			_serverApi.Connection.RegisterStatsCallbackHandler(loader.HlrHandle, Callbacks.OnStatsReceived, IntPtr.Zero);
			Callbacks.StatsReceived += OnStatsReceived;
		}

		public override void Dispose()
		{
			Callbacks.StatsReceived -= OnStatsReceived;
			_serverApi.Connection.UnregisterStatsCallbackHandler(loader.HlrHandle, Callbacks.OnStatsReceived, IntPtr.Zero);

			base.Dispose();
		}

		public void GetStats()
		{
			_serverApi.Connection.GetStats(loader.HlrHandle);
		}

		public void OnStatsReceived(IntPtr statsData, IntPtr userData)
		{
			StatsDelivered?.Invoke(statsData);
		}
	}

	public class IsarClientCamera : IsarCustomSend
	{
		private object _dataLock = new object();
		private bool _toggle;

		public struct CameraConfiguration
		{
			public int width;
			public int height;
			public float framerate;

			public override bool Equals(object obj) => obj is CameraConfiguration other && this.Equals(other);
			public bool Equals(CameraConfiguration rhs)
			{
				return width == rhs.width
					&& height == rhs.height
					&& framerate == rhs.framerate;
			}
			public override int GetHashCode() => (width, height, framerate).GetHashCode();
			public static bool operator ==(CameraConfiguration lhs, CameraConfiguration rhs) => lhs.Equals(rhs);
			public static bool operator !=(CameraConfiguration lhs, CameraConfiguration rhs) => !(lhs == rhs);
		}
		public static readonly CameraConfiguration[] SupportedResolutions = {
			new CameraConfiguration{ width = 1952, height=1100, framerate=60 },
			new CameraConfiguration{ width = 1504, height=846,  framerate=60 },
			new CameraConfiguration{ width = 1504, height=846,  framerate=30 },
			new CameraConfiguration{ width = 1920, height=1080, framerate=30 },
			new CameraConfiguration{ width = 1280, height=720,  framerate=30 },
			new CameraConfiguration{ width = 1952, height=1100, framerate=30 },
			new CameraConfiguration{ width = 640,  height=360,  framerate=30 },
			new CameraConfiguration{ width = 760,  height=428,  framerate=30 },
			new CameraConfiguration{ width = 960,  height=540,  framerate=30 },
			new CameraConfiguration{ width = 1128, height=636,  framerate=30 },
			new CameraConfiguration{ width = 424,  height=240,  framerate=30 },
			new CameraConfiguration{ width = 500,  height=282,  framerate=30 },
			new CameraConfiguration{ width = 1504, height=846,  framerate=15 },
			new CameraConfiguration{ width = 1920, height=1080, framerate=15 },
			new CameraConfiguration{ width = 1280, height=720,  framerate=15 },
			new CameraConfiguration{ width = 1952, height=1100, framerate=15 },
			new CameraConfiguration{ width = 640,  height=360,  framerate=15 },
			new CameraConfiguration{ width = 760,  height=428,  framerate=15 },
			new CameraConfiguration{ width = 960,  height=540,  framerate=15 },
			new CameraConfiguration{ width = 1128, height=636,  framerate=15 },
			new CameraConfiguration{ width = 424,  height=240,  framerate=15 },
			new CameraConfiguration{ width = 500,  height=282,  framerate=15 },
			new CameraConfiguration{ width = 1504, height=846,  framerate=5  },
		};
		public static readonly CameraConfiguration DefaultConfiguration = SupportedResolutions[2];
		private CameraConfiguration _config = DefaultConfiguration;

		public struct CameraSettings
		{
			public bool autoExposure;
			private const byte _padding0 = 0;
			private const Int16 _padding1 = 0;
			private const Int32 _padding2 = 0;
			public Int64 exposure;
			public float exposureCompensation;
			public Int32 whiteBalance;
		}
		public static readonly CameraSettings DefaultSettings = new CameraSettings
		{
			autoExposure = EXPOSURE_AUTO_DEFAULT_VALUE,
			exposure = EXPOSURE_DEFAULT_VALUE,
			exposureCompensation = EXPOSURE_COMPENSATION_DEFAULT_VALUE,
			whiteBalance = WHITE_BALANCE_DEFAULT_VALUE
		};
		private CameraSettings _settings = DefaultSettings;

		public struct CameraIntrinsics
		{
			public UInt32 width;
			public UInt32 height;
			public float focalLengthX;
			public float focalLengthY;
			public float cameraModelPrincipalPointX;
			public float cameraModelPrincipalPointY;
			public float distortionModelRadialK1;
			public float distortionModelRadialK2;
			public float distortionModelRadialK3;
			public float distortionModelTangentialP1;
			public float distortionModelTangentialP2;
		}

		public struct CameraMetadata
		{
			public CameraIntrinsics intrinsics; // 44
			public HlrMatrix4x4 extrinsics;    // 64
			private const Int32 _padding0 = 0;
			public CameraSettings settings;
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
			CustomMessageReceived -= OnCustomMessageReceived;
			ConnectionStateChanged -= OnConnectionStateChanged;

			base.Dispose();
		}

		private void OnConnectionStateChanged(HlrConnectionState state)
		{
			if (state == HlrConnectionState.Connected)
			{
				// TODO: dispatch onto unity main thread asynchronously so we dont need _dataLock around settings
				SendSettings();
			}
		}

		private void OnCustomMessageReceived(in HlrCustomMessage message)
		{
			lock (_dataLock)
			{
				// NOTE: there is a delay between server & client toggle
				if (_toggle == false) return;
			}

			IntPtr dataPtr = message.Data;
			int dataLength = (int)message.Length;
			Shared.Assertions.Assert(dataLength >= Marshal.SizeOf<Int32>());
			MessageType messageType = (MessageType)Marshal.ReadInt32(dataPtr);
			dataPtr = IntPtr.Add(dataPtr, Marshal.SizeOf<Int32>());
			dataLength -= Marshal.SizeOf<Int32>();
			if (messageType != MessageType.CAMERA_METADATA)
				return;

			// skip padding
			dataPtr = IntPtr.Add(dataPtr, Marshal.SizeOf<Int32>());
			dataLength -= Marshal.SizeOf<Int32>();

			Shared.Assertions.Assert(dataLength >= Marshal.SizeOf<Int64>());
			Int64 captureTimeNS = Marshal.ReadInt64(dataPtr);
			dataPtr = IntPtr.Add(dataPtr, Marshal.SizeOf(captureTimeNS));
			dataLength -= Marshal.SizeOf<Int64>();

			Shared.Assertions.Assert(dataLength >= Marshal.SizeOf<CameraMetadata>());
			var metadata = Marshal.PtrToStructure<CameraMetadata>(dataPtr);
			dataPtr = IntPtr.Add(dataPtr, Marshal.SizeOf<CameraMetadata>());
			dataLength -= Marshal.SizeOf<CameraMetadata>();

			MetadataReceived?.Invoke(captureTimeNS, metadata);
		}

		public static readonly int DEFAULT_WIDTH = 1504;
		public static readonly int DEFAULT_HEIGHT = 846;
		//public static readonly int BIT_COUNT = 12; // nv12
		public static readonly UnityEngine.TextureFormat DEFAULT_FORMAT = UnityEngine.TextureFormat.RGBA32;

		public bool TryAcquireLatestCameraImage(ref UnityEngine.TextureFormat unityFormat, ref Int32 width, ref Int32 height, ref Int64 timestamp)
		{
			if (!IsConnected) return false;

			HlrTextureFormat hlrFormat = HlrTextureFormat.RGBA32;

			bool retVal = HlrSvApi.TryAcquireLatestCameraImage(ref hlrFormat, ref width, ref height, ref timestamp) == HlrError.eNone;

			switch (hlrFormat)
			{
				case HlrTextureFormat.RGBA32:
					unityFormat = TextureFormat.RGBA32;
					break;
				default:
					Debug.Log("Unsupported texture format received from GetCameraFrameDesc.");
					retVal = false;
					break;
			}

			return retVal;
		}

		public bool TryAcquireCameraImageBuffer(IntPtr unmanagedData, int unmanagedDataSize)
		{
			if (!IsConnected) return false;

			return HlrSvApi.TryAcquireCameraImageBuffer(unmanagedData, unmanagedDataSize) == HlrError.eNone;
		}

		public void Reconfigure(bool toggle, CameraConfiguration config, CameraSettings settings)
		{
			lock (_dataLock)
			{
				_toggle = toggle;
				_config = config;
				_settings = settings;
			}

			if (!IsConnected) return;

			SendSettings();
		}

		private void SendSettings()
		{
			MessageType messageType = MessageType.INVALID;
			byte[] bytes = null;
			lock (_dataLock)
			{
				messageType = _toggle ? MessageType.CAMERA_ENABLE : MessageType.CAMERA_DISABLE;
				if (_toggle == true)
				{
					int byteSize = Marshal.SizeOf<CameraConfiguration>() + Marshal.SizeOf<CameraSettings>();
					bytes = new byte[byteSize];
					IntPtr ptr = IntPtr.Zero;
					try
					{
						ptr = Marshal.AllocHGlobal(byteSize);
						Marshal.StructureToPtr(_config, ptr, true);
						Marshal.StructureToPtr(_settings, IntPtr.Add(ptr, Marshal.SizeOf(_config)), true);
						Marshal.Copy(ptr, bytes, 0, byteSize);
					}
					finally
					{
						Marshal.FreeHGlobal(ptr);
					}
				}
			}
			Send(messageType, bytes);
		}

		//public bool ExposureSupported() { return true; }
		public static Int64 ExposureMax() { return 660000; }
		public static Int64 ExposureMin() { return 1000; }
		public static Int64 ExposureStep() { return 10; }
		public const Int64 EXPOSURE_DEFAULT_VALUE = 1000;

		// Gets a value that indicates if auto exposure is enabled.
		public const bool EXPOSURE_AUTO_DEFAULT_VALUE = true;
		public bool ExposureAuto() { return _settings.autoExposure; }
		public void ExposureSetAuto(bool value) { _settings.autoExposure = value; }
		public Int64 ExposureValue() { return _settings.exposure; }
		public void ExposureSetValue(Int64 ticks)
		{
			// contract checks/clamps
			if (ticks < ExposureMin()
			 || ticks > ExposureMax()
			 || ticks != (long)Math.Round((double)ticks / ExposureStep()) * ExposureStep())
				throw new ArgumentOutOfRangeException("Ensure the value is within the allowed range using the Min, Max and Step functions.");

			_settings.exposure = ticks;
		}

		//public bool ExposureCompensationSupported() { return true; }
		public static float ExposureCompensationMax() { return 2.0000064f; }
		public static float ExposureCompensationMin() { return -2.0000064f; }
		public static float ExposureCompensationStep() { return 0.166667f; }
		public const float EXPOSURE_COMPENSATION_DEFAULT_VALUE = 0.0f;
		public float ExposureCompensationValue() => _settings.exposureCompensation;
		public void ExposureCompensationSetValue(float value)
		{
			// contract checks/clamps
			if (value < ExposureCompensationMin()
			 || value > ExposureCompensationMax()
			 || !Mathf.Approximately(value, (float)Math.Round((double)value / ExposureCompensationStep()) * ExposureCompensationStep()))
				throw new ArgumentOutOfRangeException("Ensure the value is within the allowed range using the Min, Max and Step functions.");

			_settings.exposureCompensation = value;
		}

		//public bool WhiteBalanceSupported() { return true; }
		public static int WhiteBalanceMax() { return 7500; }
		public static int WhiteBalanceMin() { return 2300; }
		public static int WhiteBalanceStep() { return 25; }
		public const int WHITE_BALANCE_DEFAULT_VALUE = 2300;
		public float WhiteBalanceValue() => _settings.whiteBalance;
		public void WhiteBalanceSetValue(int value)
		{
			// contract checks/clamps
			if (value > WhiteBalanceMax()
			 || value < WhiteBalanceMin()
			 || value != (int)Math.Round((double)value / WhiteBalanceStep()) * WhiteBalanceStep())
				throw new ArgumentOutOfRangeException("Ensure the value is within the allowed range using the Min, Max and Step functions.");

			_settings.whiteBalance = value;
		}
	}
}
