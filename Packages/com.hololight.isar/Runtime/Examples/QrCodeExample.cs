using System;
using System.Collections.Generic;
using System.IO;
using HoloLight.Isar;
using HoloLight.Isar.Native;
using HoloLight.Isar.Native.Qr;
using UnityEngine;

public class QrCodeExample : MonoBehaviour
{
	IsarQr _isar;
	bool isWatching;

	private readonly string REMOTING_CONFIG_FILE_NAME = "remoting-config.cfg";
	private string _remotingConfigPath;
	private enum ConnectedDeviceType { HMD, Handheld};
	private ConnectedDeviceType _connectedDeviceType;

	void OnConnectionStateChanged(HlrConnectionState state)
	{
		if (state == HlrConnectionState.Disconnected || state == HlrConnectionState.Failed)
		{
			isWatching = false;
		}
		else
		{
			StartWatching();
		}
	}

	private Dictionary<Guid, GameObject> qrCodes = new Dictionary<Guid, GameObject>();
	private UnityEngine.Vector3 positionU = UnityEngine.Vector3.zero;
	private UnityEngine.Quaternion orientationU = UnityEngine.Quaternion.identity;

	void UpdatePose(Transform _transform, in HoloLight.Isar.Native.HlrPose pose, float halfLength)
	{
		// We get different updates right now from Handheld devices (Android/iOS) and from HMDs like Hololens. This section maps them to the same Unity coordinate space.
		if (_connectedDeviceType == ConnectedDeviceType.HMD)
		{
			if (!float.IsNaN(pose.Position.X))
			{
				var position = pose.Position;
				positionU = HoloLight.Isar.Utils.Convert.ToUnity(position);

				var offset = new UnityEngine.Vector3(halfLength, halfLength, 0.0f);
				// TODO: do Conversion before passing values to user
				positionU.z *= -1;

				var orientation = pose.Orientation;
				orientationU = HoloLight.Isar.Utils.Convert.ToUnity(orientation);
				// TODO: do Conversion before passing values to user
				orientationU.x *= -1;
				orientationU.y *= -1;

				_transform.SetPositionAndRotation(positionU, orientationU);
				_transform.Translate(offset, Space.Self);
				_transform.Rotate(Vector3.right, 90);
			}
		}
		else
		{
			// Handheld update
			if (!float.IsNaN(pose.Position.X))
			{
				var position = pose.Position;
				positionU = HoloLight.Isar.Utils.Convert.ToUnity(position);
				positionU.z *= -1;

				var orientation = pose.Orientation;
				orientationU = HoloLight.Isar.Utils.Convert.ToUnity(orientation);
				orientationU.z *= -1;
				orientationU.w *= -1; 

				_transform.SetPositionAndRotation(positionU, orientationU);
			}
		}
	}

	private void OnEnable()
	{
		_isar = new IsarQr();
		_isar.ConnectionStateChanged += OnConnectionStateChanged;
		_isar.QrCodeAdded += QrApi_OnAdded;
		_isar.QrCodeUpdated += QrApi_OnUpdated;
		_isar.QrCodeRemoved += QrApi_OnRemoved;
		_isar.QrCodeEnumerationCompleted += QrApi_OnEnumerationCompleted;
		_isar.QrCodeAccessStatusReceived += QrApi_OnAccessStatusReceived;
		_isar.QrCodeIsSupportedReceived += QrApi_OnIsSupportedReceived;
	}

	private void Start()
	{
		if (_isar != null) return;

		_isar = new IsarQr();
		_isar.ConnectionStateChanged += OnConnectionStateChanged;
		_isar.QrCodeAdded += QrApi_OnAdded;
		_isar.QrCodeUpdated += QrApi_OnUpdated;
		_isar.QrCodeRemoved += QrApi_OnRemoved;
		_isar.QrCodeEnumerationCompleted += QrApi_OnEnumerationCompleted;
		_isar.QrCodeAccessStatusReceived += QrApi_OnAccessStatusReceived;
		_isar.QrCodeIsSupportedReceived += QrApi_OnIsSupportedReceived;
	}

	private void OnDisable()
	{
		_isar.ConnectionStateChanged -= OnConnectionStateChanged;
		_isar.QrCodeAdded -= QrApi_OnAdded;
		_isar.QrCodeUpdated -= QrApi_OnUpdated;
		_isar.QrCodeRemoved -= QrApi_OnRemoved;
		_isar.QrCodeEnumerationCompleted -= QrApi_OnEnumerationCompleted;
		_isar.QrCodeAccessStatusReceived -= QrApi_OnAccessStatusReceived;
		_isar.QrCodeIsSupportedReceived -= QrApi_OnIsSupportedReceived;
		_isar.Dispose();
		_isar = null;
	}

