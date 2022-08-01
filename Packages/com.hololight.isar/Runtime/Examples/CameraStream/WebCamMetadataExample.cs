// Starts the default camera and assigns the texture to the current renderer
using UnityEngine;
using HoloLight.Isar;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using HoloLight.Isar.Native;

public class WebCamMetadataExample : MonoBehaviour, ISerializationCallbackReceiver
{
	private Texture2D _texture = null;
	private Renderer _renderer = null;
	private TextMesh _text = null;
	IntPtr unmanagedPtr = IntPtr.Zero;
	Dictionary<Int64, IsarClientCamera.CameraMetadata> metadataDictionary = new Dictionary<Int64, IsarClientCamera.CameraMetadata>();
	List<Int64> keyQueue = new List<Int64>();

	private IsarClientCamera _isar;

	[SerializeField]
	private bool toggle;
	private bool togglePreDeserialized;

	public bool Toggle
	{
		get => toggle;
		set
		{
			toggle = togglePreDeserialized = value;
			if (_isar != null) _isar.Toggle = value;
			if (value == false)
			{
				metadataDictionary.Clear();
				keyQueue.Clear();
			}
		}
	}

	public void OnAfterDeserialize()
	{
		if (toggle == togglePreDeserialized) return;
		Toggle = toggle;
	}

	public void OnBeforeSerialize()
	{
	}

	private void OnEnable()
	{
		_isar = new IsarClientCamera();
		_isar.ConnectionStateChanged += OnConnectionStateChanged;
		_isar.MetadataReceived += OnMetadataReceived;
		Toggle = toggle;
	}

	private void Start()
	{
		_texture = new Texture2D(IsarClientCamera.WIDTH, IsarClientCamera.HEIGHT, IsarClientCamera.FORMAT, false);
		_renderer = GetComponent<Renderer>();
		_renderer.material.mainTexture = _texture;
		_text = GetComponentInChildren<TextMesh>();
		unmanagedPtr = Marshal.AllocHGlobal(IsarClientCamera.SIZE);
	}

	private void OnDisable()
	{
		metadataDictionary.Clear();
		keyQueue.Clear();
		_isar.Dispose();
	}

	private void OnDestroy()
	{
		Marshal.FreeHGlobal(unmanagedPtr);
	}

	private void OnConnectionStateChanged(HlrConnectionState state)
	{
		if (state != HlrConnectionState.Connected)
		{
			metadataDictionary.Clear();
			keyQueue.Clear();
		}
	}

	private void OnMetadataReceived(Int64 captureTimeHNS, in IsarClientCamera.CameraMetadata metadata)
	{
		metadataDictionary.Add(captureTimeHNS, metadata);
	}

	private void LateUpdate()
	{
		long captureTimeHNS = 0;
		if (!_isar.TryDequeueFrame(ref captureTimeHNS, unmanagedPtr))
			return;

		IsarClientCamera.CameraMetadata metadata;
		if (metadataDictionary.TryGetValue(captureTimeHNS, out metadata))
		{
			_text.text =
				$"Intrinsics:\n" +
				$"  Frame size: {{ w: {metadata.intrinsics.width}, h: {metadata.intrinsics.height} }}\n" +
				$"  Focal length: {{ x: {metadata.intrinsics.focal_length_x}, y: {metadata.intrinsics.focal_length_y} }}\n" +
				$"  Camera model: principal point: {{ x: {metadata.intrinsics.camera_model_principal_point_x}, y: {metadata.intrinsics.camera_model_principal_point_y} }}\n" +
				$"  Distortion model:\n" +
				$"    radial: k1: {metadata.intrinsics.distortion_model_radial_k1} k2: {metadata.intrinsics.distortion_model_radial_k2} k3: {metadata.intrinsics.distortion_model_radial_k3}\n" +
				$"    tangential: p1: {metadata.intrinsics.distortion_model_tangential_p1} p2: {metadata.intrinsics.distortion_model_tangential_p2}";
			_text.color = Color.white;
		}
		else
		{
			_text.color = Color.gray;
		}

		_texture.LoadRawTextureData(unmanagedPtr, IsarClientCamera.SIZE);
		_texture.Apply();

	}

}