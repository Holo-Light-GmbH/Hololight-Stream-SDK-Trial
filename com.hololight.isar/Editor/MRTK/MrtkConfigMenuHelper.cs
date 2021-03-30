using UnityEditor;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Editor;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;

namespace HoloLight.Isar.Editor
{
	public class MrtkConfigMenuHelper : MonoBehaviour
	{
		public const string DEFAULT_ISAR_CONFIG_PROFILE = "IsarXRSDKConfigurationProfile";

		// Add a menu item named "Configure MRTK" to ISAR main menu.
		[MenuItem("ISAR/Configure MRTK")]
		static void ConfigureMrtk()
		{
			AddProfile();
		}

		private static void AddProfile()
		{
			//Debug.Log("ISAR: Trying to set configuration profile ...");

			var isarProfile = GetDefaultIsarConfigProfile();
			if (isarProfile == null)
			{
				Debug.LogWarning($"ISAR: Configuration profile {DEFAULT_ISAR_CONFIG_PROFILE} not found.");
				return;
			}

			if (MixedRealityToolkit.IsInitialized &&
			    MixedRealityToolkit.Instance.ActiveProfile == isarProfile)
			{
				Debug.Log($"ISAR: Configuration profile {DEFAULT_ISAR_CONFIG_PROFILE} is already active.");
				return;
			}

			// NOTE: initializes MRTK if not initialized
			MixedRealityInspectorUtility.AddMixedRealityToolkitToScene();

			MixedRealityToolkit.Instance.ActiveProfile = isarProfile;
			if (MixedRealityToolkit.Instance.ActiveProfile == isarProfile)
			{
				Debug.Log($"ISAR: Successfully activated configuration profile {DEFAULT_ISAR_CONFIG_PROFILE}.");
			}
			else
			{
				Debug.LogWarning($"ISAR: Unable to activate configuration profile {DEFAULT_ISAR_CONFIG_PROFILE}.");

			}
		}

		public static MixedRealityToolkitConfigurationProfile GetDefaultIsarConfigProfile()
		{
			var allConfigProfiles =
				ScriptableObjectExtensions.GetAllInstances<MixedRealityToolkitConfigurationProfile>();
			return GetRemotingConfigProfile(allConfigProfiles);
		}

		public static MixedRealityToolkitConfigurationProfile GetRemotingConfigProfile(
			MixedRealityToolkitConfigurationProfile[] allProfiles)
		{
			foreach (var profile in allProfiles)
			{
				if (profile.name == DEFAULT_ISAR_CONFIG_PROFILE)
				{
					return profile;
				}
			}

			return null;
		}
	}
}