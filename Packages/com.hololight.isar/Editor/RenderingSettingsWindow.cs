using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

namespace HoloLight.Isar.Editor
{
	public class RenderingSettingsWindow : EditorWindow
	{
		//Static path and filename related constants
		private static readonly string ISAR_CORE_RUNTIME_PACKAGE_SOURCE_PATH =
			"Packages/com.hololight.isar/Runtime/Resources/";
		private static readonly string ISAR_CORE_EDITOR_PACKAGE_SOURCE_PATH =
			"Packages/com.hololight.isar/Editor/";
		private static readonly string REMOTING_CONFIG_FILE_NAME =
			"remoting-config.cfg";
		private static readonly string RENDER_CONFIG_PRESETS_FILE_NAME =
			"render-config-presets.cfg";

		private static string REMOTING_CONFIG_PATH =
			Path.Combine(ISAR_CORE_RUNTIME_PACKAGE_SOURCE_PATH, REMOTING_CONFIG_FILE_NAME);
		private static string RENDER_CONFIG_PRESETS_PATH =
			Path.Combine(ISAR_CORE_EDITOR_PACKAGE_SOURCE_PATH, RENDER_CONFIG_PRESETS_FILE_NAME);

		//Attributes
		
		//IB: Make this static so that selected dropdown value will be remembered upon new render of UI
		private static int s_selectedPresetIndex;
		private static int NON_MODIFIABLE_PRESETS_INDEX = 3;

		private IsarRemotingConfig _currentRemotingConfig;
		private List<RenderConfig> _renderConfigPresets;
		private RenderConfig _currentRenderConfig;

		//Attributes for handling UI change regarding entering new rendering preset
		private bool _shouldNewPresetUIAppear;
		private string _newPresetName;

//IB: Make sure that streaming assets remoting-config is updated when standalone build is performed
#if UNITY_EDITOR
		class MyCustomBuildProcessor : IPreprocessBuildWithReport
		{
			public int callbackOrder => 0;

			public void OnPreprocessBuild(BuildReport report)
			{
				string sourcePath = System.IO.Path.Combine(ISAR_CORE_RUNTIME_PACKAGE_SOURCE_PATH, REMOTING_CONFIG_FILE_NAME);
				string destinationPath = System.IO.Path.Combine(Application.streamingAssetsPath, REMOTING_CONFIG_FILE_NAME);
				FileUtil.ReplaceFile(sourcePath, destinationPath);
			}
		}
#endif

	public RenderingSettingsWindow() :
			base()
		{
			this._currentRemotingConfig = new IsarRemotingConfig();
			this._currentRenderConfig = new RenderConfig();
			this._renderConfigPresets = new List<RenderConfig>();
			this._shouldNewPresetUIAppear = false;
			this._newPresetName = "";
		}

		// Add menu regarding rendering settings
		[MenuItem("ISAR/Configure Rendering Settings")]
		static void Init()
		{
			IsarRemotingConfig remotingConfig = IsarRemotingConfig.CreateFromJSON(File.ReadAllText(REMOTING_CONFIG_PATH));
			List<RenderConfig> renderingConfigPresets = RenderConfig.CreateFromJSON(File.ReadAllText(RENDER_CONFIG_PRESETS_PATH));

			s_selectedPresetIndex = -1;

			//Set selected index according to preset name initially
			for (int i = 0; i < renderingConfigPresets.Count; i++)
			{
				s_selectedPresetIndex = (renderingConfigPresets[i].name.Equals(remotingConfig.renderConfig.name)) ? i : s_selectedPresetIndex;
			}


			// Get existing open window or if none, make a new one:
			RenderingSettingsWindow window = (RenderingSettingsWindow)EditorWindow.GetWindow(typeof(RenderingSettingsWindow),
				false,
				"ISAR Rendering Settings");

			window.InitializeWindow(remotingConfig, renderingConfigPresets);
		}

		public void InitializeWindow(IsarRemotingConfig cfg, List<RenderConfig> renderConfigPresets)
		{
			this._currentRemotingConfig = cfg;

			//this means previously selected preset is removed so better update current config before showing window
			if (s_selectedPresetIndex == -1)
			{
				s_selectedPresetIndex = 0;

				if (renderConfigPresets.Count > 0)
				{
					this._renderConfigPresets = renderConfigPresets;
					UpdateCurrentRenderingConfigOnPresetChange();
					UpdateConfigFile();
				}
			}
			else
			{
				if (renderConfigPresets.Count > 0)
				{
					this._renderConfigPresets = renderConfigPresets;
					UpdateCurrentRenderingConfigOnPresetChange();
				}
			}
		}

		void OnGUI()
		{
			if (!this._shouldNewPresetUIAppear)
				RenderingPresetsUIOnGUI();
			else
				AddNewPresetUIOnGUI();

			GUILayout.Space(10);

			RenderingSettingsInputsOnGUI();

			GUILayout.Space(2);

			if(!this._shouldNewPresetUIAppear)
				ApplyButtonOnGUI();
		}

