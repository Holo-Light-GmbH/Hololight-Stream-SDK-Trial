using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.XR.ARSubsystems;

#if UNITY_EDITOR
#endif

namespace HoloLight.Isar.ARSubsystems
{
	/// <summary>
	/// Takes care of initializing the native library and calls maintains all of the required XR and AR subsystems
	/// </summary>
	public class IsarARSubsystemsLoader : BaseIsarLoader
	{
		private static List<XRSessionSubsystemDescriptor> s_SessionSubsystemDescriptors = new List<XRSessionSubsystemDescriptor>();
		private static List<XRImageTrackingSubsystemDescriptor> s_ImageTrackingSubsystemDescriptors = new List<XRImageTrackingSubsystemDescriptor>();
		private static List<XRCameraSubsystemDescriptor> s_CameraSubsystemDescriptors = new List<XRCameraSubsystemDescriptor>();
		private static List<XRRaycastSubsystemDescriptor> s_RaycastSubsystemDescriptors = new List<XRRaycastSubsystemDescriptor>();
		private static List<XRPlaneSubsystemDescriptor> s_PlaneSubsystemDescriptors = new List<XRPlaneSubsystemDescriptor>();
		private static List<XRDisplaySubsystemDescriptor> s_DisplaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();
		private static List<XRInputSubsystemDescriptor> s_InputSubsystemDescriptors = new List<XRInputSubsystemDescriptor>();

		public XRSessionSubsystem SessionSubsystem { get => GetLoadedSubsystem<XRSessionSubsystem>(); }
		public XRImageTrackingSubsystem ImageTrackingSubsystem { get => GetLoadedSubsystem<XRImageTrackingSubsystem>(); }
		public XRCameraSubsystem CameraSubsystem { get => GetLoadedSubsystem<XRCameraSubsystem>(); }
		public XRRaycastSubsystem RaycastSubsystem { get => GetLoadedSubsystem<XRRaycastSubsystem>(); }
		public XRPlaneSubsystem PlaneSubsystem { get => GetLoadedSubsystem<XRPlaneSubsystem>(); }
		public XRDisplaySubsystem DisplaySubsystem { get => GetLoadedSubsystem<XRDisplaySubsystem>(); }
		public XRInputSubsystem InputSubsystem { get => GetLoadedSubsystem<XRInputSubsystem>(); }

		protected override bool CreateSubsystems()
		{
			CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(s_SessionSubsystemDescriptors, "ISAR Session");
			CreateSubsystem<XRImageTrackingSubsystemDescriptor, XRImageTrackingSubsystem>(s_ImageTrackingSubsystemDescriptors, "ISAR Image Tracking");
			CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>(s_CameraSubsystemDescriptors, "ISAR Camera");
			CreateSubsystem<XRRaycastSubsystemDescriptor, XRRaycastSubsystem>(s_RaycastSubsystemDescriptors, "ISAR Raycast");
			CreateSubsystem<XRPlaneSubsystemDescriptor, XRPlaneSubsystem>(s_PlaneSubsystemDescriptors, "ISAR Plane");
			CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(s_DisplaySubsystemDescriptors, "ISAR Display");
			CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(s_InputSubsystemDescriptors, "ISAR Input");

			return true;
		}

		protected override bool StartSubsystems()
		{
			StartSubsystem<XRSessionSubsystem>();
			StartSubsystem<XRDisplaySubsystem>();
			StartSubsystem<XRInputSubsystem>();

			return true;
		}

		protected override bool StopSubsystems()
		{
			StopSubsystem<XRSessionSubsystem>();
			StopSubsystem<XRInputSubsystem>();
			StopSubsystem<XRDisplaySubsystem>();

			return true;
		}

		protected override bool DestroySubsystems()
		{
			DestroySubsystem<XRSessionSubsystem>();
			DestroySubsystem<XRImageTrackingSubsystem>();
			DestroySubsystem<XRCameraSubsystem>();
			DestroySubsystem<XRRaycastSubsystem>();
			DestroySubsystem<XRPlaneSubsystem>();
			DestroySubsystem<XRInputSubsystem>();
			DestroySubsystem<XRDisplaySubsystem>();

			return true;
		}
	}
}
