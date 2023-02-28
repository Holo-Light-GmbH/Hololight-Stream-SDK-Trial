using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace HoloLight.Isar.ARSubsystems
{
	enum ImageType : int
	{
		STATIC = 0,
		MOVING = 1
	}

	class IsarTrackedImage : IsarTrackable
	{
		private Guid _referenceImageGuid;
		public ImageType ImageType { get; private set; } = ImageType.MOVING;

		public IsarTrackedImage(Guid referenceImageGuid, int id) : base(id)
		{
			_referenceImageGuid = referenceImageGuid;

			Track = XRTrackedImage.defaultValue;
		}

		public void UpdateTrackedImageData(UpdateType type, Pose pose, float width, float height)
		{
			// If we haven't yet updated the data, discard this
			if (!HasChanges) UpdateType = type;

			Vector2 size = new Vector2(width, height);
			TrackingState state = UpdateType == UpdateType.Added || UpdateType == UpdateType.Updated ? 
				TrackingState.Tracking : TrackingState.None;

			Track = new XRTrackedImage(_trackableId, _referenceImageGuid, pose, size, state, GCHandle.ToIntPtr(GCHandle.Alloc(this)));

			HasChanges = true;
		}

		public void Remove()
		{
			UpdateType = UpdateType.Removed;
			HasChanges = true;
		}
	}
}