		private void RenderingSettingsInputsOnGUI()
		{
			bool greyOutFlag = !(s_selectedPresetIndex > NON_MODIFIABLE_PRESETS_INDEX - 1) && !this._shouldNewPresetUIAppear;

			if (greyOutFlag)
				GUI.enabled = false;

				GUILayout.Label("Rendering Settings", EditorStyles.boldLabel);
			this._currentRenderConfig.width =
				EditorGUILayout.IntField("Width", this._currentRenderConfig.width);
			this._currentRenderConfig.height =
				EditorGUILayout.IntField("Height", this._currentRenderConfig.height);
			this._currentRenderConfig.numViews =
				EditorGUILayout.IntField("Number of views", this._currentRenderConfig.numViews);
			this._currentRenderConfig.bandwidth =
				EditorGUILayout.IntField("Bandwidth", this._currentRenderConfig.bandwidth);
			this._currentRenderConfig.framerate =
				EditorGUILayout.IntField("Framerate", this._currentRenderConfig.framerate);

			if (greyOutFlag)
				GUI.enabled = true;
		}

		private void AddNewPresetUIOnGUI()
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Add Preset", EditorStyles.boldLabel);


			//just limit preset name to 20 characters for now
			GUILayout.BeginHorizontal();
			GUILayout.Label("Preset Name", GUILayout.ExpandWidth(false));
			this._newPresetName = GUILayout.TextField(_newPresetName, 20);
			GUILayout.EndHorizontal();

			GUILayout.Space(2);
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Add"))
			{
				if (!string.IsNullOrWhiteSpace(this._newPresetName))
				{
					this._currentRenderConfig.name = this._newPresetName;
					this._renderConfigPresets.Add(new RenderConfig(this._currentRenderConfig));
					s_selectedPresetIndex = this._renderConfigPresets.Count - 1;

					//Focus fix so the remnants of last entered value no longer lingers in input fields...
					GUI.FocusControl(null);

					UpdatePresetsFile();

					ResetNewPresetUISettings();
				}
				else
				{
					EditorUtility.DisplayDialog("Error", "No preset name set. Please add a preset name before continuing.", "OK");
				}
			}

			if (GUILayout.Button("Cancel"))
			{
				//Focus fix so the remnants of last entered value no longer lingers in input fields...
				GUI.FocusControl(null);

				ResetNewPresetUISettings();
			}

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}

		private void ResetNewPresetUISettings()
		{
			this._shouldNewPresetUIAppear = false;
			this._newPresetName = "";
		}

		private void RenderingPresetsUIOnGUI()
		{
			GUILayout.Label("Rendering Presets", EditorStyles.boldLabel);
			GUILayout.BeginVertical();
			DropDownOnGUI();
			GUILayout.Space(2);
			AddRemovePresetButtonsOnGUI();
			GUILayout.EndVertical();
		}

		private void DropDownOnGUI()
		{
			if (_renderConfigPresets.Count > 0)
			{
				string[] presetNames = _renderConfigPresets.Select(item => item.name).ToArray();

				int newIndex = EditorGUILayout.Popup(s_selectedPresetIndex, presetNames);
				if (s_selectedPresetIndex != newIndex)
				{
					s_selectedPresetIndex = newIndex;
					UpdateCurrentRenderingConfigOnPresetChange();
				}
			}
		}


		private void AddRemovePresetButtonsOnGUI()
		{
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Add New Preset"))
			{
				//Focus fix so the remnants of last entered value lingers in input fields...
				GUI.FocusControl(null);
				this._shouldNewPresetUIAppear = true;
			}

			//Remove button logic
			if (_renderConfigPresets.Count > 0 && 
				this._renderConfigPresets.Count > NON_MODIFIABLE_PRESETS_INDEX &&
				s_selectedPresetIndex > NON_MODIFIABLE_PRESETS_INDEX - 1 && 
				GUILayout.Button("Remove Preset"))
			{
				this._renderConfigPresets.RemoveAt(s_selectedPresetIndex);
				s_selectedPresetIndex--;
				UpdateCurrentRenderingConfigOnPresetChange();
				UpdatePresetsFile(); 
			}

			GUILayout.EndHorizontal();
		}

		private void UpdatePresetsFile()
		{
			string jsonOut = RenderConfig.ToPresetsJSON(this._renderConfigPresets);
			File.WriteAllText(RENDER_CONFIG_PRESETS_PATH, jsonOut);

		}

		private void UpdateConfigFile()
		{
			this._currentRemotingConfig.renderConfig = new RenderConfig(this._currentRenderConfig);
			string jsonOut = IsarRemotingConfig.ToJSON(this._currentRemotingConfig);
			File.WriteAllText(REMOTING_CONFIG_PATH, jsonOut);
		}

		private void UpdateCurrentRenderingConfigOnPresetChange()
		{
			this._currentRenderConfig = new RenderConfig(_renderConfigPresets[s_selectedPresetIndex]);
		}

		private void ApplyButtonOnGUI()
		{
			if (GUILayout.Button("Apply"))
			{
				UpdateConfigFile();
				if (s_selectedPresetIndex >= NON_MODIFIABLE_PRESETS_INDEX && s_selectedPresetIndex <= _renderConfigPresets.Count)
				{
					_renderConfigPresets[s_selectedPresetIndex] = this._currentRenderConfig;
					UpdatePresetsFile();
				}
				GUIUtility.hotControl = 0;
				this.Close();
			}
		}
	}
}

