using UnityEditor.XR.Management;

namespace HoloLight.Isar
{
	/// <summary>
	/// This class makes sure that we get an instance of our settings ScriptableObject in the build.
	/// </summary>
	public class IsarXRBuildProcessor : XRBuildHelper<IsarXRSettings>
	{
		public override string BuildSettingsKey => Constants.ISAR_XR_SETTINGS_KEY;
	}
}

