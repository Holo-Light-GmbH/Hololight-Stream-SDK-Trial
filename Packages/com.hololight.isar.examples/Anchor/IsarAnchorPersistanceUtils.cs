using UnityEngine;
using UnityEngine.XR.ARFoundation;
using HoloLight.Isar.ARSubsystems;
using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;


public class IsarAnchorPersistanceUtils : MonoBehaviour
{
	public GameObject DialogPrefab;

	IsarXRAnchorSubsystem _subsystem;
	ARAnchorManager _anchorManager;
	
	private string _anchorNamePrefix = "PAnchor";
	static int s_persistedAnchorIndex = 0;

	private void Start()
	{
		_anchorManager = FindObjectOfType<ARAnchorManager>();
		if(_anchorManager == null)
		{
			Debug.LogError("Could not find an ARAnchorManager in the Scene. Please add an ARAnchorManager Object to the Scene.");
            return;
		}
		_subsystem = (IsarXRAnchorSubsystem)_anchorManager.subsystem;

		if (_subsystem == null)
			Debug.LogError("Error ISAR XR Anchor Subsystem is not loaded, " +
						   "please enable it from Edit -> XR Plug-in Management -> ISAR AR Subsystems");
	}

	public async void  EnumeratePersistedNamesAsync()
	{
		uint timeout_ms = 2000;
		var task = await _subsystem.EnumeratePersistedAnchorNames(timeout_ms);

		if (task.isSuccess)
		{
			string info_str = "Succesfully enumerated persisted names: \n";
			foreach (var str in task.data)
			{
				info_str += str + "\n";
			}

			ShowNotification("Enumerating Persistent Anchor Names Status", info_str);
		}
		else
		{
			ShowNotification("Enumerating Persistent Anchor Names Status", "Enumerating Persistent Anchor Names Failed !");
		}
	}

	private void ShowNotification(string title, string message)
	{
		Dialog.Open(DialogPrefab,
			DialogButtonType.OK,
			title,
			message,
			true);
	}


	public async void CreateStoreConnectionAsync()
	{
		uint timeout_ms = 2000;
		var succeeded = await _subsystem.CreateStoreConnection(timeout_ms);

		if (succeeded)
		{
			ShowNotification("Anchor Store Connection", "Store connection is established succesfully");
		}
		else
		{
			ShowNotification("Anchor Store Connection", "Store connection could not be established");
		}
	}

	public async void DestroyStoreConnectionAsync()
	{
		uint timeout_ms = 2000;
		var succeeded = await _subsystem.DestroyStoreConnection(timeout_ms);

		if (succeeded)
		{
			ShowNotification("Anchor Store Connection", "Store connection is destroyed succesfully");
		}
		else
		{
			ShowNotification("Anchor Store Connection", "Store connection could not be destroyed");
		}
	}

	public async void ClearStoreAsync()
	{
		uint timeout_ms = 2000;
		var succeeded = await _subsystem.ClearStore(timeout_ms);

		if (succeeded)
		{
			ShowNotification("Anchor Store Clear", "Store cleared succesfully");

			foreach (var ar_anchor in _anchorManager.trackables)
			{
				IsarAnchor isar_anchor = ar_anchor.GetComponentInParent<IsarAnchor>();
				isar_anchor.PersistentName = "";
				isar_anchor.IsPersisted = false;
			}
		}
		else
		{
			ShowNotification("Anchor Store Clear", "Store could not be cleared succesfully");
		}
	}

	public async void PersistSpatialAnchorAsync()
	{
		uint timeout_ms = 2000;
		string anchor_name = _anchorNamePrefix + s_persistedAnchorIndex;

		ARAnchor anchor = GetComponent<ARAnchor>();
		UnityEngine.XR.ARSubsystems.TrackableId anchor_id = anchor.trackableId;

		var succeeded = await _subsystem.PersistAnchor(anchor_name, anchor_id, timeout_ms);
		if (succeeded)
		{
			s_persistedAnchorIndex++;
			Debug.Log("Anchor persisted succesfully name: " + anchor_name + " id: " + anchor_id.subId1);

			anchor.GetComponentInParent<IsarAnchor>().PersistentName = anchor_name;
			anchor.GetComponentInParent<IsarAnchor>().IsPersisted = true;
		}
	}

	public async void UnpersistSpatialAnchorAsync()
	{
		uint timeout_ms = 2000;
		ARAnchor anchor = GetComponent<ARAnchor>();
		string anchor_name = anchor.GetComponentInParent<IsarAnchor>().PersistentName;

		var is_succeded = await _subsystem.UnpersistAnchor(anchor_name, timeout_ms);
		if (is_succeded)
		{
			Debug.Log("Anchor unpersisted succesfully name: " + anchor_name);
			anchor.GetComponentInParent<IsarAnchor>().IsPersisted = false;
			anchor.GetComponentInParent<IsarAnchor>().PersistentName = "";
		}
	}

	public async void CreateAnchorFromPersistedNameAsync()
	{
		uint timeout_ms = 2000;
		var enumerated_names = await _subsystem.EnumeratePersistedAnchorNames(timeout_ms);

		if (enumerated_names.isSuccess)
		{
			if (enumerated_names.data.Length == 0)
			{
				ShowNotification("Create Anchors From Persisted Names Status", "There is no persisted anchors on anchor store");
				return;
			}
		}
		else
		{
			ShowNotification("Create Anchors From Persisted Names Status", "Could not enumerate persistent names from store");
			return;
		}

		// Do not create if there is already a persisted anchor with same name
		List<string> filtered_names_data = new List<string>(enumerated_names.data);

		foreach (var ar_anchor in _anchorManager.trackables)
		{
			IsarAnchor isar_anchor = ar_anchor.GetComponentInParent<IsarAnchor>();
			filtered_names_data.IndexOf(isar_anchor.PersistentName);
			int index = Array.IndexOf<string>(enumerated_names.data, isar_anchor.PersistentName);

			if ( index >= 0 )
			{
				filtered_names_data.Remove(isar_anchor.PersistentName);
			}
		}

		foreach (string anchor_name in filtered_names_data)
		{
			var result = await _subsystem.CreateAnchorFromPersistedName(anchor_name, timeout_ms);
			if (result.isSuccess)
			{
				Debug.Log("Anchor created from persisted name succesfully (name): " + anchor_name + " id: " + result.id);
				ARAnchor anchor = _anchorManager.GetAnchor(result.id);

				if (anchor != null)
				{
					anchor.GetComponentInParent<IsarAnchor>().IsPersisted = true;
					anchor.GetComponent<IsarAnchor>().PersistentName = anchor_name;
				}
			}
		}
	}
}
