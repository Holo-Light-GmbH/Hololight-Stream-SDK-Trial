
using System;

namespace HoloLight.Isar.Native.Qr
{
	public struct QrIsSupportedEventArgs
	{
		public bool IsSupported;
	}

	public enum QrAccessStatus : int
	{
		DeniedBySystem = 0,
		NotDeclaredByApp = 1,
		DeniedByUser = 2,
		UserPromptRequired = 3,
		Allowed = 4,
	}

	public struct QrRequestAccessEventArgs
	{
		public QrAccessStatus Status;
	}

	//public struct QrGetListEventArgs
	//{
	//	public QrCodeList List;

	//}

// ref: winrt::Microsoft::MixedReality::QR::QRVersion
	public enum QrVersion : int {
		INVALID = 0,
		QR1 = 1,
		QR2 = 2,
		QR3 = 3,
		QR4 = 4,
		QR5 = 5,
		QR6 = 6,
		QR7 = 7,
		QR8 = 8,
		QR9 = 9,
		QR10 = 10,
		QR11 = 11,
		QR12 = 12,
		QR13 = 13,
		QR14 = 14,
		QR15 = 15,
		QR16 = 16,
		QR17 = 17,
		QR18 = 18,
		QR19 = 19,
		QR20 = 20,
		QR21 = 21,
		QR22 = 22,
		QR23 = 23,
		QR24 = 24,
		QR25 = 25,
		QR26 = 26,
		QR27 = 27,
		QR28 = 28,
		QR29 = 29,
		QR30 = 30,
		QR31 = 31,
		QR32 = 32,
		QR33 = 33,
		QR34 = 34,
		QR35 = 35,
		QR36 = 36,
		QR37 = 37,
		QR38 = 38,
		QR39 = 39,
		QR40 = 40,
		MICRO_QRM1 = 41,
		MICRO_QRM2 = 42,
		MICRO_QRM3 = 43,
		MICRO_QRM4 = 44,
	}

	public struct QrCode
	{
		public readonly Guid Id;
		//public readonly HlrGuid spatialGraphNodeId;
		public readonly Int64 Timestamp;
		public readonly Int64 SysTimestamp;
		public readonly float PhysicalSideLength;
		public readonly QrVersion Version;
		public readonly UInt32 DataSize;
		public readonly UIntPtr Data;
		//public byte* Data; // unsafe/fixed | ref: https://stackoverflow.com/questions/8268625/get-pointer-on-byte-array-from-unmanaged-c-dll-in-c-sharp
		//public byte[] Data { get; internal set; }
		//private byte[] data;
		//public byte[] Data
		//{
		//	readonly get
		//	{
		//		return data;
		//	}
		//	internal set
		//	{
		//		data = value;
		//	}
		//}
		public readonly HlrPose Pose;
	}

	public /*ref*/ struct QrAddedEventArgs
	{
		public readonly QrCode Code;
	}

	public /*ref*/ struct QrUpdatedEventArgs
	{
		public readonly QrCode Code;
	}

	public /*ref*/ struct QrRemovedEventArgs
	{
		public readonly QrCode Code;
	}

	//public struct QrEnumerationCompletedEventArgs
	//{

	//}
}
