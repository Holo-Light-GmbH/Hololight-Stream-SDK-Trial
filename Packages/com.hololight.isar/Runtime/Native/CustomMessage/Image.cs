using System;
using UnityEngine;

namespace HoloLight.Isar.Native.CustomMessage
{
	enum ImageType : int
	{
		STATIC = 0,
		MOVING = 1
	}

	class Image
	{
		public Sprite imageTexture;
		public int serverId;
		public ImageType type;
		public float width;


		public Image(Sprite imageTexture, int serverId, ImageType type, float width)
		{
			this.imageTexture = imageTexture;
			this.serverId = serverId;
			this.width = width;
			this.type = type;
		}
	}

	public class TrackedImage
	{
		public int id = 0;
		public string name;
		public GameObject gameObject;
		public Transform transform;
		public Vector3 position = new Vector3();
		public Quaternion rotation = new Quaternion();
		public float width = 0;
		public float height = 0;

		public TrackedImage(GameObject gameObject, int imageId)
		{
			this.gameObject = gameObject;
			this.transform = gameObject.transform;
			this.name = gameObject.name;
			this.id = imageId;
		}

		internal void UpdatePose()
		{
			this.gameObject.SetActive(true);
			this.transform.position = this.position;
			this.transform.rotation = this.rotation;
		}

		internal void UpdateScale()
		{
			this.transform.localScale = new Vector3(this.width, 0.01f, this.height);
		}
	}
}