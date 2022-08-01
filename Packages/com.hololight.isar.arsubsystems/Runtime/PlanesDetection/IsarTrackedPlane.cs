using System;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace HoloLight.Isar.ARSubsystems
{
	class IsarTrackedPlane : IsarTrackable
	{
		public Vector2[] Polygon { get; private set; }

		public IsarTrackedPlane(int id) : base(id)
		{
			Track = BoundedPlane.defaultValue;
		}

		public void UpdateTrackedPlane(UpdateType type, TrackableId subsmumedBy, Pose pose, Vector2 center, Vector2 size, PlaneAlignment alignment, TrackingState state, Vector2[] polygon, PlaneClassification classification = PlaneClassification.None)
		{
			// If we haven't yet updated the data, discard this
			if(!HasChanges) UpdateType = type;

			Track = new BoundedPlane(_trackableId, subsmumedBy, pose, center, size, alignment, state, IntPtr.Zero, classification);

			Polygon = polygon;

			HasChanges = true;
		}

		public void Remove()
		{
			UpdateType = UpdateType.Removed;
			HasChanges = true;
		}
	}
}