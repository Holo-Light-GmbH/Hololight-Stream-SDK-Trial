using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class IsarAnchorGenerator : MonoBehaviour
{
	public GameObject AnchorPrefab;

	public void CreateAnchor()
	{
		var gameObject = Instantiate(AnchorPrefab, this.transform.position, this.transform.rotation);
		var anchor = gameObject.AddComponent<ARAnchor>();

		if (anchor != null)
		{
			Debug.Log("Anchor is created with attributes: " + anchor.trackableId.subId1);
		}
	}

}