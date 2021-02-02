using System;
using System.Collections;
using System.Collections.Generic;
using HoloLight.Isar.Native;
using HoloLight.Isar.Native.Qr;
using UnityEngine;

// KL: Use a base class for wrapping C APIs from the library which handles stuff like Init/Close so you don't have to
// and exposes connection state things because calling things without a valid connection isn't that good either.
// But it'd be nice if it worked for scriptableobjects also, but I think MonoBehaviour is more important rn.
// MonoBehaviour inheritance is kinda ugly tho, I think. We'll see.
// Also, this could use refcounting in the name so the purpose is more clear? Not sure either.
// VW: Basically, inherit from IsarEventListener.
// Don't know about scriptable objects though.
// that connection init is internally refcounted is of no concern to users and it's implementation-specific knowledge which serves no purpose here, especially if users will simply inherit a base class which already does the initialization.
public /*abstract*/ class QrSupport : MonoBehaviour
{
	ConnectionHandle _handle = new ConnectionHandle();
	ServerApi _serverApi;
	ConnectionState _connectionState;
	object lockObj = new object();
	bool isWatching;

	void OnConnectionStateChanged(ConnectionState state)
	{
		lock (lockObj)
		{
			_connectionState = state;
		}

		if (state == ConnectionState.Disconnected)
		{
			isWatching = false;
		}
	}

	private bool IsConnected
	{
		get
		{
			ConnectionState state;
			lock (lockObj)
			{
				state = _connectionState;
			}
			return state == ConnectionState.Connected;
		}
	}

	private Dictionary<Guid, GameObject> qrCodes = new Dictionary<Guid, GameObject>();
	private UnityEngine.Vector3 positionU = UnityEngine.Vector3.zero;
	private UnityEngine.Quaternion orientationU = UnityEngine.Quaternion.identity;

	void UpdatePose(Transform _transform, in HoloLight.Isar.Native.Pose pose, float halfLength)
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
		}
	}

	protected virtual void Start()
    {
		//Init remoting library (or rather get its struct, because XR display already initialized it when we called CreateSubsystem) 
		_serverApi = new ServerApi();
		Error err = ServerApi.Create(ref _serverApi);

		if (err != Error.eNone)
		{
			throw new Exception("Failed to create API struct");
		}

		GraphicsApiConfig gfx = new GraphicsApiConfig();
		ConnectionCallbacks cb = new ConnectionCallbacks();
		err = _serverApi.ConnectionApi.Init(null, gfx, cb, ref _handle);

		if (err != Error.eNone)
		{
			throw new Exception("Failed to init remoting lib");
		}

		//Kinda awful that we have to do this because it's easy to forget and other safehandle types don't do this.
		//Maybe something for the "nice" C# layer if we want one. Right now it's raw bindings in here.
		_handle.ConnectionApi = _serverApi.ConnectionApi;

		Callbacks.ConnectionStateChanged += OnConnectionStateChanged;

		QrApi.Init(_serverApi.ConnectionApi, _handle);
		QrApi.EnumerationCompleted += QrApi_OnEnumerationCompleted;
		QrApi.IsSupportedReceived += QrApi_OnIsSupportedReceived;
		QrApi.AccessStatusReceived += QrApi_OnAccessStatusReceived;
		QrApi.Added += QrApi_OnAdded;
		QrApi.Updated += QrApi_OnUpdated;
		QrApi.Removed += QrApi_OnRemoved;

		MessageCallbacks msgCallbacks = new MessageCallbacks(null, null, QrApi.OnIsSupported, QrApi.OnRequestAccess, QrApi.OnAdded, QrApi.OnUpdated, QrApi.OnRemoved, QrApi.OnEnumerationCompleted);
		_serverApi.ConnectionApi.RegisterMessageCallbacks(_handle, ref msgCallbacks);
	}

	protected virtual void OnDestroy()
	{
		_handle.Dispose();
	}

	private void Update()
	{
		if (IsConnected)
		{
			QrApi.ProcessMessages();
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
			if (IsConnected)
			{
				Error error = QrApi.Start();
				if (error == Error.eNone)
				{
					isWatching = true;
				}
				else
					Debug.LogWarning($"QrApi call returned an error: {error}");
			}
			else
			{
				Debug.LogWarning($"cant watch! (isConnected: {IsConnected})");
			}
		}
	}

	[ContextMenu("Stop")]
	public void StopWatching()
	{
		if (isWatching)
		{
			if (IsConnected)
			{
				//_server.QrStop();
				var error = QrApi.Stop();
				if (error != Error.eNone) Debug.LogWarning($"QrApi call returned an error: {error}");
			}
			else
			{
				Debug.LogWarning($"cant stop watching! (isConnected: {IsConnected})");
			}
		}
		isWatching = false;
	}

	[ContextMenu("IsSupported")]
	public void IsSupported()
	{
		if (IsConnected)
		{
			var error = QrApi.IsSupported();
			if (error != Error.eNone) Debug.LogWarning($"QrApi call returned an error: {error}");
		}
		else
		{
			Debug.LogWarning($"cant stop watching! (isConnected: {IsConnected})");
		}
	}

	[ContextMenu("RequestAccess")]
	public void RequestAccess()
	{
		if (IsConnected)
		{
			//var error = QrApi.RequestAccess(connection);
			var error = QrApi.RequestAccess();
			if (error != Error.eNone) Debug.LogWarning($"QrApi call returned an error: {error}");
		}
		else
		{
			Debug.LogWarning($"cant stop watching! (isConnected: {IsConnected})");
		}
	}

	private void QrApi_OnAdded(in QrAddedEventArgs args)
	{
		var id = args.Code.Id;
		Debug.Log($"QrCode Id: {id}");

		var plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
		UpdatePose(plane.transform, args.Code.Pose, args.Code.PhysicalSideLength / 2);

		plane.transform.localScale = new UnityEngine.Vector3(args.Code.PhysicalSideLength / 10.0f, args.Code.PhysicalSideLength / 10.0f, 0.1f);
		var newGlobalScale = new UnityEngine.Vector3(args.Code.PhysicalSideLength, args.Code.PhysicalSideLength, 0.001f);
		plane.transform.localScale = UnityEngine.Vector3.one;
		plane.transform.localScale = new UnityEngine.Vector3(
			newGlobalScale.x / plane.transform.lossyScale.x,
			newGlobalScale.y / plane.transform.lossyScale.y,
			newGlobalScale.z / plane.transform.lossyScale.z);
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
