using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

#if UNITY_EDITOR
#endif

namespace HoloLight.Isar.ARSubsystems
{
	/// <summary>
	/// ISAR implementation of the <c>XRSessionSubsystem</c>. Do not create this directly. Use the <c>SubsystemManager</c> instead.
	/// </summary>
	[Preserve]
	public sealed class IsarXRCameraSubsystem : XRCameraSubsystem
	{
		private IsarProvider _provider;

#if !UNITY_2020_2_OR_NEWER
		protected override Provider CreateProvider()
		{
			_provider = new IsarProvider();

			return _provider;
		}

#endif

		class IsarProvider : Provider
		{
			public IsarProvider()
			{

			}

		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void RegisterDescriptor()
		{
			XRCameraSubsystem.Register(new XRCameraSubsystemCinfo
			{
				id = "Isar Camera",
#if UNITY_2020_2_OR_NEWER
                providerType = typeof(IsarXRCameraSubsystem.IsarProvider),
                subsystemTypeOverride = typeof(IsarXRCameraSubsystem),
#else
				implementationType = typeof(IsarXRCameraSubsystem),
#endif
				supportsAverageBrightness = false,
				supportsAverageColorTemperature = false,
				supportsColorCorrection = false,
				supportsDisplayMatrix = false,
				supportsProjectionMatrix = false,
				supportsTimestamp = false,
				supportsCameraConfigurations = false,
				supportsCameraImage = false,
				supportsAverageIntensityInLumens = false,
				supportsFocusModes = false,
				supportsFaceTrackingAmbientIntensityLightEstimation = false,
				supportsFaceTrackingHDRLightEstimation = false,
				supportsWorldTrackingAmbientIntensityLightEstimation = false,
				supportsWorldTrackingHDRLightEstimation = false,
				supportsCameraGrain = false,
			});
		}
	}

	static class NativeApi
	{

	}

}