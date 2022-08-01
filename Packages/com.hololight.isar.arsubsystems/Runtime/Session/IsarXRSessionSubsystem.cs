using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
#endif

namespace HoloLight.Isar.ARSubsystems
{
	/// <summary>
	/// ISAR implementation of the <c>XRSessionSubsystem</c>. Do not create this directly. Use the <c>SubsystemManager</c> instead.
	/// </summary>
	[Preserve]
	public sealed class IsarXRSessionSubsystem : XRSessionSubsystem
	{

#if !UNITY_2020_2_OR_NEWER
		/// <summary>
		/// Creates the provider interface.
		/// </summary>
		/// <returns>The provider interface for ISAR</returns>
		protected override Provider CreateProvider()
		{
			return new IsarProvider(this);
		}
#endif

		class IsarProvider : Provider
		{
			IsarTouchscreen _touchscreen;

#if UNITY_2020_2_OR_NEWER
			public IsarProvider()
#else
			IsarXRSessionSubsystem m_Subsystem;
            public IsarProvider(IsarXRSessionSubsystem subsystem)
#endif
			{
#if !UNITY_2020_2_OR_NEWER
                m_Subsystem = subsystem;
#endif

				InputSystem.RegisterLayout<IsarTouchscreen>();
			}

#if UNITY_2020_2_OR_NEWER
			public override void Start()
#else
            public override void Resume()
#endif
			{
				// Add a touchscreen device to unity
				_touchscreen = InputSystem.AddDevice<IsarTouchscreen>("ISAR Touchscreen");
			}

#if UNITY_2020_2_OR_NEWER
			public override void Stop()
#else
            public override void Pause()
#endif
			{ 
				InputSystem.RemoveDevice(_touchscreen);
			}

			public override Promise<SessionAvailability> GetAvailabilityAsync()
			{
				var promise = new IsarXRPromise<SessionAvailability>();
				promise.Resolve(SessionAvailability.Supported | SessionAvailability.Installed);
				return promise;
			}

			public override Promise<SessionInstallationStatus> InstallAsync()
			{
				var promise = new IsarXRPromise<SessionInstallationStatus>();
				promise.Resolve(SessionInstallationStatus.None);
				return promise;
			}

			public override TrackingState trackingState
			{
				get
				{
					return TrackingState.None;
				}
			}

			public override NotTrackingReason notTrackingReason
			{
				get
				{
					return NotTrackingReason.None;
				}
			}

			public override bool matchFrameRateEnabled
			{
				get
				{
					return false;
				}
			}

			/// <summary>
			/// Won't ever be called as matchFrameRateEnabled is currently false. 
			/// </summary>
			public override int frameRate
			{
				get { return 30; }
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void RegisterDescriptor()
		{
			XRSessionSubsystemDescriptor.RegisterDescriptor(new XRSessionSubsystemDescriptor.Cinfo
			{
				id = "ISAR Session",
#if UNITY_2020_2_OR_NEWER
				providerType = typeof(IsarXRSessionSubsystem.IsarProvider),
				subsystemTypeOverride = typeof(IsarXRSessionSubsystem),
#else
				subsystemImplementationType = typeof(IsarXRSessionSubsystem),
#endif
				supportsInstall = false,
				supportsMatchFrameRate = false
			});
		}

		static class NativeApi
		{

		}
	}
}