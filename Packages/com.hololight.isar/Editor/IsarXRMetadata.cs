using System.Collections.Generic;
using UnityEditor;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;

namespace HoloLight.Isar
{
	public class IsarXRLoaderMetadata : IXRLoaderMetadata
	{
		public string loaderName => "ISAR XR";

		public string loaderType => "HoloLight.Isar.IsarXRLoader";

		public List<BuildTargetGroup> supportedBuildTargets => new List<BuildTargetGroup> { BuildTargetGroup.Standalone, BuildTargetGroup.WSA };
	}

	public class IsarXRPackageMetadata : IXRPackageMetadata
	{
		public string packageName => "ISAR XRSDK";

		public string packageId => "com.hololight.isar";

		public string settingsType => "HoloLight.Isar.IsarXRSettings";

		public List<IXRLoaderMetadata> loaderMetadata => new List<IXRLoaderMetadata>() { new IsarXRLoaderMetadata() };
	}

	public class IsarXRPackage : IXRPackage
	{
		public IXRPackageMetadata metadata => new IsarXRPackageMetadata();

		public bool PopulateNewSettingsInstance(ScriptableObject obj)
		{
			var settings = obj as IsarXRSettings;

			if (settings != null)
			{
				//Let's do single-pass by default when creating a new instance of XR settings
				settings.RenderingMode = RenderingMode.SinglePassInstanced;
			}

			return true;
		}
	}
}
