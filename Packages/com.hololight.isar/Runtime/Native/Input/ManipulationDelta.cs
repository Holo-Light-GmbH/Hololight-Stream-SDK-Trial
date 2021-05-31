/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

namespace HoloLight.Isar.Native.Input
{
	public /*readonly*/ struct HlrSpatialManipulationDelta
	{
		public /*readonly*/ HlrVector3 Translation;

		public HlrSpatialManipulationDelta(HlrVector3 translation)
		{
			Translation = translation;
		}
	}
}