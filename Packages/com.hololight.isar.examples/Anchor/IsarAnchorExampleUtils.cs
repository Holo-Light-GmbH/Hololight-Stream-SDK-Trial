using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HoloLight.Isar.ARSubsystems;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class IsarAnchorExampleUtils : MonoBehaviour
{
	private static readonly string ANCHOR_DATA_SOURCE_PATH = "Packages/com.hololight.isar.examples/Anchor/";
	private static readonly string ANACHOR_DATA_SOURCE_NAME = "data.isa";

	public GameObject LoaderAnimationObj;
	public GameObject MenuObject;
	public GameObject DialogPrefab;

	IsarXRAnchorSubsystem _subsystem;
	ARAnchorManager _anchorManager;

	public void Start()
	{
		if(LoaderAnimationObj != null)
			LoaderAnimationObj.SetActive(false);

		_anchorManager = FindObjectOfType<ARAnchorManager>();
		if (_anchorManager == null)
		{
			Debug.LogError("Could not find an ARAnchorManager in the Scene. Please add an ARAnchorManager Object to the Scene.");
            return;
		}

		_subsystem = (IsarXRAnchorSubsystem)_anchorManager.subsystem;

		if (_subsystem == null)
			Debug.LogError("Error ISAR XR Anchor Subsystem is not loaded, " +
						   "please enable it from Edit -> XR Plug-in Management -> ISAR AR Subsystems");
	}

	public async void ExportAnchors()
	{
		// 25 seconds timeout time for export operation
		uint timeoutMs = 25000;

		await ShowLoadingIndicator("Exporting Anchors");

		var exportTask = await _subsystem.ExportAnchors(timeoutMs);


		if (exportTask.isSuccess && exportTask.data.Length > 0)
		{
			await HideLoadingIndicator();

			gameObject.SetActive(true);

			string sourceDataPath = Path.Combine(ANCHOR_DATA_SOURCE_PATH, ANACHOR_DATA_SOURCE_NAME);

			if (File.Exists(sourceDataPath))
			{
				File.Delete(sourceDataPath);
			}

			File.Create(sourceDataPath, exportTask.data.Length).Dispose();
			File.WriteAllBytes(sourceDataPath, exportTask.data);

			Dialog.Open(DialogPrefab,
						DialogButtonType.OK,
						"Export Status",
						"Anchors are Succesfully Exported",
						true);

		}
		else
		{
			await HideLoadingIndicator();

			Dialog.Open(DialogPrefab,
						DialogButtonType.OK,
						"Export Status",
						"Exporting of Anchors is Failed. Please retry exporting.",
						true);
		}

	}

	private async Task<bool> ShowLoadingIndicator(string message)
	{
		MenuObject.SetActive(false);
		LoaderAnimationObj.SetActive(true);
		LoaderAnimationObj.GetComponent<ProgressIndicatorOrbsRotator>().Message = message;
		await LoaderAnimationObj.GetComponent<ProgressIndicatorOrbsRotator>().OpenAsync();
		return true;
	}

	private async Task<bool> HideLoadingIndicator()
	{
		await LoaderAnimationObj.GetComponent<ProgressIndicatorOrbsRotator>().CloseAsync();
		LoaderAnimationObj.SetActive(false);
		MenuObject.SetActive(true);
		return true;
	}

	public async void ImportAnchors()
	{

		// 25 seconds timeout time for import operation
		uint timeoutMs = 25000;

		string sourceDataPath = Path.Combine(ANCHOR_DATA_SOURCE_PATH, ANACHOR_DATA_SOURCE_NAME);

		await ShowLoadingIndicator("Importing Anchors");

		if (File.Exists(sourceDataPath))
		{
			byte[] data = File.ReadAllBytes(sourceDataPath);
			var importTask = await _subsystem.ImportAnchors(data, timeoutMs);
			
			if (importTask)
			{
				await HideLoadingIndicator ();

				Dialog.Open(DialogPrefab,
							DialogButtonType.OK,
							"Anchor Import Status",
							"Anchors are Succesfully Imported",
							true);
			}
			else
			{
				await HideLoadingIndicator ();

				Dialog.Open(DialogPrefab,
							DialogButtonType.OK,
							"Anchor Import Status",
							"Importing of Anchors is Failed. Please retry to import.",
							true);
			}
		}
		else
		{
			await HideLoadingIndicator ();

			Dialog.Open(DialogPrefab,
						DialogButtonType.OK,
						"Anchor Import Status",
						"Anchor file can not be found at path: ",
						true);

		}
	}

	public void ClearAllAnchors()
	{
		GameObject[] anchorObjects = GameObject.FindGameObjectsWithTag("ISARAnchor");

		foreach(GameObject obj in anchorObjects)
		{
			obj.GetComponentInParent<IsarAnchorDeleter>().DeleteAnchor();
		}
	}
}
