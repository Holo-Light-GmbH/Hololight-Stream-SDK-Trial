using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.XR.ARSubsystems;

namespace HoloLight.Isar.ARSubsystems
{
	enum UpdateType
	{
		None = 0,
		Added,
		Updated,
		Removed
	}

	internal abstract class IsarTrackable
	{
		protected TrackableId _trackableId;

		public UpdateType UpdateType { get; protected set; } = UpdateType.None;
		public bool HasChanges { get; set; }
		public ITrackable Track { get; protected set; }
		public int IsarId { get; protected set; }

		public IsarTrackable(int id)
		{
			var newGuid = Guid.NewGuid();
			unsafe { _trackableId = *(TrackableId*)&newGuid; }

			IsarId = id;
		}
	}
}
