using Microsoft.MixedReality.Toolkit.CameraSystem;
using Microsoft.MixedReality.Toolkit.XRSDK;

namespace HoloLight.Isar.Runtime.MRTK
{
	public class IsarXRSDKCameraSettings : GenericXRSDKCameraSettings
	{
		public IsarXRSDKCameraSettings(
			IMixedRealityCameraSystem cameraSystem,
			string name = null,
			uint priority = 10,
			BaseCameraSettingsProfile profile = null) : base(cameraSystem, name, priority, profile)
		{
		}

		//MRTK's class that we derive from somehow doesn't do the right thing when run in the editor, so
		//here goes a hacky workaround:
		public override bool IsOpaque => false;
	}
}

