using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using HoloLight.Isar;
using HoloLight.Isar.Native;
using HoloLight.Isar.Native.Qr;
using UnityEngine;

public class QrCodeExample : MonoBehaviour
{
	IsarQr _isar;
	bool isWatching;

	private enum ConnectedDeviceType { HMD, Handheld };
	private ConnectedDeviceType _connectedDeviceType;

	private List<GameObject> _qrVisualizers;
	private bool _destroyVisualizers = false;

	void OnConnectionStateChanged(HlrConnectionState state)
	{
		if (state == HlrConnectionState.Disconnected || state == HlrConnectionState.Failed)
		{
			isWatching = false;
			_destroyVisualizers = true;
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

		if(_destroyVisualizers)
		{
			DestroyVisualizers();
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
					var config = _isar.loader.RemotingConfig;
					if (config.renderConfig.numViews == 2)
					{
						_connectedDeviceType = ConnectedDeviceType.HMD;
					}
					else
					{
						_connectedDeviceType = ConnectedDeviceType.Handheld;
					}
					if(_qrVisualizers == null)
					{
						_qrVisualizers = new List<GameObject>();
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
				Debug.LogWarning($"Unable to call StartWatching. Isar is not connected.");
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
				Debug.LogWarning($"Unable to call StopWatching. Isar is not connected.");
			}
		}
		isWatching = false;
		_destroyVisualizers = true;
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
			Debug.LogWarning($"Unable to call IsSupported. Isar is not connected.");
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
			Debug.LogWarning($"Unable to call RequestAccess. Isar is not connected.");
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
		_qrVisualizers.Add(plane);
	}

	private void QrApi_OnUpdated(in QrUpdatedEventArgs args)
	{
		var id = args.Code.Id;
		Debug.Log($"QrCode Id: {id}");
		Debug.Log(GetQRCodeData(args.Code));
		UpdatePose(qrCodes[id].transform, args.Code.Pose, args.Code.PhysicalSideLength / 2);
	}

	private void QrApi_OnRemoved(in QrRemovedEventArgs args)
	{
		var id = args.Code.Id;
		Debug.Log($"QrCode Id: {id}");

		var plane = qrCodes[id];
		_qrVisualizers.Remove(plane);
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

	private string GetQRCodeData(QrCode code)
	{
		var encoder = Encoding.UTF8;
		string retString;

		// Create a managed byte data array from the uint pointer and length. We first need an IntPtr because marshal methods require this.
		var byteData = new byte[(int)code.DataSize];
		var intPtr = unchecked((IntPtr)(long)(ulong)code.Data);
		Marshal.Copy(intPtr, byteData, 0, byteData.Length);

		// Format provider are 4 bit. We need to shift our array 4 bit to the right to read clear byte data again.
		for (var i = 0; i < 4; i++)
		{
			ShiftRight(ref byteData);
		}

		// Versions 1-9 have 8 bit (1 byte) as length indicator, with the shift we cut one additional byte from the format provider
		if ((int)code.Version < 10)
		{
			retString = encoder.GetString(byteData, 2, byteData[1]);
		}

		// Versions 10+ have 16 bit (2 byte) as length indicator, with the shift we cut one additional byte from the format provider
		else
		{
			byte[] length = { byteData[1], byteData[2] };
			retString = encoder.GetString(byteData, 3, BitConverter.ToInt16(length, 0));
		}
		return retString;
	}

	private bool ShiftRight(ref byte[] bytes)
	{
		var rightMostCarryFlag = false;
		var rightEnd = bytes.Length - 1;

		// Iterate through the elements of the array right to left.
		for (var index = rightEnd; index >= 0; index--)
		{
			// If the rightmost bit of the current byte is 1 then we have a carry.
			var carryFlag = (bytes[index] & 0x01) > 0;

			if (index < rightEnd)
			{
				if (carryFlag)
				{
					// Apply the carry to the leftmost bit of the current bytes neighbor to the right.
					bytes[index + 1] = (byte)(bytes[index + 1] | 0x80);
				}
			}
			else
			{
				rightMostCarryFlag = carryFlag;
			}

			bytes[index] = (byte)(bytes[index] >> 1);
		}

		return rightMostCarryFlag;
	}

	private void DestroyVisualizers()
	{
		foreach (var qrCode in _qrVisualizers)
		{
			Destroy(qrCode);
		}
		_qrVisualizers.Clear();
		_destroyVisualizers = false;
	}
}
