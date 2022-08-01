using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.ARSubsystems;


namespace HoloLight.Isar.ARSubsystems
{
	class IsarImageDatabase : RuntimeReferenceImageLibrary
	{
		private static int imageIdCount = 0;

		public Dictionary<int, XRReferenceImage> TrackedImages { get; private set; } = new Dictionary<int, XRReferenceImage>();

		public IsarImageDatabase(XRReferenceImageLibrary library)
		{
			foreach(var referenceImage in library)
			{
				TrackedImages[imageIdCount++] =  referenceImage;
			}
		}

		public override int count => TrackedImages.Count;

		protected override XRReferenceImage GetReferenceImageAt(int index)
		{
			if (count < index) return new XRReferenceImage();

			var image = TrackedImages.Values.ElementAt(index);
			if (image != null) return image;
			else return new XRReferenceImage();
		}
	}
}
