//This is deprecated with XRSDK, so bye asset menu.
#if ISAR_LEGACY
using UnityEditor;
using UnityEngine;

namespace HoloLight.Isar.Editor
{
	public class ConfigMenuHelper : MonoBehaviour
	{
		/// <summary>
		/// Add menu item "Add RemoteCamera" to ISAR menu.
		/// </summary>
		[MenuItem("ISAR/Add RemoteCamera")]
		static void ConfigureRemotingCamera()
		{
			AddRemoteCamera();
		}

		/// <summary>
		/// Add RemoteCamera script to main camera.
		/// </summary>
		private static void AddRemoteCamera()
		{
			Debug.Log("ISAR: Trying to add RemoteCamera ...");
			Camera mainCamera = Camera.main;

			if (mainCamera == null)
			{
				Debug.LogWarning("ISAR: No main camera found. Can not add component RemoteCamera.");
				return;
			}

			bool isRemoteCameraAdded = mainCamera.GetComponentInChildren(typeof(RemoteCamera), true);

			if (!isRemoteCameraAdded)
			{
				mainCamera.gameObject.AddComponent<RemoteCamera>();
			}

			Debug.Log("ISAR: RemoteCamera added successfully.");
		}
	}
}
#endif