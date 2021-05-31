
//#define USE_SIGNALING_FALLBACK

using System;
using System.Collections.Generic;
using System.Net;
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

		/// <summary>
		/// Settings that need to be passed into the plugin BEFORE any initialization.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		struct UserDefinedSettings
		{
			public UserDefinedSettings(string configPath, HlrSdpCreatedCallback sdpCb, HlrLocalIceCandidateCreatedCallback iceCb, RenderingMode renderingMode)
			{
				this.configPath = configPath;
				this.sdpCreatedCb = sdpCb;
				this.localIceCandidateCreatedCb = iceCb;
				this.renderingMode = renderingMode;
			}

			[MarshalAs(UnmanagedType.LPWStr)]
			public string configPath;
			public HlrSdpCreatedCallback sdpCreatedCb;
			public HlrLocalIceCandidateCreatedCallback localIceCandidateCreatedCb;
			public RenderingMode renderingMode;
		}

		[DllImport("remoting_unity")]
		static extern void SetUserDefinedSettings(UserDefinedSettings settings);

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

			_signaling = (ISignaling)Activator.CreateInstance(Type.GetType(xrSettings.SignalingImplementationType));

			UserDefinedSettings settings = new UserDefinedSettings(configPath, sdpCallback, iceCallback, mode);
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
			
			return true;
		}

		public override bool Stop()
		{
			Debug.Log("===== Stop =====");

			var display = GetLoadedSubsystem<XRDisplaySubsystem>();
			if (display != null && display.running)
			{
				_signaling.Dispose();
			}

			StopSubsystem<XRInputSubsystem>();
			StopSubsystem<XRDisplaySubsystem>();

			return true;
		}

		public override bool Deinitialize()
		{
			Debug.Log("===== Deinitialize =====");

			_handle.Dispose();
			_signaling.IceCandidateReceived -= Signaling_IceCandidateReceived;
			_signaling.SdpAnswerReceived -= Signaling_SdpAnswerReceived;
			_signaling.Connected -= Signaling_OnConnected;

			Callbacks.ConnectionStateChanged -= Callbacks_ConnectionStateChanged;
			Callbacks.LocalIceCandidateCreated -= Callbacks_LocalIceCandidateCreated;
			Callbacks.SdpCreated -= Callbacks_SdpCreated;

			DestroySubsystem<XRInputSubsystem>();
			DestroySubsystem<XRDisplaySubsystem>();

			return true;
		}

		private void Callbacks_ConnectionStateChanged(HlrConnectionState newState)
		{
			lock (_lockObj)
			{
				_connectionState = newState;
			}

			Debug.Log($"ISAR connection state changed to {_connectionState}");

			if (newState == HlrConnectionState.Connected)
			{
				_signaling.Dispose();
			}
			else if (newState == HlrConnectionState.Disconnected)
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

		private async Task Signaling_OnConnected()
		{
			//This whole versioning thing still doesn't fit inside my head. What happens when
			//the client doesn't support ours? Do we ever get notified?
			//Answer: not right now, but this is one of signaling's responsibilities in a production app.
			await _signaling.SendVersionAsync(_serverApi.Version.PackedValue);

			//This leads to Callbacks_SdpCreated if successful
			HlrError err = _serverApi.Signaling.CreateOffer(_handle);

			if (err != HlrError.eNone)
			{
				Debug.LogError("Failed to create offer");
			}
		}
	}
}
