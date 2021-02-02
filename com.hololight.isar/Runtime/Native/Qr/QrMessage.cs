using System.Runtime.InteropServices;

namespace HoloLight.Isar.Native.Qr
{
	public enum QrMessageType : int
	{
		IsSupported,
		RequestAccess,
		GetList,
		Added,
		Updated,
		Removed,
		EnumerationCompleted,
		Min = IsSupported,
		Max = Min - 1
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct QrMessage
	{
		[FieldOffset(0)] internal QrMessageType Type;

		//union - https://stackoverflow.com/questions/126781/c-union-in-c-sharp
		[FieldOffset(8)] internal QrIsSupportedEventArgs IsSupportedEventArgs;
		[FieldOffset(8)] internal QrRequestAccessEventArgs RequestAccessEventArgs;
		//[FieldOffset(8)] internal QrGetListEventArgs GetListEventArgs;
		[FieldOffset(8)] internal QrAddedEventArgs AddedEventArgs;
		[FieldOffset(8)] internal QrUpdatedEventArgs UpdatedEventArgs;
		[FieldOffset(8)] internal QrRemovedEventArgs RemovedEventArgs;
		//[FieldOffset(8)] internal QrEnumerationCompletedEventArgs EnumerationCompletedEventArgs;
	}
}
