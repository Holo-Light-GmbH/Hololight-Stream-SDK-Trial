// Starts the default camera and assigns the texture to the current renderer
using UnityEngine;

public class WebCamTextureExample : MonoBehaviour
{
	private WebCamTexture webcamTexture = null;

	private int index = 0;

	private Renderer _renderer = null;

	void Start()
	{
		Application.RequestUserAuthorization(UserAuthorization.WebCam);
		var devices = WebCamTexture.devices;
		//var camera = devices.First(device => device.name == "ISAR Virtual Cam");

		if (devices.Length == 0) return;


		for (int i = 0; i < devices.Length; i++)
		{
			Debug.Log(devices[i].name);
			if (devices[i].name.Contains("ISAR"))
			// "Device not found" when pressing Play
			//if (webCamDevice.name.Contains("Droid"))
			{
				index = i;
				//break;
			}
		}

		webcamTexture = new WebCamTexture(devices[index].name);
		_renderer = GetComponent<Renderer>();
		_renderer.material.mainTexture = webcamTexture;
		webcamTexture.Play();
	}

	[ContextMenu("Play")]
	void Play()
	{
		webcamTexture.Play();
	}

	[ContextMenu("Stop")]
	void Stop()
	{
		webcamTexture.Stop();
	}

	[ContextMenu("Next")]
	void Next()
	{
		webcamTexture.Stop();
		var devices = WebCamTexture.devices;
		index += 1;
		index %= devices.Length;
		webcamTexture = new WebCamTexture(devices[index].name);
		_renderer.material.mainTexture = webcamTexture;
		webcamTexture.Play();
	}

}