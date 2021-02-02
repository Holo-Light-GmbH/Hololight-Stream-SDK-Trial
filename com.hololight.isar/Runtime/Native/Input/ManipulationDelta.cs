/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

namespace HoloLight.Isar.Native.Input
{
	public /*readonly*/ struct ManipulationDelta
	{
		public /*readonly*/ Vector3 Translation;

		public ManipulationDelta(Vector3 translation)
		{
			Translation = translation;
		}
	}
}