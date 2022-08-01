using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using HoloLight.Isar.Native;
using HoloLight.Isar.Native.CustomMessage;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace HoloLight.Isar.ARSubsystems
{
	public class IsarTouchscreen : Touchscreen, IInputUpdateCallbackReceiver
	{
		private int _height, _width;
		private IsarCustomSend _customSend;
		private object _lockObj = new object();
		private Queue<TouchState> _touchStateQueue = new Queue<TouchState>();
		private bool _flagMainCameraNull = true;
		private Camera _mainCamera = null;
		private float _horizontalFactor, _verticalFactor;

		private const int DATA_LENGTH = 5 * 4;

		protected override void OnAdded()
		{
			// Create a custom send to receive messages
			_customSend = new IsarCustomSend();
			_customSend.ConnectionStateChanged += CustomSend_ConnectionStateChanged;

			// Get the touchscreen resolution from the config
			GetConfiguredResolution(Application.streamingAssetsPath + "/remoting-config.cfg", out _width, out _height);

			base.OnAdded();
		}

		protected override void OnRemoved()
		{
			if (_customSend != null) _customSend.Dispose();

			base.OnRemoved();
		}

		/// <summary>
		/// A connection state change has been notified. Enable/Disable the device.
		/// </summary>
		/// <param name="connectionState">Changed state</param>
		private void CustomSend_ConnectionStateChanged(HlrConnectionState connectionState)
		{
			if(connectionState == HlrConnectionState.Connected)
			{
				_customSend.CustomMessageReceived += CustomSend_CustomMessageReceived;
			}
			else
			{
				_customSend.CustomMessageReceived -= CustomSend_CustomMessageReceived;
			}
		}

		/// <summary>
		/// Unpack the config file and get the resolution
		/// </summary>
		/// <param name="configPath">Path to config file</param>
		/// <param name="width">Out parameter with the width in the config</param>
		/// <param name="height">Out parameter with the height in the config</param>
		private void GetConfiguredResolution(string configPath, out int width, out int height)
		{
			StreamReader reader = File.OpenText(configPath);
			IsarRemotingConfig config = IsarRemotingConfig.CreateFromJSON(reader.ReadToEnd());

			width = config.renderConfig.width;
			height = config.renderConfig.height;
		}

		/// <summary>
		/// Custom message received, try to unpack and queue data if successful
		/// </summary>
		/// <param name="message"></param>
		private void CustomSend_CustomMessageReceived(in HlrCustomMessage message)
		{
			lock (_lockObj)
			{
				if (GetTouchState(message, out TouchState state)) _touchStateQueue.Enqueue(state);
			}
		}

		/// <summary>
		/// Pack the current touch state based on the message received
		/// </summary>
		/// <param name="message">Custom message received from client</param>
		/// <param name="touchState">Touch state populated with data</param>
		/// <returns>True if unpacking was successful, false if not</returns>
		private bool GetTouchState(HlrCustomMessage message, out TouchState touchState)
		{

			IntPtr readPtr = message.Data;
			HoloLight.Isar.Shared.Assertions.Assert(message.Length >= Marshal.SizeOf<Int32>());
			MessageType messageType = (MessageType)Marshal.ReadInt32(readPtr);
			if (messageType != MessageType.TOUCH_DATA || message.Length < DATA_LENGTH)
			{
				touchState = new TouchState();
				return false;
			}
			readPtr = IntPtr.Add(readPtr, Marshal.SizeOf<Int32>());

			// Creating Touch Data out of parsed Values
			byte[] data = new byte[DATA_LENGTH];
			Marshal.Copy(readPtr, data, 0, DATA_LENGTH);
			int index = 0;


			// Creating Touch Data out of parsed Values
			float type = BitConverter.ToSingle(data, index);
			index += 4;
			float x = BitConverter.ToSingle(data, index);
			index += 4;
			float y = BitConverter.ToSingle(data, index);
			index += 4;
			_horizontalFactor = BitConverter.ToSingle(data, index);
			index += 4;
			_verticalFactor = BitConverter.ToSingle(data, index);

			// Set the touch position in pixels based off the current width and height of the render texture
			Vector2 touchPosition = new Vector2(x * _width, y * _height);

			UnityEngine.InputSystem.TouchPhase touchPhase = type == 0 ? UnityEngine.InputSystem.TouchPhase.Began :
									type == 2 ? UnityEngine.InputSystem.TouchPhase.Moved : UnityEngine.InputSystem.TouchPhase.Ended;
			touchState = new TouchState()
			{
				phase = touchPhase,
				touchId = 1,
				position = touchPosition,
				isIndirectTouch = true
			};

			return true;
		}

		public void OnUpdate()
		{
			if (_mainCamera == null)
			{
				if(Camera.main == null && _flagMainCameraNull)
				{
					Debug.LogWarning("Please tag the main camera with the 'MainCamera' tag, otherwise the touchscreen device may not operate correctly.");
					_flagMainCameraNull = false;
				}
				_mainCamera = Camera.main;
			}

			lock (_lockObj)
			{
				// Apply the horizontal and vertical factor to the projection matrix if we have a camera 
				// and there is new touch states
				if (_touchStateQueue.Count > 0 && _mainCamera != null)
				{
					var proj = _mainCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
					proj.m00 *= _horizontalFactor;
					proj.m11 *= _verticalFactor;
					_mainCamera.projectionMatrix = proj;
				}

				while(_touchStateQueue.Count > 0)
				{ 
					var state = _touchStateQueue.Dequeue();
					InputSystem.QueueStateEvent(this, state, InputState.currentTime);
				}
			}
		}
	}
}
