
//#define USE_Signaling_FALLBACK

using UnityEngine;
using UnityEngine.XR.Management;
using System.Threading;
using HoloLight.Isar.Native;
using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks;
using HoloLight.Isar.Signaling;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HoloLight.Isar
{
	/// <summary>
	/// Takes care of initializing the native library and calls the display/input providers' lifecycle methods.
	/// </summary>
	public abstract class BaseIsarLoader : XRLoaderHelper
	{
		#region MemberVariables
		//KL: we could use Isar class instance here for init/close instead of raw API.

		//Used to post calls on Unity's main thread. Useful when we want to call into remoting lib while in a native callback.
		//Here's a lengthy read why this is useful:
		//https://docs.microsoft.com/en-us/archive/msdn-magazine/2011/february/msdn-magazine-parallel-computing-it-s-all-about-the-synchronizationcontext
		private SynchronizationContext _syncContext;

		private ISignaling _signaling;
		private HlrSvApi _serverApi;
		private HlrHandle _handle = new HlrHandle();

		private HlrConnectionState _connectionState;
		private object _connectionStateLock = new object();
		public HlrConnectionState ConnectionState {
			get
			{
				HlrConnectionState state;
				lock (_connectionStateLock)
				{
					state = _connectionState;
				}
				return state;
			}
			private set {
				lock (_connectionStateLock)
				{
					_connectionState = value;
				};
			}
		}

		private object _isRunningLock = new object();
		private bool _isRunning = false;
		public bool IsRunning {
			get {
				lock (_isRunningLock) { return _isRunning; }
			}
			private set {
				lock (_isRunningLock) { _isRunning = value; }
			}
		}

		// This exists because signaling callbacks can be executed on threads other than Unity's script thread (async etc.),
		// so we can't call GetXRSettings in OnConnected.
		bool _sendDepthAlpha = false;
		#endregion

		#region Structures
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
		#endregion

		#region DllImports
		[DllImport("remoting_unity")]
		static extern void SetUserDefinedSettings(UserDefinedSettings settings);

		[DllImport("remoting_unity")]
		public static extern void SetBitrate(Int64 bitrate);
		#endregion

		/// <summary>
		/// Get the settings
		/// </summary>
		/// <returns>The settings class</returns>
		private IsarXRSettings GetXRSettings()
		{
#if !UNITY_EDITOR
		return IsarXRSettings.s_Settings;
#else
			IsarXRSettings settingsObj = null;
			EditorBuildSettings.TryGetConfigObject(Constants.ISAR_XR_SETTINGS_KEY, out settingsObj);
			return settingsObj;
#endif
		}

		public sealed override bool Initialize()
		{
			//bool retVal = true;

			Debug.Log("===== Initialize =====");

			_syncContext = SynchronizationContext.Current;

			string configPath = Application.streamingAssetsPath + "/remoting-config.cfg";
			HlrSdpCreatedCallback sdpCallback = Callbacks.OnSdpCreated;
			HlrLocalIceCandidateCreatedCallback iceCallback = Callbacks.OnLocalIceCandidateCreated;

			IsarXRSettings xrSettings = GetXRSettings();
			if (xrSettings == null)
			{
				//throw new Exception("Failed to get XR settings");
				return false;
			}

			_sendDepthAlpha = xrSettings.SendDepthAlpha_Preview;

			_signaling = (ISignaling)Activator.CreateInstance(Type.GetType(xrSettings.SignalingImplementationType));

			UserDefinedSettings settings = new UserDefinedSettings(configPath, sdpCallback, iceCallback, xrSettings.RenderingMode, Convert.ToInt32(_sendDepthAlpha));
			SetUserDefinedSettings(settings);

			if (!CreateSubsystems())
			{
				//throw new Exception("Failed to create subsystems");
				return false;
			}

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
			// TODO
			//_serverApi.Connection.RegisterCustomMessageHandler(_handle, Callbacks.OnCustomMessageReceived, IntPtr.Zero);

			Callbacks.SdpCreated += Callbacks_SdpCreated;
			Callbacks.LocalIceCandidateCreated += Callbacks_LocalIceCandidateCreated;
			Callbacks.ConnectionStateChanged += Callbacks_ConnectionStateChanged;

			// NOTE: this should not be necessary
			//lock (_isRunningLock) { _isRunning = false; }

			return true;
		}

		public sealed override bool Start()
		{
			if (!StartSubsystems())
			{
				return false;
			}

			_signaling.Listen();

			IsRunning = true;

			return true;
		}

		public sealed override bool Stop()
		{
			Debug.Log("===== Stop =====");

			_signaling.Dispose();

			bool retVal = StopSubsystems();

			// TODO: move to beginning of this function?
			IsRunning = false;

			return retVal;
		}

		// TODO: move signaling & flow control out of loader and into user code (allow automatic & custom xr loading)
		private bool HACK_isDeinitializing = false;
		public sealed override bool Deinitialize()
		{
			Debug.Log("===== Deinitialize =====");

			_signaling.IceCandidateReceived -= Signaling_IceCandidateReceived;
			_signaling.SdpAnswerReceived -= Signaling_SdpAnswerReceived;
			_signaling.Connected -= Signaling_OnConnected;
			//OnDeinitialized(); ?

			if (ConnectionState == HlrConnectionState.Connected)
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

			return DestroySubsystems();
		}

		protected abstract bool CreateSubsystems();
		protected abstract bool StartSubsystems();
		protected abstract bool StopSubsystems();
		protected abstract bool DestroySubsystems();

		#region ServerEventHandlers
		/// <summary>
		/// Callback indicating the connection state has changed
		/// </summary>
		/// <param name="newState">States whether connected or disconnected</param>
		private void Callbacks_ConnectionStateChanged(HlrConnectionState newState)
		{
			HlrConnectionState prevState;
			// atomic.exchange
			lock (_connectionStateLock)
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
							lock (_isRunningLock)
							{
								if (_isRunning)
								{
									Stop();
									Start();
								}
							}
						}, newState);
					}
					//OnDisconnected();
					break;
				case HlrConnectionState.Failed:
					if (prevState == HlrConnectionState.Connected)
					{
						if (!HACK_isDeinitializing)
						{
							//Post makes an asynchronous call the next time Unity's SynchronizationContext impl does some processing.
							//Here's Unity's reference source:
							//https://github.com/Unity-Technologies/UnityCsReference/blob/2019.4/Runtime/Export/Scripting/UnitySynchronizationContext.cs
							_syncContext.Post((state) =>
							{
								lock (_isRunningLock)
								{
									if (_isRunning)
									{
										Stop();
										Start();
									}
								}
							}, newState);
						}
					}
					else if (prevState == HlrConnectionState.Disconnected)
					{
						_signaling.Dispose();
					}
					break;
				default:
					Debug.LogError("Unhandled ConnectionState");
					break;
			}
		}

		/// <summary>
		/// SDP Created callback
		/// </summary>
		/// <param name="type">Offer or answer</param>
		/// <param name="sdp">Connection description</param>
		/// <param name="userData"></param>
		private async void Callbacks_SdpCreated(HlrSdpType type, string sdp, IntPtr userData)
		{
			await _signaling.SendOfferAsync(sdp);
		}

		/// <summary>
		/// ICE Candidate created callback
		/// </summary>
		/// <param name="mId">I</param>
		/// <param name="mLineIndex"></param>
		/// <param name="candidate"></param>
		/// <param name="userData"></param>
		private async void Callbacks_LocalIceCandidateCreated(string mId, int mLineIndex, string candidate, IntPtr userData)
		{
			await _signaling.SendIceCandidateAsync(mId, mLineIndex, candidate);
		}
		#endregion

		#region SignalingEventHandlers
		/// <summary>
		/// ICE Candidate received callback
		/// </summary>
		/// <param name="mId"></param>
		/// <param name="mLineIndex"></param>
		/// <param name="candidate"></param>
		private void Signaling_IceCandidateReceived(string mId, int mLineIndex, string candidate)
		{
			var err = _serverApi.Signaling.AddIceCandidate(_handle, mId, mLineIndex, candidate);

			if (err != HlrError.eNone)
			{
				Debug.LogError("Failed to add ICE candidate");
			}
		}

		/// <summary>
		/// SDP answer received callback
		/// </summary>
		/// <param name="sdp"></param>
		private void Signaling_SdpAnswerReceived(string sdp)
		{
			Debug.Log($"Received answer SDP: {sdp}");

			var err = _serverApi.Signaling.SetRemoteAnswer(_handle, sdp);

			if (err != HlrError.eNone)
			{
				Debug.LogError("Failed to set remote answer");
			}
		}

		/// <summary>
		/// Signaling connection callback
		/// </summary>
		/// <returns></returns>
		private void Signaling_OnConnected()
		{
			Task.Run(async () => await _signaling.SendVersionAsync(_serverApi.Version.PackedValue, _sendDepthAlpha)).Wait();

			HlrError err = _serverApi.Signaling.CreateOffer(_handle);

			if (err != HlrError.eNone)
			{
				Debug.LogError("Failed to create offer");
			}
		}
		#endregion
	}
}
