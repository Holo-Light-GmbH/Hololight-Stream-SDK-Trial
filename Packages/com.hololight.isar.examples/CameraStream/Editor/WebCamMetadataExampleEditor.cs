using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

namespace HoloLight.Isar
{
	[CustomEditor(typeof(WebCamMetadataExample))]
	public class WebCamMetadataExampleEditor : Editor
	{
		private WebCamMetadataExample _script;
		private bool _dirty = false;

		public static string CameraConfigurationToString(IsarClientCamera.CameraConfiguration config)
		{
			return $"{config.width}x{config.height}@{config.framerate}fps";
		}

		private void OnEnable()
		{
			_script = target as WebCamMetadataExample;
		}

		void OnCameraConfigurationSelected(object cameraConfiguration)
		{
			var config = (IsarClientCamera.CameraConfiguration)cameraConfiguration;
			if (_script.config.Equals(config)) return;

			_script.config = config;
			_dirty = true;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
				_script.meshScale = EditorGUILayout.Slider("Mesh scale", _script.meshScale, 0, 5);
			var applyScale = EditorGUI.EndChangeCheck();

			if (applyScale == true)
				_script.ApplyScale();

			EditorGUI.BeginChangeCheck();

			_script.enableCamera = EditorGUILayout.BeginToggleGroup("Enable Camera", _script.enableCamera);

			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Resolution");
				var dropdown = new GUIContent(CameraConfigurationToString(_script.config));
				var rect = EditorGUILayout.BeginVertical();
					if (EditorGUILayout.DropdownButton(dropdown, FocusType.Keyboard))
					{
						GenericMenu menu = new GenericMenu();
						foreach (var supportedResolution in IsarClientCamera.SupportedResolutions)
						{
							menu.AddItem(
								new GUIContent(CameraConfigurationToString(supportedResolution)),
								_script.config.Equals(supportedResolution),
								OnCameraConfigurationSelected,
								supportedResolution
							);
						}
						menu.DropDown(rect);
					}
				EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			_script.settings.autoExposure = EditorGUILayout.Toggle("Auto Exposure", _script.settings.autoExposure);
			EditorGUI.BeginDisabledGroup(_script.settings.autoExposure);
				_script.settings.exposure = EditorGUILayout.IntSlider("Exposure", (int)_script.settings.exposure, (int)IsarClientCamera.ExposureMin(), (int)IsarClientCamera.ExposureMax());
				_script.settings.exposure = (long)System.Math.Round((double)_script.settings.exposure / IsarClientCamera.ExposureStep()) * IsarClientCamera.ExposureStep();
			EditorGUI.EndDisabledGroup();

			_script.settings.exposureCompensation = EditorGUILayout.Slider("Exposure Compensation", _script.settings.exposureCompensation, IsarClientCamera.ExposureCompensationMin(), IsarClientCamera.ExposureCompensationMax());
			_script.settings.exposureCompensation = (float)System.Math.Round((double)_script.settings.exposureCompensation / IsarClientCamera.ExposureCompensationStep()) * IsarClientCamera.ExposureCompensationStep();
			_script.settings.whiteBalance = EditorGUILayout.IntSlider("White Balance", _script.settings.whiteBalance, IsarClientCamera.WhiteBalanceMin(), IsarClientCamera.WhiteBalanceMax());
			_script.settings.whiteBalance = (int)System.Math.Round((double)_script.settings.whiteBalance / IsarClientCamera.WhiteBalanceStep()) * IsarClientCamera.WhiteBalanceStep();

			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.BeginHorizontal();

				GUILayout.FlexibleSpace();

				var reset = GUILayout.Button(new GUIContent("Reset", null, "Reset to default values"), GUILayout.ExpandWidth(false));
				if (reset == true)
				{
					_script.Reset();
				}

				if (EditorGUI.EndChangeCheck())
				{
					// Note: this is executed per GUI update, this means that it is still possible to undo a change resulting in 0 changes but the _dirty flag still being true
					// this is also the behaviour of unitys default inspector dirty marking
					_dirty = true;
				}

				//_script.dirty = EditorGUILayout.Toggle("Debug dirty", _dirty);

				EditorGUI.BeginDisabledGroup(!(Application.isPlaying /*&& _script.IsConnected*/ && _dirty));

					var send = GUILayout.Button(new GUIContent("Reconfigure", null, "Reconfigure client device")/*, GUILayout.ExpandWidth(false)*/);
					if (send == true)
					{
						_script.Reconfigure();
						_script.dirty = _dirty = false;
					}

				EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndHorizontal();
		}
	}
}

#endif