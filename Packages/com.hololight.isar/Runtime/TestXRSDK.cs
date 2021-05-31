using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

/// <summary>
/// Sample class showing how to manually Init/Start/Stop/Destroy XRSDK Loaders.
/// </summary>
public class TestXRSDK : MonoBehaviour
{
	void Start()
	{
		//we could create test ui buttons for testing enabling/disabling XR mode
		//..but I guess that'd only work if we have rendering since we can't see the UI otherwise :)
		//PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
		Debug.Log($"streamingAssetsPath: {Application.streamingAssetsPath}, workDir: {System.IO.Directory.GetCurrentDirectory()}");
		
		LogXRSettings();

		StartCoroutine(InitXR());
	}

	private void OnDisable()
	{
		LogXRSettings();

		DestroyXR();
	}

	public IEnumerator InitXR()
	{
		Debug.Log("Initializing XR...");
		yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
	}

	public void StartXR()
	{
		if (XRGeneralSettings.Instance.Manager.activeLoader == null)
		{
			Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
		}
		else
		{
			//since init is called from a coroutine and you can't really wait for coroutines to be done
			//without ugly hacks, let's just check this flag and be done with it.
			if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
			{
				Debug.Log("Starting XR...");
				XRGeneralSettings.Instance.Manager.StartSubsystems();
			}
		}
	}

	public void StopXR()
	{
		Debug.Log("Stopping XR...");

		if (XRGeneralSettings.Instance.Manager.activeLoader != null)
		{
			XRGeneralSettings.Instance.Manager.StopSubsystems();
		}
	}

	private void DestroyXR()
	{
		Debug.Log("Destroying XR...");
		LogXRSettings();

		if (XRGeneralSettings.Instance.Manager.activeLoader != null)
		{
			XRGeneralSettings.Instance.Manager.DeinitializeLoader();
			Debug.Log("XR stopped completely.");
		}
	}

	private void LogXRSettings()
	{
		var stereoMode = XRSettings.stereoRenderingMode;
		Debug.Log($"Stereo mode: {stereoMode}");

		var displays = new List<XRDisplaySubsystem>();
		SubsystemManager.GetInstances<XRDisplaySubsystem>(displays);

		if (displays.Count > 0)
		{
			bool singlePassDisabled = displays[0].singlePassRenderingDisabled;
			bool legacyRendererDisabled = displays[0].disableLegacyRenderer;

			Debug.Log($"Single pass disabled: {singlePassDisabled}, legacy renderer disabled: {legacyRendererDisabled}");
		}
	}
}
