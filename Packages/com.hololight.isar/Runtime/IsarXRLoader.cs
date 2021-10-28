
//#define USE_SIGNALING_FALLBACK

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HoloLight.Isar.Native;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using System.Threading;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.XR.Isar
{
	/// <summary>
	/// Takes care of initializing the native library and calls the display/input providers' lifecycle methods.
	/// </summary>
	public class IsarXRLoader : XRLoaderHelper
	{
		//KL: we could use Isar class instance here for init/close instead of raw API.

		//Used to post calls on Unity's main thread. Useful when we want to call into remoting lib while in a native callback.
		//Here's a lengthy read why this is useful:
		//https://docs.microsoft.com/en-us/archive/msdn-magazine/2011/february/msdn-magazine-parallel-computing-it-s-all-about-the-synchronizationcontext
		SynchronizationContext _syncContext;

		ISignaling _signaling;
		HlrSvApi _serverApi;
		HlrHandle _handle = new HlrHandle();

		HlrConnectionState _connectionState;
		object _lockObj = new object();
		private bool IsConnected
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

		// This exists because signaling callbacks can be executed on threads other than Unity's script thread (async etc.),
		// so we can't call GetXRSettings in OnConnected.
		bool _sendDepthAlpha = false;

		/// <summary>
		/// Settings that need to be passed into the plugin BEFORE any initialization.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		struct UserDefinedSettings
		{
			public UserDefinedSettings(string configPath, HlrSdpCreatedCallback sdpCb, HlrLocalIceCandidateCreatedCallback iceCb, RenderingMode renderingMode, int sendDepthAlpha)
			{
				this.configPath = configPath;
				this.sdpCreatedCb = sdpCb;
				this.localIceCandidateCreatedCb = iceCb;
				this.renderingMode = renderingMode;
				this.sendDepthAlpha = sendDepthAlpha;
			}

			[MarshalAs(UnmanagedType.LPWStr)]
			public string configPath;
			public HlrSdpCreatedCallback sdpCreatedCb;
			public HlrLocalIceCandidateCreatedCallback localIceCandidateCreatedCb;
			public RenderingMode renderingMode;
			//This is treated as a bool, but those are not blittable (need marshaling) and I didn't want to
			//go there at the time of writing.
			public int sendDepthAlpha;
		}

		[DllImport("remoting_unity")]
		static extern void SetUserDefinedSettings(UserDefinedSettings settings);

		[DllImport("remoting_unity")]
		public static extern void SetBitrate(Int64 bitrate);
		private static List<XRDisplaySubsystemDescriptor> s_DisplaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();
		private static List<XRInputSubsystemDescriptor> s_InputSubsystemDescriptors = new List<XRInputSubsystemDescriptor>();

		IsarXRSettings GetXRSettings()
		{
#if !UNITY_EDITOR
		return IsarXRSettings.s_Settings;
#else
			IsarXRSettings settingsObj = null;
			EditorBuildSettings.TryGetConfigObject(Constants.ISAR_XR_SETTINGS_KEY, out settingsObj);
			return settingsObj;
#endif
		}

		public override bool Initialize()
		{
			Debug.Log("===== Initialize =====");

			_syncContext = SynchronizationContext.Current;

			string configPath = Application.streamingAssetsPath + "/remoting-config.cfg";
			HlrSdpCreatedCallback sdpCallback = Callbacks.OnSdpCreated;
			HlrLocalIceCandidateCreatedCallback iceCallback = Callbacks.OnLocalIceCandidateCreated;

			IsarXRSettings xrSettings = GetXRSettings();

			//NOTE: this will default to (RenderingMode)0, which equals MultiPass. I'm not sure of the circumstances
			//where we might run into XR settings being null, but if you ever get MultiPass where you wanted SinglePassInstanced,
			//check XR Plugin settings and the ScriptableObjects in Assets/XR.
			RenderingMode mode = default;
			if (xrSettings != null)
			{
				mode = xrSettings.RenderingMode;
			}

			_sendDepthAlpha = xrSettings.SendDepthAlpha;

			_signaling = (ISignaling)Activator.CreateInstance(Type.GetType(xrSettings.SignalingImplementationType));

			UserDefinedSettings settings = new UserDefinedSettings(configPath, sdpCallback, iceCallback, mode, Convert.ToInt32(_sendDepthAlpha));
			SetUserDefinedSettings(settings);

			CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(s_DisplaySubsystemDescriptors, "ISAR Display");
			CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(s_InputSubsystemDescriptors, "ISAR Input");

			_signaling.Connected += Signaling_OnConnected;
			_signaling.SdpAnswerReceived += Signaling_SdpAnswerReceived;
			_signaling.IceCandidateReceived += Signaling_IceCandidateReceived;

			//Init remoting library (or rather get its struct, because XR display already initialized it when we called CreateSubsystem)
			_serverApi = new HlrSvApi();
			HlrError err = HlrSvApi.Create(ref _serverApi);

			if (err != HlrError.eNone)
			{
				throw new Exception("Failed to create API struct");
			}

			HlrGraphicsApiConfig gfx = new HlrGraphicsApiConfig();
			HlrConnectionCallbacks cb = new HlrConnectionCallbacks();
			err = _serverApi.Connection.Init(null, gfx, cb, ref _handle);

			if (err != HlrError.eNone)
			{
				throw new Exception("Failed to init remoting lib");
			}

			//Kinda awful that we have to do this because it's easy to forget and other safehandle types don't do this.
			//Maybe something for the "nice" C# layer if we want one. Right now it's raw bindings in here.
			_handle.ConnectionApi = _serverApi.Connection;

			_serverApi.Connection.RegisterConnectionStateHandler(_handle, Callbacks.OnConnectionStateChanged, IntPtr.Zero);

			Callbacks.SdpCreated += Callbacks_SdpCreated;
			Callbacks.LocalIceCandidateCreated += Callbacks_LocalIceCandidateCreated;
			Callbacks.ConnectionStateChanged += Callbacks_ConnectionStateChanged;

			return true;
		}

		public override bool Start()
		{
			Debug.Log("===== Start =====");

			StartSubsystem<XRDisplaySubsystem>();
			StartSubsystem<XRInputSubsystem>();

			_signaling.Listen();
			//OnStarted();

			return true;
		}

		public override bool Stop()
		{
			Debug.Log("===== Stop =====");

			var input = GetLoadedSubsystem<XRInputSubsystem>();
			if (input != null && input.running)
			{
				StopSubsystem<XRInputSubsystem>();
			}

			var display = GetLoadedSubsystem<XRDisplaySubsystem>();
			if (display != null && display.running)
			{
				//OnStopping();
				_signaling.Dispose();
				StopSubsystem<XRDisplaySubsystem>();
				//OnStopped();
			}

			return true;
		}

		// TODO: move signaling & flow control out of loader and into user code (allow automatic & custom xr loading)
		private bool HACK_isDeinitializing = false;
		public override bool Deinitialize()
		{
			Debug.Log("===== Deinitialize =====");

			_signaling.IceCandidateReceived -= Signaling_IceCandidateReceived;
			_signaling.SdpAnswerReceived -= Signaling_SdpAnswerReceived;
			_signaling.Connected -= Signaling_OnConnected;
			//OnDeinitialized(); ?

			if (IsConnected)
			{
				HACK_isDeinitializing = true;
				Callbacks.OnConnectionStateChanged(HlrConnectionState.Disconnected, IntPtr.Zero);
				HACK_isDeinitializing = false;
			}

			Callbacks.ConnectionStateChanged -= Callbacks_ConnectionStateChanged;
			Callbacks.LocalIceCandidateCreated -= Callbacks_LocalIceCandidateCreated;
			Callbacks.SdpCreated -= Callbacks_SdpCreated;
			// TODO: we should call Callbacks_ConnectionStateChanged(HlrConnectionState.Disconnected); but because of our current architecture, we can't (as opposed to the hololens client)
			//if (IsConnected)
			//{
			//	lock (_lockObj)
			//	{
			//		_connectionState = HlrConnectionState.Disconnected;
			//	}
			//	Debug.Log($"ISAR connection state changed to {HlrConnectionState.Disconnected}");
			//}

			_handle.Dispose();

			DestroySubsystem<XRInputSubsystem>();
			DestroySubsystem<XRDisplaySubsystem>();

			return true;
		}

		private void Callbacks_ConnectionStateChanged(HlrConnectionState newState)
		{
			HlrConnectionState prevState;
			lock (_lockObj)
			{
				prevState = _connectionState;
				_connectionState = newState;
			}
			Debug.Log($"ISAR connection state changed to {newState}");

			switch (newState)
			{
				case HlrConnectionState.Connected:
					_signaling.Dispose();
					//OnConnected();
					break;
				case HlrConnectionState.Disconnected:
					if (!HACK_isDeinitializing)
					{
						//Post makes an asynchronous call the next time Unity's SynchronizationContext impl does some processing.
						//Here's Unity's reference source:
						//https://github.com/Unity-Technologies/UnityCsReference/blob/2019.4/Runtime/Export/Scripting/UnitySynchronizationContext.cs
						_syncContext.Post((state) =>
						{
							Stop();
							Start();
						}, newState);
					}
					//OnDisconnected();
					break;
				case HlrConnectionState.Failed:
					if (prevState==HlrConnectionState.Connected)
					{
						if (!HACK_isDeinitializing)
						{
							//Post makes an asynchronous call the next time Unity's SynchronizationContext impl does some processing.
							//Here's Unity's reference source:
							//https://github.com/Unity-Technologies/UnityCsReference/blob/2019.4/Runtime/Export/Scripting/UnitySynchronizationContext.cs
							_syncContext.Post((state) =>
							{
								Stop();
								Start();
							}, newState);
						}
					}
					else if (prevState==HlrConnectionState.Disconnected)
					{
						_signaling.Dispose();
					}
					break;
				default:
					Debug.LogError("Unhandled ConnectionState");
					break;
			}
		}

		private async void Callbacks_SdpCreated(HlrSdpType type, string sdp, IntPtr userData)
		{
			await _signaling.SendOfferAsync(sdp);
		}

		private async void Callbacks_LocalIceCandidateCreated(string mId, int mLineIndex, string candidate, IntPtr userData)
		{
			await _signaling.SendIceCandidateAsync(mId, mLineIndex, candidate);
		}

		private void Signaling_IceCandidateReceived(string mId, int mLineIndex, string candidate)
		{
			var err = _serverApi.Signaling.AddIceCandidate(_handle, mId, mLineIndex, candidate);

			if (err != HlrError.eNone)
			{
				Debug.LogError("Failed to add ICE candidate");
			}
		}

		private void Signaling_SdpAnswerReceived(string sdp)
		{
			Debug.Log($"Received answer SDP: {sdp}");

			var err = _serverApi.Signaling.SetRemoteAnswer(_handle, sdp);

			if (err != HlrError.eNone)
			{
				Debug.LogError("Failed to set remote answer");
			}
		}

		private void Signaling_OnConnected()
		{
			Task.Run(async () => await _signaling.SendVersionAsync(_serverApi.Version.PackedValue, _sendDepthAlpha)).Wait();

			HlrError err = _serverApi.Signaling.CreateOffer(_handle);

			if (err != HlrError.eNone)
			{
				Debug.LogError("Failed to create offer");
			}
		}
	}
}
