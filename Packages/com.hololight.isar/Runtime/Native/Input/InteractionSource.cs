/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;

namespace HoloLight.Isar.Native.Input
{
	public enum HlrSpatialInteractionSourceKind : int
	{
		Other = 0,
		Hand = 1,
		Voice = 2,
		Controller = 3,
	}

	public enum HlrSpatialInteractionSourceHandedness : int
	{
		Unspecified = 0,
		Left = 1,
		Right = 2,
	}

	[Flags]
	public enum HlrSpatialInteractionSourceFlags : uint
	{
		None = 0,
		SupportsTouchpad = 1,
		SupportsThumbstick = 2,
		SupportsPointing = 4,
		SupportsGrasp = 8,
		SupportsMenu = 16, // 0x00000010
	}

	public struct HlrSpatialInteractionSource /* : IEquatable<InteractionSource>*/
	{
		public uint Id;
		public HlrSpatialInteractionSourceKind Kind;
		public HlrSpatialInteractionSourceHandedness Handedness;
		public HlrSpatialInteractionSourceFlags Flags;
		//public ushort VendorId;
		//public ushort ProductId;
		//public ushort ProductVersion;

		//public override bool Equals(object obj)
		//{
		//	InteractionSource? nullable = obj as InteractionSource?;
		//	return nullable.HasValue && this.Equals(nullable.Value);
		//}

		//public bool Equals(InteractionSource other)
		//{
		//	return (int)other.Id == (int)this.Id;
		//}

		//public override int GetHashCode()
		//{
		//	return (int)this.Id;
		//}

		public bool SupportsGrasp
		{
			get
			{
				return this.Flags.HasFlag(HlrSpatialInteractionSourceFlags.SupportsPointing);
				//return (this.Flags & InteractionSourceFlags.SupportsGrasp) != InteractionSourceFlags.None;
			}
		}

		public bool SupportsMenu
		{
			get
			{
				return this.Flags.HasFlag(HlrSpatialInteractionSourceFlags.SupportsPointing);
				//return (this.Flags & InteractionSourceFlags.SupportsMenu) != InteractionSourceFlags.None;
			}
		}

		public bool SupportsPointing
		{
			get
			{
				return this.Flags.HasFlag(HlrSpatialInteractionSourceFlags.SupportsPointing);
				//return (this.Flags & InteractionSourceFlags.SupportsPointing) != InteractionSourceFlags.None;
			}
		}

		public bool SupportsThumbstick
		{
			get
			{
				return this.Flags.HasFlag(HlrSpatialInteractionSourceFlags.SupportsPointing);
				//return (this.Flags & InteractionSourceFlags.SupportsThumbstick) != InteractionSourceFlags.None;
			}
		}

		public bool SupportsTouchpad
		{
			get
			{
				return this.Flags.HasFlag(HlrSpatialInteractionSourceFlags.SupportsPointing);
				//return (this.Flags & InteractionSourceFlags.SupportsTouchpad) != InteractionSourceFlags.None;
			}
		}

	}
}