	private void OnDestroy()
	{
		if (_isar == null) return;
		_isar.ConnectionStateChanged -= OnConnectionStateChanged;
		_isar.QrCodeAdded -= QrApi_OnAdded;
		_isar.QrCodeUpdated -= QrApi_OnUpdated;
		_isar.QrCodeRemoved -= QrApi_OnRemoved;
		_isar.QrCodeEnumerationCompleted -= QrApi_OnEnumerationCompleted;
		_isar.QrCodeAccessStatusReceived -= QrApi_OnAccessStatusReceived;
		_isar.QrCodeIsSupportedReceived -= QrApi_OnIsSupportedReceived;
		_isar.Dispose();
		_isar = null;
	}

	private void Update()
	{
		if (_isar.IsConnected)
		{
			_isar.ProcessMessages(); // TODO: this should only be called in one location of the entire app; use SynchronisationContext
		}
	}

	public void ToggleWatching()
	{
		if (isWatching)
		{
			StopWatching();
		}
		else
		{
			StartWatching();
		}
	}

	[ContextMenu("Start")]
	public void StartWatching()
	{
		if (!isWatching)
		{
			if (_isar.IsConnected)
			{
				try
				{
					_isar.Start();
					_remotingConfigPath = Path.Combine(Application.streamingAssetsPath, REMOTING_CONFIG_FILE_NAME);
					IsarRemotingConfig config = IsarRemotingConfig.CreateFromJSON(File.ReadAllText(_remotingConfigPath));
					if (config.renderConfig.numViews == 2)
					{
						_connectedDeviceType = ConnectedDeviceType.HMD;
					}
					else
					{
						_connectedDeviceType = ConnectedDeviceType.Handheld;
					}
					isWatching = true;
				}
				catch (Exception)
				{
					throw;
				}
			}
			else
			{
				Debug.LogWarning($"cant watch! (isConnected: {_isar.IsConnected})");
			}
		}
	}

	[ContextMenu("Stop")]
	public void StopWatching()
	{
		if (isWatching)
		{
			if (_isar.IsConnected)
			{
				try
				{
					_isar.Stop();
				}
				catch (Exception)
				{
					throw;
				}
			}
			else
			{
				Debug.LogWarning($"cant stop watching! (isConnected: {_isar.IsConnected})");
			}
		}
		isWatching = false;
	}

	[ContextMenu("IsSupported")]
	public void IsSupported()
	{
		if (_isar.IsConnected)
		{
			try
			{
				_isar.IsSupported();
			}
			catch (Exception)
			{
				throw;
			}
		}
		else
		{
			Debug.LogWarning($"cant stop watching! (isConnected: {_isar.IsConnected})");
		}
	}

	[ContextMenu("RequestAccess")]
	public void RequestAccess()
	{
		if (_isar.IsConnected)
		{
			try
			{
				_isar.RequestAccess();
			}
			catch (Exception)
			{
				throw;
			}
		}
		else
		{
			Debug.LogWarning($"cant stop watching! (isConnected: {_isar.IsConnected})");
		}
	}

	private void QrApi_OnAdded(in QrAddedEventArgs args)
	{
		var id = args.Code.Id;
		Debug.Log($"QrCode Id: {id}");

			var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
			UpdatePose(plane.transform, args.Code.Pose, args.Code.PhysicalSideLength / 2);
			plane.transform.localScale = new UnityEngine.Vector3(args.Code.PhysicalSideLength / 10.0f, 1, args.Code.PhysicalSideLength / 10.0f);
			qrCodes[id] = plane;	
	}

	private void QrApi_OnUpdated(in QrUpdatedEventArgs args)
	{
		var id = args.Code.Id;
		Debug.Log($"QrCode Id: {id}");
		UpdatePose(qrCodes[id].transform, args.Code.Pose, args.Code.PhysicalSideLength / 2);
	}

	private void QrApi_OnRemoved(in QrRemovedEventArgs args)
	{
		var id = args.Code.Id;
		Debug.Log($"QrCode Id: {id}");

		var plane = qrCodes[id];
		qrCodes.Remove(id);
		DestroyImmediate(plane);
	}

	private void QrApi_OnAccessStatusReceived(in QrRequestAccessEventArgs args)
	{
		Debug.Log($"QrCodeTest QrApi_OnAccessStatusReceived: {args.Status}");
	}

	private void QrApi_OnIsSupportedReceived(in QrIsSupportedEventArgs args)
	{
		Debug.Log($"QrCodeTest QrApi_OnIsSupportedReceived: {args.IsSupported}");
	}

	private void QrApi_OnEnumerationCompleted()
	{
		Debug.Log("QrCodeTest QrApi_OnEnumerationCompleted");
		isWatching = false;
	}
}
