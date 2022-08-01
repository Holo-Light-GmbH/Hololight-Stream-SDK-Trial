using HoloLight.Isar.Signaling;
using UnityEngine;
using UnityEngine.XR.Management;

namespace HoloLight.Isar
{
	public static class Constants
	{
		public const string ISAR_XR_SETTINGS_KEY = "HoloLight.ISAR.Settings";
	}

	//This attribute makes the data here appear inside "Project Settings > XR Plug-in Management > ISAR"
	[XRConfigurationData("ISAR", Constants.ISAR_XR_SETTINGS_KEY)]
	public class IsarXRSettings : ScriptableObject
	{

		[Tooltip("Choose how Unity should render. This can impact performance.")]
		public RenderingMode RenderingMode;

		[Tooltip("An implementation of ISignaling to be instantiated and used by the XR loader (via reflection). Make sure to use the fully-qualified type name.")]
		public string SignalingImplementationType = typeof(DebugSignaling).FullName;

		[Tooltip("Send depth/alpha data for stabilization/reprojection (Beta Version)")]
		public bool SendDepthAlpha_Preview;

		//Copy-pasted from WindowsMRSettings.
		//At runtime this is basically a singleton. IsarXRBuildProcessor (or rather its base class) takes care
		//that an instance of this will be in the build and preloaded before "regular" assets. We save this
		//instance in Awake() and can be sure it exists later when we want to use it (in IsarXRLoader).
		//For the editor there's an extra settings API that we can query if we want the current value.
#if !UNITY_EDITOR
		internal static IsarXRSettings s_Settings;

		public void Awake()
		{
			s_Settings = this;
		}
#endif
	}

	public enum RenderingMode : int
	{
		MultiPass = 0,
		SinglePassInstanced = 1
	}
}
