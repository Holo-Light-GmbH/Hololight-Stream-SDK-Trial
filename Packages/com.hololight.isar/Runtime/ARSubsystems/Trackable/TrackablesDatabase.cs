using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.ARSubsystems;

namespace HoloLight.Isar.ARSubsystems
{
	internal static class TrackablesDatabase
	{
		private static List<IsarTrackable> _trackables = new List<IsarTrackable>();
		private static readonly object _lockObj = new object();

		public static int Count => _trackables.Count;

		public static void AddTrack(IsarTrackable track)
		{
			lock (_lockObj)
			{
				_trackables.Add(track);
			}
		}

		public static IEnumerable<IsarTrackable> GetAllTracks()
		{
			lock (_lockObj)
			{
				return _trackables;
			}
		}

		public static IEnumerable<T> GetTrackableOfTypes<T>() where T : IsarTrackable
		{
			lock (_lockObj)
			{
				return _trackables.Where((track) => track is T).Cast<T>();
			}
		}

		public static IsarTrackable GetTrack(int id)
		{
			lock (_lockObj)
			{
				return _trackables.FirstOrDefault((track) => track.IsarId == id);
			}
		}

		public static T GetTrack<T>(int id) where T : IsarTrackable
		{
			lock (_lockObj)
			{
				return GetTrack(id) as T;
			}
		}

		public static IsarTrackable GetTrack(TrackableId id)
		{
			lock (_lockObj)
			{
				return _trackables.FirstOrDefault((track) => track.Track.trackableId == id);
			}
		}

		public static T GetTrack<T>(TrackableId id) where T : IsarTrackable
		{
			lock (_lockObj)
			{
				return GetTrack(id) as T;
			}
		}

		public static int GetTrackableTypeCount<T>() where T : IsarTrackable
		{
			lock (_lockObj)
			{
				return _trackables.Count((track) => track is T);
			}
		}

		public static bool Contains(int isarId)
		{
			lock (_lockObj)
			{
				return _trackables.FirstOrDefault((track) => track.IsarId == isarId) != null;
			}
		}

		public static bool Contains(TrackableId id)
		{
			lock (_lockObj)
			{
				return _trackables.FirstOrDefault((track) => track.Track.trackableId == id) != null;
			}
		}

		public static void Remove(IsarTrackable trackable)
		{
			lock (_lockObj)
			{
				_trackables.Remove(trackable);
			}
		}

		public static void Remove(TrackableId trackableId)
		{
			lock (_lockObj)
			{
				var track = _trackables.FirstOrDefault((isarTrack) => isarTrack.Track.trackableId == trackableId);

				if (track != null) _trackables.Remove(track);
			}
		}
	}
}
