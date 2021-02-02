/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System.IO;

using UnityEngine;
using UnityEditor;

namespace HoloLight.Isar.Editor
{
	/// <summary>
	/// Copies resource files which are needed during runtime
	/// from Resources folder to Assets folder at package installation.
	/// </summary>
	public class ConfigLoader
	{
		private static readonly string REMOTING_PACKAGE_SOURCE_PATH =
			"Packages/com.hololight.isar/Runtime/Resources/";

		private static readonly string CONFIG_FILE = "remoting-config.cfg";
		private static readonly string LINK_XML_FILE = "link.xml";

		// This method is triggered on package installation
		// and copies remoting-config.cfg file to StreamingAssets folder
		[InitializeOnLoadMethod]
		private static void LoadConfig()
		{
			if (!Directory.Exists(Application.streamingAssetsPath))
			{
				Directory.CreateDirectory(Application.streamingAssetsPath);
			}

			string assetsConfigPath = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE);

			// copy remoting.cfg
			//if (!File.Exists(assetsConfigPath))
			//{
			string sourceConfigFilePath = Path.Combine(REMOTING_PACKAGE_SOURCE_PATH, CONFIG_FILE);
			string destinationConfigFilePath = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE);

			if (File.Exists(sourceConfigFilePath))
			{
				File.Copy(sourceConfigFilePath, destinationConfigFilePath, true);
			}
			//}

			assetsConfigPath = Path.Combine(Application.streamingAssetsPath, LINK_XML_FILE);

			// copy link.xml
			// link.xml prevents source code stripping
			//if (!File.Exists(assetsConfigPath))
			//{
			string sourceLinkFilePath = Path.Combine(REMOTING_PACKAGE_SOURCE_PATH, LINK_XML_FILE);
			string destinationLinkFilePath = Path.Combine(Application.streamingAssetsPath, LINK_XML_FILE);

			if (File.Exists(sourceLinkFilePath))
			{
				File.Copy(sourceLinkFilePath, destinationLinkFilePath, true);
			}
			//}
		}
	}
}
