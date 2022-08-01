namespace HoloLight.Isar.Native.CustomMessage
{
	public enum MessageType
	{
		IMAGE_UPDATED = 1,
		TOUCH_DATA = 2,

		IMAGE_TO_TRACK = 3,
		IMAGE_ADDED = 4,
		IMAGE_REMOVED = 5,

		AR_RAYCAST_SCREEN_POINT = 6,
		AR_RAYCAST_RAY = 7,
		AR_RAYCAST_HIT_RESULTS = 8,
		AR_RAYCAST_ADD_CONTINIOUS_SCREEN_POINT = 9,     // Placeholder, not currently used
		AR_RAYCAST_ADD_CONTINIOUS_RAY = 10,             // Placeholder, not currently used
		AR_RAYCAST_REMOVE = 11,                         // Placeholder, not currently used

		AR_PLANE_DETECTION_CONFIG = 12,
		AR_PLANE_DETECTION_RESULTS = 13,

		CAMERA_ENABLE = 18,
		CAMERA_DISABLE = 19,
		CAMERA_METADATA = 20,
	}
}