/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

using System;

namespace HoloLight.Isar.Native
{
	// <summary>
	// Represents a version(major, minor, patch) as packed uint
	// </summary>
	public struct Version
	{
		private const UInt32 INVALID_VERSION = 0;
		public readonly UInt32 PackedValue;

		public Version(UInt32 packedValue = INVALID_VERSION)
		{
			PackedValue = packedValue;
		}

		public Version(UInt32 major, UInt32 minor, UInt32 patch)
		{
			PackedValue = (major << 22) | (minor << 12) | patch;
		}

		public override int GetHashCode()
		{
			return (int) PackedValue;
		}

		public override bool Equals(object obj)
		{
			return obj is Version other && Equals(other);
		}

		public bool Equals(Version other)
		{
			return PackedValue == other.PackedValue;
		}

		public static bool operator ==(Version lhs, Version rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Version lhs, Version rhs)
		{
			return !lhs.Equals(rhs);
		}

		public static uint GetMajor(Version version)
		{
			uint major = version.PackedValue >> 22;
			return major;
		}

		public static uint GetMinor(Version version)
		{
			uint minor = (version.PackedValue >> 12) & 0x3ff;
			return minor;
		}

		public static uint GetPatch(Version version)
		{
			uint patch = version.PackedValue & 0xfff;
			return patch;
		}

		public static bool IsValid(Version version)
		{
			bool isValid = (version.PackedValue != INVALID_VERSION);
			return isValid;
		}

		public static System.Version ToSystemVersion(Version version)
		{
			int major = Convert.ToInt32(GetMajor(version));
			int minor = Convert.ToInt32(GetMinor(version));
			int patch = Convert.ToInt32(GetPatch(version));

			return new System.Version(major, minor, patch);
		}

		public override string ToString()
		{
			return $"{GetMajor(this)}.{GetMinor(this)}.{GetPatch(this)}";
		}
	}
}