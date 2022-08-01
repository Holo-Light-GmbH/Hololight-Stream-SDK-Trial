using System.Collections.Generic;
using UnityEditor;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;

namespace HoloLight.Isar
{
	public class IsarARSubsystemsLoaderMetadata : IXRLoaderMetadata
	{
		public string loaderName => "ISAR AR Subsystems";

		public string loaderType => "HoloLight.Isar.ARSubsystems.IsarARSubsystemsLoader";

		public List<BuildTargetGroup> supportedBuildTargets => new List<BuildTargetGroup> { BuildTargetGroup.Standalone, BuildTargetGroup.WSA };
	}

	public class IsarARSubsystemsPackageMetadata : IXRPackageMetadata
	{
		public string packageName => "ISAR Extension for AR Subsystems";

		public string packageId => "com.hololight.isar.arsubsystems";

		public string settingsType => "HoloLight.Isar.IsarXRSettings";

		public List<IXRLoaderMetadata> loaderMetadata => new List<IXRLoaderMetadata>() { new IsarARSubsystemsLoaderMetadata() };
	}

	public class IsarARSubsystemsPackage : IXRPackage
	{
		public IXRPackageMetadata metadata => new IsarARSubsystemsPackageMetadata();

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
