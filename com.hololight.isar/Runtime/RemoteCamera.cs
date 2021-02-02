#if ISAR_LEGACY
/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using Matrix4x4 = UnityEngine.Matrix4x4;
using HoloLight.Isar.Native;
using HoloLight.Isar.Native.Input;
using HoloLight.Isar.Signaling;
using HoloLight.Isar.Utils;
using Convert = HoloLight.Isar.Utils.Convert;
using Debug = UnityEngine.Debug;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace HoloLight.Isar
{
	[RequireComponent(typeof(Camera))]
	public class RemoteCamera : MonoBehaviour
	{
		private const string CONFIG_FILE = "remoting-config.cfg";
		private static readonly string CONFIG_PATH = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE);

		private IPAddress _signalingServerIpAddress = IPAddress.Any;
		private int _signalingServerPort = DebugSignaling.DEFAULT_PORT;

		private DebugSignaling _signaling;
		private DnsSd _dnsSd;
		public RemotingServer Server = new RemotingServer();

		/// <summary>
		/// Internal queue used to marshal work back to the script thread.
		/// </summary>
		private readonly ConcurrentQueue<Action> _scriptThreadQueue = new ConcurrentQueue<Action>();

		#region camera

		[SerializeField]
		private int eyeWidth = 1440;

		[SerializeField]
		private int eyeHeight = 936;

		private readonly Rect _leftEyeRect = new Rect(0, 0, 0.5f, 1f);
		private readonly Rect _rightEyeRect = new Rect(0.5f, 0, 0.5f, 1f);
		private readonly Rect _fullScreenRect = new Rect(0f, 0f, 1f, 1f);

		private Camera _camera;

		private IntPtr _remotingTexPtr = IntPtr.Zero;
		private RenderTexture _remotingTex;

		public bool ShowLocalOutput = true;

		private float _lastFrameTime = 0;

		#endregion camera

		// pose
		//private readonly AutoResetEvent _newPoseAvailableEvent = new AutoResetEvent(false);
		private StereoViewPose _currentPose;
		private readonly object _poseLock = new object();
		private bool _receivedAtLeastOnePose = false;
		private long _lastTimestamp = 0;

		void Start()
		{
			_camera = GetComponent<Camera>();

			Time.fixedDeltaTime = 0.016f;//setting fixed update to 60 fps
			_remotingTex = new RenderTexture(2 * eyeWidth, eyeHeight, 24, RenderTextureFormat.ARGB32);
			_remotingTex.Create();
			_camera.targetTexture = _remotingTex;
			_remotingTexPtr = _remotingTex.GetNativeTexturePtr();

			Server.GetGraphicsDeviceInfo(_remotingTexPtr, RemotingServer_OnDeviceInfoReceived);

		}

		private void RemotingServer_OnDeviceInfoReceived(GraphicsApiConfig gfxApiConfig)
		{
			Debug.Log($"D3d11Info received on thread {Thread.CurrentThread.ManagedThreadId}");

			//this assert isn't needed; lib API should return a descriptive error when config file
			//path is garbage.
			// TODO: set & validate this in the XRSDK preferences window
			Assert.IsTrue(File.Exists(CONFIG_PATH));
			_scriptThreadQueue.Enqueue(() => InitializeServer(gfxApiConfig, CONFIG_PATH));
		}

		private void InitializeServer(GraphicsApiConfig gfxConfig, string configPath)
		{
			//try
			//{
				// NOTE: this cannot throw
				Server.Initialize(gfxConfig, configPath, StartSignaling);
			//}
			//catch (Exception e)
			//{
			//	Debug.LogError($"RemoteCamera: Failed to init Server: {e}");
			//}
		}

		private void Reset()
		{
			StopSignaling();
			StartSignaling();
		}

		async void StartSignaling()
		{
			try
			{
				_dnsSd = new DnsSd(CONFIG_PATH);
				// TODO: test if this throws & try/catch to make this non-fatal
				_dnsSd.Advertise(_signalingServerPort);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}

			_signaling = new DebugSignaling();
			RegisterSignalingEventHandlers();

			// TODO: loop a couple times instead of recursive Reset() & exit/fail after x failed attempts
			try
			{
				await _signaling.Listen(_signalingServerIpAddress, _signalingServerPort).ConfigureAwait(false);
			}
			catch (TaskCanceledException)
			{
				Debug.Log("+++> StartSignaling: Connecting to signaling server cancelled by the user.");
				// ignored - expected behavior
			}
			catch (ObjectDisposedException ex)
			{
				Debug.Log(
					"+++> StartSignaling: tcp listener was shut down while waiting for connections: " + ex.Message);
			}
			catch (System.Net.Sockets.SocketException ex)
			{
				// repro: start a second server while the first one is still signaling
				var ex2 = ex; // HACK: Visual Studio Debugger shows "null" instead of the value of ex...
				Debug.LogError(ex.Message);
			}
			catch (IOException ex)
			{
				// repro: kill client during signaling
				// Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host.
				Debug.LogWarning(ex.Message);
				Reset();
			}
			catch (Exception ex)
			{
				switch ((uint)ex.HResult)
				{
					case 0x80004005: // SocketException
					// AddressNotAvailable: "The requested address is not valid in its context"
					// AccessDenied: "An attempt was made to access a socket in a way forbidden by its access permissions"
					// AlreadyInUse
					//System.Diagnostics.Debugger.Break();
					//goto case 0x8007274D;
					case 0x80072AF9: // "No such host is known. (Exception from HRESULT: 0x80072AF9)"
					case 0x8007274D: // "No connection could be made because the target machine actively refused it. No connection could be made because the target machine actively refused it."
						Debug.LogWarning((uint)ex.HResult + ": " + ex.Message);
						// TODO: there might be more cases where we need to Reset() (calls StartSignaling() recursively)
						Reset();
						break;
					default:
						Debug.LogError((uint)ex.HResult + ": " + ex.Message);
						//System.Diagnostics.Debugger.Break();
						break;
				}
			}
		}

		void StopSignaling()
		{
#if WITH_CANCELLATION
			_signaling.Close();
			UnregisterSignalingEventHandlers();
#endif

			_signaling?.Dispose();

			//stop/restart signaling component in the easiest/dumbest way possible
			_dnsSd?.Dispose();

		}

		private void Callbacks_OnConnectionStateChanged(ConnectionState state)
		{
			Debug.Log("Callbacks_OnConnectionStateChanged");
			if (state == ConnectionState.Connected)
			{
				Debug.Log("Isar.ImmersiveView connected!!");
				System.Diagnostics.Debug.WriteLine("+++> Isar.ImmersiveView connected!!");
				Task.Run(StopSignaling);
				Server.ViewPoseReceived += RemotingServer_ViewPoseReceived;
			}
			else if (state == ConnectionState.Disconnected)
			{
				Debug.Log("Isar.ImmersiveView disconnected!!");
				System.Diagnostics.Debug.WriteLine("+++> Isar.ImmersiveView disconnected!!");
				Server.ViewPoseReceived -= RemotingServer_ViewPoseReceived;

				_lastTimestamp = 0;
				//_scriptThreadQueue.Enqueue(() =>
				Task.Run(() =>
				{
					// NOTE: this needs to be on a different thread than Callbacks_OnConnectionStateChanged
					//This may not be called directly from the event handler, otherwise remoting lib will deadlock
					//due to the fact that callbacks usually come from the threads that are owned by WebRTC's PeerConnectionFactory.
					Server.Reset();

					StartSignaling();
				});
			}
		}

		//private void RemotingServer_ViewPoseReceived(StereoViewPose pose)
		private void RemotingServer_ViewPoseReceived(in StereoViewPose pose)
		{
			if (_lastTimestamp >= pose.timestamp)
			{
				Debug.Log("Received pose older than the current rendered - skip");
				return;
			}

			_lastTimestamp = pose.timestamp;
			if (Monitor.TryEnter(_poseLock, 2))
			{
				try
				{
					_currentPose = pose;
					_receivedAtLeastOnePose = true;
				}
				finally
				{
					Monitor.Exit(_poseLock);
				}
			}
			else
			{
				Debug.Log("Couldn't write new pose because it was locked...");
			}
		}

		private void Update()
		{
			// Execute pending work queued by background tasks
			while (_scriptThreadQueue.TryDequeue(out Action workload))
			{
				workload(); // TODO: try/catch
			}

			if (!Server.IsInitialized) return;

			Server.HandleEvents();

			if (_receivedAtLeastOnePose && Time.realtimeSinceStartup - _lastFrameTime > 0.016)
			{
				_lastFrameTime = Time.realtimeSinceStartup;
				StereoViewPose pose;
				lock (_poseLock)
				{
					pose = _currentPose;
				}
				RenderBothEyesWithCurrentPose(ShowLocalOutput, pose);
				Server.SendFrame(pose.timestamp, _remotingTexPtr);
			}
		}

		private void RenderBothEyesWithCurrentPose(bool restoreCamForLocalOutput, in StereoViewPose pose)
		{
			_camera.targetTexture = _remotingTex;
			// TODO(viktor): do these when receiving poses -> .Net vs Unity bindings layer
			var viewLeft = Convert.ToUnity(pose.viewLeft);
			var projLeft = Convert.ToUnity(pose.projLeft);
			var viewRight = Convert.ToUnity(pose.viewRight);
			var projRight = Convert.ToUnity(pose.projRight);

			// TODO(viktor): render both eyes in one render pass (need to wait for unity to support this?)
			RenderEye(StereoTargetEyeMask.Left, _leftEyeRect, viewLeft, projLeft);
			RenderEye(StereoTargetEyeMask.Right, _rightEyeRect, viewRight, projRight);

			if (restoreCamForLocalOutput)
			{
				_camera.targetTexture = null;
				_camera.rect = _fullScreenRect;
			}
		}

		private void RenderEye(StereoTargetEyeMask eye, in Rect camRect,
			in Matrix4x4 viewMatrix, in Matrix4x4 projectionMatrix)
		{
			_camera.stereoTargetEye = eye; // idk if this does anything
			_camera.rect = camRect;
			_camera.projectionMatrix = projectionMatrix;

			// view matrix reference: https://www.3dgep.com/understanding-the-view-matrix/#Memory_Layout_of_Column-Major_Matrices
			// Unity expects left handed coord system for the view matrix
			// convert from directx to opengl
			var worldToCameraMatrix = viewMatrix;
			Convert.FlipHandednessColumnMajor(ref worldToCameraMatrix);
			_camera.worldToCameraMatrix = worldToCameraMatrix;

			{
				// we still have to set the Transform data for any scripts that try to use it

				// convert opengl to directx (we can't use the original matrix because matrix math is like that...)
				Convert.FlipHandednessRowMajor(ref worldToCameraMatrix);

				// the view matrix stores the inverse of the position and rotation
				// using the inverse of the view matrix we get the transformation matrix for placing the camera object
				// but instead of doing an expensive inverse calculation we do this in-place though for better performance

				var x1 = worldToCameraMatrix.m00;
				var x2 = worldToCameraMatrix.m01;
				var x3 = worldToCameraMatrix.m02;

				var y1 = worldToCameraMatrix.m10;
				var y2 = worldToCameraMatrix.m11;
				var y3 = worldToCameraMatrix.m12;

				var z1 = worldToCameraMatrix.m20;
				var z2 = worldToCameraMatrix.m21;
				var z3 = worldToCameraMatrix.m22;

				var e1 = worldToCameraMatrix.m03;
				var e2 = worldToCameraMatrix.m13;
				var e3 = worldToCameraMatrix.m23;

				// undo the inverse for the position only
				// TODO: this should basically be a dot product, try unity's Dot() function (?)
				float xPos = -e1*x1 - e2*y1 - e3*z1;
				float yPos = -e1*x2 - e2*y2 - e3*z2;
				float zPos = -e1*x3 - e2*y3 - e3*z3;
				var position = new Vector3(xPos, yPos, zPos);

				{
					// debug draw axes
					var right   = new Vector3(x1, x2, x3);
					var up      = new Vector3(y1, y2, y3);
					var forward = new Vector3(z1, z2, z3);

					Debug.DrawLine(position, position + right, Color.red);
					Debug.DrawLine(position, position + up, Color.green);
					Debug.DrawLine(position, position + forward, Color.blue);
				}

				// since the right/up/forward axes are orthonormal we can simply do a transpose instead of an inverse here
				// TODO: don't do the transpose and instead do the quaternion calculation inline like with the position, using the correct values
				var orientation = MathUtils.QuaternionFromMatrix(worldToCameraMatrix.transpose);

				_camera.transform.position = position;
				_camera.transform.rotation = orientation;
			}

			_camera.Render();
		}

		#region Signaling

		private void RegisterSignalingEventHandlers()
		{
			Debug.Log("RegisterSignalingEventHandlers");
			Debug.Assert(_signaling != null);
			if (_signaling == null) return;
			_signaling.Connected += Signaling_OnConnected;
			_signaling.Disconnected += Signaling_OnDisconnected;
			_signaling.SdpReceived += Signaling_OnRemoteAnswerReceived;
			_signaling.IceCandidateReceived += Signaling_OnIceCandidateReceived;
		}

		// NOTE: Unused because Signaling.Dispose does it implicitly and Disconnected can be called during Dispose which means that it should be unregistered afterwards (as is done implicitly inside of Dispose)
		private void UnregisterSignalingEventHandlers()
		{
			Debug.Log("UnregisterSignalingEventHandlers");
			Debug.Assert(_signaling != null);
			if (_signaling == null) return;
			_signaling.IceCandidateReceived -= Signaling_OnIceCandidateReceived;
			_signaling.SdpReceived -= Signaling_OnRemoteAnswerReceived;
			_signaling.Disconnected -= Signaling_OnDisconnected; // can be called "during" Signaling.Close()
			_signaling.Connected -= Signaling_OnConnected;
		}

		private void RegisterRemotingEventHandlers()
		{
			Debug.Log("RegisterRemotingEventHandlers");
			Server.SdpCreated += RemotingServer_SdpOfferCreated;
			Server.IceCandidateCreated += RemotingServer_IceCandidateCreated;
			Callbacks.ConnectionStateChanged += Callbacks_OnConnectionStateChanged;
		}

		private void UnregisterRemotingEventHandlers()
		{
			Debug.Log("UnregisterRemotingEventHandlers");
			Callbacks.ConnectionStateChanged -= Callbacks_OnConnectionStateChanged;
			Server.IceCandidateCreated -= RemotingServer_IceCandidateCreated;
			Server.SdpCreated -= RemotingServer_SdpOfferCreated;
		}

		private async Task Signaling_OnConnected()
		{
			Debug.Log("===> Camera OnConnected");

			var currentApiVersion = Server.Version;
			await _signaling.SendVersionAsync(currentApiVersion.PackedValue);

			RegisterRemotingEventHandlers();
			Server.CreateOffer();
		}

		private void Signaling_OnDisconnected()
		{
			UnregisterRemotingEventHandlers();
			Debug.Log("===> Camera OnDisconnected");
		}

		private void Signaling_OnRemoteAnswerReceived(string type, string sdp)
		{
			if (type != Tokens.SDP_TYPE_ANSWER)
			{
				Debug.Log($"Received unexpected SDP message ({type})");
				return;
			}

			Server.SetRemoteAnswer(sdp);
		}

		private void Signaling_OnIceCandidateReceived(string mId, int mLineIndex, string candidate)
		{
			Server.AddIceCandidate(mId, mLineIndex, candidate);
		}

		/// <summary>
		/// Server has created Offer SDP message.
		/// </summary>
		/// <param name="sdp">Session Description Protocol Offer Message</param>
		// TODO(viktor): async send calls are prone to race conditions
		// option 1: dont make them async (blocks a werbtc thread(s)
		// option 2: use message queue
		private async void RemotingServer_SdpOfferCreated(SdpType type, string sdp)
		{
			Server.SdpCreated -= RemotingServer_SdpOfferCreated;
			await _signaling.SendSdpQueuedAsync(Tokens.SDP_TYPE_OFFER, sdp);
		}

		//TODO: change prefix to RemotingServer to be consistent with the majority of callbacks
		private async void RemotingServer_IceCandidateCreated(string mId, int mLineIndex, string candidate)
		{
			await _signaling.SendIceCandidateQueuedAsync(mId, mLineIndex, candidate);
		}

		#endregion Signaling

		private void OnDestroy()
		{
			StopSignaling();

			if (Server != null)
			{
				Server.ViewPoseReceived -= RemotingServer_ViewPoseReceived;
				Server.Dispose();
			}

			//_newPoseAvailableEvent.Dispose();
		}
	}
}
#endif
