using UnityEditor.XR.Management;

namespace Unity.XR.Isar
{
	/// <summary>
	/// This class makes sure that we get an instance of our settings ScriptableObject in the build.
	/// </summary>
	public class IsarXRBuildProcessor : XRBuildHelper<IsarXRSettings>
	{
		public override string BuildSettingsKey => "Unity.XR.HoloLight.ISAR";
	}
}

