using UnityEditor;
using UnityEngine;

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Editor;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;

namespace HoloLight.Isar.Editor
{
	public class MrtkConfigMenuHelper : MonoBehaviour
	{
		public const string DEFAULT_REMOTE_CONFIG_PROFILE = "IsarXRSDKConfigurationProfile";

		// Add a menu item named "Configure MRTK" to ISAR main menu.
		[MenuItem("ISAR/Configure MRTK")]
		static void ConfigureMrtkRemoting()
		{
			AddRemotingProfile();
		}

		private static void AddRemotingProfile()
		{
			Debug.Log("ISAR: Trying to set configuration profile ...");

			var remotingProfile = GetDefaultRemotingConfigProfile();

			MixedRealityInspectorUtility.AddMixedRealityToolkitToScene(remotingProfile);

			bool hasProfile = MixedRealityToolkit.Instance.HasActiveProfile;
			bool hasRemotingProfile = hasProfile ? MixedRealityToolkit.Instance.ActiveProfile.name == DEFAULT_REMOTE_CONFIG_PROFILE : false;

			if (remotingProfile == null)
			{
				Debug.LogWarning($"ISAR: No configuration profile {DEFAULT_REMOTE_CONFIG_PROFILE} found.");
				return;
			}

			if (!hasProfile || !hasRemotingProfile)
			{
				MixedRealityToolkit.Instance.ActiveProfile = remotingProfile;
				Debug.Log($"ISAR: Configuration profile {DEFAULT_REMOTE_CONFIG_PROFILE} added successfully.");
			}
		}

		public static MixedRealityToolkitConfigurationProfile GetDefaultRemotingConfigProfile()
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
				if (profile.name == DEFAULT_REMOTE_CONFIG_PROFILE)
				{
					return profile;
				}
			}

			return null;
		}
	}
}