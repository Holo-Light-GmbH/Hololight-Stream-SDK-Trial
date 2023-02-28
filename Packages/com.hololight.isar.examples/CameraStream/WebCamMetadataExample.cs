using UnityEngine;
using HoloLight.Isar;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using HoloLight.Isar.Native;
using Unity.Collections.LowLevel.Unsafe;

public class WebCamMetadataExample : MonoBehaviour
{
	private Texture2D _texture = null;
	private Renderer _renderer = null;
	private TextMesh _text = null;
	private Int32 _textureWidth = IsarClientCamera.DefaultConfiguration.width;
	private Int32 _textureHeight = IsarClientCamera.DefaultConfiguration.height;
	private TextureFormat _format = TextureFormat.RGBA32;
	private IntPtr _cpuImagePtr = IntPtr.Zero;
	private readonly Dictionary<Int64, IsarClientCamera.CameraMetadata> _metadataDictionary = new Dictionary<Int64, IsarClientCamera.CameraMetadata>();
	private readonly List<Int64> _keyQueue = new List<Int64>();
	private long _lastCaptureTimeNHS = 0;

	private IsarClientCamera _isar;

	private const float ScaleFactor = 0.0001f;
	private const float MeshScaleDefault = 1.0f;
	public float meshScale = MeshScaleDefault;

	public bool enableCamera;

	public IsarClientCamera.CameraConfiguration config = IsarClientCamera.DefaultConfiguration;
	public IsarClientCamera.CameraSettings settings = IsarClientCamera.DefaultSettings;

	public bool dirty = false;
	public bool IsConnected => _isar != null && _isar.IsConnected;

	public void Reconfigure()
	{
		_isar.Reconfigure(enableCamera, config, settings);
		if (enableCamera == false)
		{
			_metadataDictionary.Clear();
			_keyQueue.Clear();
		}
	}

	public void Reset()
	{
		meshScale = MeshScaleDefault;
		ApplyScale();
		enableCamera = false;
		settings = IsarClientCamera.DefaultSettings;
		config = IsarClientCamera.DefaultConfiguration;
	}

	private void OnEnable()
	{

		try
		{
			_lastCaptureTimeNHS = 0;
			_isar = new IsarClientCamera();
			_isar.ConnectionStateChanged += OnConnectionStateChanged;
			_isar.MetadataReceived += OnMetadataReceived;
			Reconfigure();
		}
		catch (Exception)
		{
			enabled = false;
		}
	}

	private void Start()
	{
		_renderer = GetComponent<Renderer>();
		_text = GetComponentInChildren<TextMesh>();
		if (!SetupTexture(IsarClientCamera.DEFAULT_FORMAT, config.width, config.height))
		{
			return;
		}
	}

	private void OnDisable()
	{
		_metadataDictionary.Clear();
		_keyQueue.Clear();
		_isar?.Dispose();
	}

	private void OnDestroy()
	{
		Marshal.FreeHGlobal(_cpuImagePtr);
	}

	private void OnConnectionStateChanged(HlrConnectionState state)
	{
		if (state != HlrConnectionState.Connected)
		{
			_metadataDictionary.Clear();
			_keyQueue.Clear();
		}
	}

	private void OnMetadataReceived(Int64 captureTimeHNS, in IsarClientCamera.CameraMetadata metadata)
	{
		// There is a delay between the server & client toggle, so we need to ignore metadata from the stream which is about to be disabled
		//if (enableCamera)
		//{
			_metadataDictionary[captureTimeHNS] = metadata;
		//}
	}

	public void ApplyScale()
	{
		var scale = transform.localScale;
		scale.x = meshScale * ScaleFactor * _textureWidth;
		scale.z = meshScale * ScaleFactor * _textureHeight;
		transform.localScale = scale;
	}

	private bool SetupTexture(TextureFormat format, int width, int height)
	{
		if(format != TextureFormat.RGBA32)
		{
			Debug.LogError($"Format {format} is not supported!");
			return false;
		}

		_texture = new Texture2D(width, height, format, false);
		_renderer.material.mainTexture = _texture;

		_format = format;
		_textureWidth = width;
		_textureHeight = height;

		ApplyScale();

		return true;
	}

	private void LateUpdate()
	{
		TextureFormat format = TextureFormat.RGBA32;
		Int32 width = 0, height = 0;
		long captureTimeHNS = 0;

		if (!_isar.TryAcquireLatestCameraImage(ref format, ref width, ref height, ref captureTimeHNS))
		{
			return;
		}

		if(_lastCaptureTimeNHS == captureTimeHNS) return;

		_lastCaptureTimeNHS = captureTimeHNS;

		if (width != _textureWidth || height != _textureHeight || format != _format)
		{
			if (!SetupTexture(format, width, height))
			{
				return;
			}
		}

		unsafe
		{
			var rawTextureData = _texture.GetRawTextureData<byte>();

			if (!_isar.TryAcquireCameraImageBuffer(new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length))
				return;

			IsarClientCamera.CameraMetadata metadata;
			if (_metadataDictionary.TryGetValue(captureTimeHNS, out metadata))
			{
				_text.text =
					$"Intrinsics:\n" +
					$"  Frame size: {{ w: {metadata.intrinsics.width}, h: {metadata.intrinsics.height} }}\n" +
					$"  Focal length: {{ x: {metadata.intrinsics.focalLengthX}, y: {metadata.intrinsics.focalLengthY} }}\n" +
					$"  Camera model: principal point: {{ x: {metadata.intrinsics.cameraModelPrincipalPointX}, y: {metadata.intrinsics.cameraModelPrincipalPointY} }}\n" +
					$"  Distortion model:\n" +
					$"    radial: k1: {metadata.intrinsics.distortionModelRadialK1} k2: {metadata.intrinsics.distortionModelRadialK2} k3: {metadata.intrinsics.distortionModelRadialK3}\n" +
					$"    tangential: p1: {metadata.intrinsics.distortionModelTangentialP1} p2: {metadata.intrinsics.distortionModelTangentialP2}\n" +
					$"Extrinsics:\n" +
					$"  {metadata.extrinsics.M00:F6}\t{metadata.extrinsics.M10:F6}\t{metadata.extrinsics.M20:F6}\t{metadata.extrinsics.M30:F6}\n" +
					$"  {metadata.extrinsics.M01:F6}\t{metadata.extrinsics.M11:F6}\t{metadata.extrinsics.M21:F6}\t{metadata.extrinsics.M31:F6}\n" +
					$"  {metadata.extrinsics.M02:F6}\t{metadata.extrinsics.M12:F6}\t{metadata.extrinsics.M22:F6}\t{metadata.extrinsics.M32:F6}\n" +
					$"  {metadata.extrinsics.M03:F6}\t{metadata.extrinsics.M13:F6}\t{metadata.extrinsics.M23:F6}\t{metadata.extrinsics.M33:F6}\n" +
					$"Settings:\n" +
					$"  auto exposure: {metadata.settings.autoExposure}\n" +
					$"  exposure: {metadata.settings.exposure}\n" +
					$"  exposure compensation: {metadata.settings.exposureCompensation}\n" +
					$"  white balance: {metadata.settings.whiteBalance}";
				_text.color = Color.white;

				_metadataDictionary.Remove(captureTimeHNS);
			}
			else
			{
				_text.color = Color.gray;
			}

			_texture.Apply();

		}
	}

}