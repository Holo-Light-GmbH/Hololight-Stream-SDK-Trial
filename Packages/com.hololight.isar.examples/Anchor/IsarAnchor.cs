using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class IsarAnchor : MonoBehaviour
{

	bool isPersistedStateLastUpdate;
	public Material PersistedMaterial;
	public Material OriginalMaterial;
	public GameObject InfoUI;

	public string PersistentName { get; set; }
	public bool IsPersisted { get; set; }

	void Start()
    {
		IsPersisted = false;
		PersistentName = "";
		isPersistedStateLastUpdate = false;
		GetComponent<MeshRenderer>().material = OriginalMaterial;
	}

	private string CreateUIText()
	{
		ARAnchor anchor = GetComponent<ARAnchor>();

		string info = "Anchor ID: " + anchor.trackableId.subId1 + 
					  "\nPersisted: " + IsPersisted + "\n";

		info += "Persisted Name: " + PersistentName + "\n";

		info += "Transform:\n" +
				 "  Position: " + GetComponent<RectTransform>().transform.position.ToString("F4") + "\n" +
				 "  Rotation: " + GetComponent<RectTransform>().transform.rotation.ToString("F4");

		return info;
	}

	void SetVisible(bool isVisible)
	{
		Component[] a = gameObject.GetComponentsInChildren(typeof(Renderer));
		foreach (Component b in a)
		{
			Renderer c = (Renderer)b;
			c.enabled = isVisible;
		}
	}

	// Update is called once per frame
	void Update()
    {
		ARAnchor anchor = GetComponent<ARAnchor>();

		// Adjust visibility of anchor if the anchor is not tracked
		SetVisible(anchor.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking);

		// Change material according to persistence
		if (isPersistedStateLastUpdate != IsPersisted)
		{
			if (IsPersisted)
				GetComponent<MeshRenderer>().material = PersistedMaterial;
			else
				GetComponent<MeshRenderer>().material = OriginalMaterial;
		}

		transform.Find("PersistButton").gameObject.SetActive(!IsPersisted);
		transform.Find("UnpersistButton").gameObject.SetActive(IsPersisted);

		InfoUI.GetComponent<TMPro.TextMeshPro>().text = CreateUIText();
		isPersistedStateLastUpdate = IsPersisted;
	}
}
