using UnityEngine;
using UnityEditor;
using System.IO;

namespace HoloLight.Isar.Editor
{
	public class PortSettingWindow : EditorWindow
	{
		//Static path and filename related constants
		private static readonly string ISAR_CORE_RUNTIME_PACKAGE_SOURCE_PATH =
			"Packages/com.hololight.isar/Runtime/Resources/";
		private static readonly string REMOTING_CONFIG_FILE_NAME =
			"remoting-config.cfg";
		private static string REMOTING_CONFIG_PATH =
			Path.Combine(ISAR_CORE_RUNTIME_PACKAGE_SOURCE_PATH, REMOTING_CONFIG_FILE_NAME);

		private static IsarRemotingConfig _remotingConfig;

		[MenuItem("ISAR/Set Signaling Port")]
		private static void Init()
		{
			_remotingConfig = IsarRemotingConfig.CreateFromJSON(File.ReadAllText(REMOTING_CONFIG_PATH));
			PortSettingWindow window = (PortSettingWindow)EditorWindow.GetWindow<PortSettingWindow>(false, "Set Signaling Port");
		}

		private void OnGUI()
		{
			GUILayout.Space(10);

			_remotingConfig.signalingConfig.port = EditorGUILayout.IntField("Port Number", _remotingConfig.signalingConfig.port);
			
			GUILayout.Space(2);

			if (_remotingConfig.signalingConfig.port < 1024 ||
			   _remotingConfig.signalingConfig.port > 65535)
			{
				EditorGUILayout.LabelField("Port number out of bounds, please enter a number between 1024 and 65535.");
			}
			else
			{
				if (GUILayout.Button("Apply"))
				{
					string jsonOut = IsarRemotingConfig.ToJSON(_remotingConfig);
					File.WriteAllText(REMOTING_CONFIG_PATH, jsonOut);
					GUIUtility.hotControl = 0;
					this.Close();
				}
			}
		}
	}
}