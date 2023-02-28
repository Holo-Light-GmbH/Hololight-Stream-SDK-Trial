using UnityEngine.XR.ARSubsystems;

namespace HoloLight.Isar.ARSubsystems
{
	internal class IsarXRPromise<T> : Promise<T>
	{
		protected override void OnKeepWaiting()
		{
			return;
		}

		internal new void Resolve(T result)
		{
			base.Resolve(result);
		}

		static int s_LastFrameUpdated;
	}
}

