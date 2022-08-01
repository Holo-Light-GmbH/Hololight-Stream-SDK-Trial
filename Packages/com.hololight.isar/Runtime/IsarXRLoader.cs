
//#define USE_SIGNALING_FALLBACK

using System.Collections.Generic;
using UnityEngine.XR;

using UnityEngine.XR.Management;
using System.Threading;

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

		public XRDisplaySubsystem DisplaySubsystem { get => GetLoadedSubsystem<XRDisplaySubsystem>(); }
		public XRInputSubsystem InputSubsystem { get => GetLoadedSubsystem<XRInputSubsystem>(); }
		public XRMeshSubsystem MeshSubsystem { get => GetLoadedSubsystem<XRMeshSubsystem>(); }

		protected override bool CreateSubsystems()
		{
			CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(s_DisplaySubsystemDescriptors, "ISAR Display");
			CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(s_InputSubsystemDescriptors, "ISAR Input");
			CreateSubsystem<XRMeshSubsystemDescriptor, XRMeshSubsystem>(s_MeshSubsystemDescriptors, "ISAR Meshing");

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
			StopSubsystem<XRInputSubsystem>();
			StopSubsystem<XRDisplaySubsystem>();
			StopSubsystem<XRMeshSubsystem>();

			return true;
		}

		protected override bool DestroySubsystems()
		{
			DestroySubsystem<XRInputSubsystem>();
			DestroySubsystem<XRDisplaySubsystem>();
			DestroySubsystem<XRMeshSubsystem>();

			return true;
		}
	}
}
