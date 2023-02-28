
//#define USE_SIGNALING_FALLBACK

using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.XR.ARSubsystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HoloLight.Isar
{
	/// <summary>
	/// Takes care of initializing the native library and calls the display/input providers' lifecycle methods.
	/// </summary>
	public class IsarXRLoader : BaseIsarLoader
	{
		private static List<XRDisplaySubsystemDescriptor> s_DisplaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();
		private static List<XRInputSubsystemDescriptor> s_InputSubsystemDescriptors = new List<XRInputSubsystemDescriptor>();
		private static List<XRMeshSubsystemDescriptor> s_MeshSubsystemDescriptors = new List<XRMeshSubsystemDescriptor>();
		private static List<XRSessionSubsystemDescriptor> s_SessionSubsystemDescriptors = new List<XRSessionSubsystemDescriptor>();
		private static List<XRImageTrackingSubsystemDescriptor> s_ImageTrackingSubsystemDescriptors = new List<XRImageTrackingSubsystemDescriptor>();
		private static List<XRCameraSubsystemDescriptor> s_CameraSubsystemDescriptors = new List<XRCameraSubsystemDescriptor>();
		private static List<XRRaycastSubsystemDescriptor> s_RaycastSubsystemDescriptors = new List<XRRaycastSubsystemDescriptor>();
		private static List<XRPlaneSubsystemDescriptor> s_PlaneSubsystemDescriptors = new List<XRPlaneSubsystemDescriptor>();
		private static List<XRAnchorSubsystemDescriptor> s_AnchorSubsystemDescriptors = new List<XRAnchorSubsystemDescriptor>();

		public XRDisplaySubsystem DisplaySubsystem { get => GetLoadedSubsystem<XRDisplaySubsystem>(); }
		public XRInputSubsystem InputSubsystem { get => GetLoadedSubsystem<XRInputSubsystem>(); }
		public XRMeshSubsystem MeshSubsystem { get => GetLoadedSubsystem<XRMeshSubsystem>(); }
		public XRSessionSubsystem SessionSubsystem { get => GetLoadedSubsystem<XRSessionSubsystem>(); }
		public XRImageTrackingSubsystem ImageTrackingSubsystem { get => GetLoadedSubsystem<XRImageTrackingSubsystem>(); }
		public XRCameraSubsystem CameraSubsystem { get => GetLoadedSubsystem<XRCameraSubsystem>(); }
		public XRRaycastSubsystem RaycastSubsystem { get => GetLoadedSubsystem<XRRaycastSubsystem>(); }
		public XRPlaneSubsystem PlaneSubsystem { get => GetLoadedSubsystem<XRPlaneSubsystem>(); }
		public XRAnchorSubsystem AnchorSubsystem { get => GetLoadedSubsystem<XRAnchorSubsystem>(); }

		protected override bool CreateSubsystems()
		{
			CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(s_DisplaySubsystemDescriptors, "ISAR Display");
			CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(s_InputSubsystemDescriptors, "ISAR Input");
			CreateSubsystem<XRMeshSubsystemDescriptor, XRMeshSubsystem>(s_MeshSubsystemDescriptors, "ISAR Meshing");
			CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(s_SessionSubsystemDescriptors, "ISAR Session");
			CreateSubsystem<XRImageTrackingSubsystemDescriptor, XRImageTrackingSubsystem>(s_ImageTrackingSubsystemDescriptors, "ISAR Image Tracking");
			CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>(s_CameraSubsystemDescriptors, "ISAR Camera");
			CreateSubsystem<XRRaycastSubsystemDescriptor, XRRaycastSubsystem>(s_RaycastSubsystemDescriptors, "ISAR Raycast");
			CreateSubsystem<XRPlaneSubsystemDescriptor, XRPlaneSubsystem>(s_PlaneSubsystemDescriptors, "ISAR Plane");
			CreateSubsystem<XRAnchorSubsystemDescriptor, XRAnchorSubsystem>(s_AnchorSubsystemDescriptors, "ISAR Anchor");

			return true;
		}

		protected override bool StartSubsystems()
		{
			StartSubsystem<XRDisplaySubsystem>();
			StartSubsystem<XRInputSubsystem>();
			StartSubsystem<XRMeshSubsystem>();

			return true;
		}

		protected override bool StopSubsystems()
		{
			StopSubsystem<XRMeshSubsystem>();
			StopSubsystem<XRInputSubsystem>();
			StopSubsystem<XRDisplaySubsystem>();

			return true;
		}

		protected override bool DestroySubsystems()
		{
			DestroySubsystem<XRMeshSubsystem>();
			DestroySubsystem<XRInputSubsystem>();
			DestroySubsystem<XRDisplaySubsystem>();
			DestroySubsystem<XRSessionSubsystem>();
			DestroySubsystem<XRImageTrackingSubsystem>();
			DestroySubsystem<XRCameraSubsystem>();
			DestroySubsystem<XRRaycastSubsystem>();
			DestroySubsystem<XRPlaneSubsystem>();
			DestroySubsystem<XRAnchorSubsystem>();

			return true;
		}
	}
}
