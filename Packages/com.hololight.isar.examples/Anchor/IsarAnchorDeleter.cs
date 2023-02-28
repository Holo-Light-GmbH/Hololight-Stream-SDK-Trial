using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.ARFoundation;

public class IsarAnchorDeleter : MonoBehaviour
{
	ARAnchor _anchor;

	private void Start()
	{
		_anchor = GetComponent<ARAnchor>();
	}

	public void DeleteAnchor()
	{
		_anchor.enabled = false;
	}
}
