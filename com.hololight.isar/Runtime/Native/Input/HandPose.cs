/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

namespace HoloLight.Isar.Native.Input
{

	public enum HlrHandJointKind : int
	{
		Palm = 0,
		Wrist = 1,
		ThumbMetacarpal = 2,
		ThumbProximal = 3,
		ThumbDistal = 4,
		ThumbTip = 5,
		IndexMetacarpal = 6,
		IndexProximal = 7,
		IndexIntermediate = 8,
		IndexDistal = 9,
		IndexTip = 10,
		MiddleMetacarpal = 11,
		MiddleProximal = 12,
		MiddleIntermediate = 13,
		MiddleDistal = 14,
		MiddleTip = 15,
		RingMetacarpal = 16,
		RingProximal = 17,
		RingIntermediate = 18,
		RingDistal = 19,
		RingTip = 20,
		LittleMetacarpal = 21,
		LittleProximal = 22,
		LittleIntermediate = 23,
		LittleDistal = 24,
		LittleTip = 25,
		Count
	}

	public enum HlrJointPoseAccuracy
	{
		High = 0,
		Approximate = 1,
	}

	// TODO: encode Accuracy (0/1) into radius sign bit (can't have negative radius)
	public struct HlrJointPose
	{
		public HlrQuaternion Orientation; // 16 bytes
		public HlrVector3 Position;
		public float Radius;
		public HlrJointPoseAccuracy Accuracy;

		public HlrJointPose(HlrQuaternion orientation, HlrVector3 position, float radius, HlrJointPoseAccuracy accuracy)
		{
			Orientation = orientation;
			Position = position;
			Radius = radius;
			Accuracy = accuracy;
		}
	}

	// TODO: https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html
	// NativeArray has no marshalling cost
	public struct JointPoseArray
	{
		private HlrJointPose Palm;
		private HlrJointPose Wrist;
		private HlrJointPose ThumbMetacarpal;
		private HlrJointPose ThumbProximal;
		private HlrJointPose ThumbDistal;
		private HlrJointPose ThumbTip;
		private HlrJointPose IndexMetacarpal;
		private HlrJointPose IndexProximal;
		private HlrJointPose IndexIntermediate;
		private HlrJointPose IndexDistal;
		private HlrJointPose IndexTip;
		private HlrJointPose MiddleMetacarpal;
		private HlrJointPose MiddleProximal;
		private HlrJointPose MiddleIntermediate;
		private HlrJointPose MiddleDistal;
		private HlrJointPose MiddleTip;
		private HlrJointPose RingMetacarpal;
		private HlrJointPose RingProximal;
		private HlrJointPose RingIntermediate;
		private HlrJointPose RingDistal;
		private HlrJointPose RingTip;
		private HlrJointPose LittleMetacarpal;
		private HlrJointPose LittleProximal;
		private HlrJointPose LittleIntermediate;
		private HlrJointPose LittleDistal;
		private HlrJointPose LittleTip;

		public int Length
		{
			get { return (int)HlrHandJointKind.Count; }
		}

		[System.Runtime.CompilerServices.IndexerName("data")]
		public HlrJointPose this[int index]
		{
		  get
		  {
			switch (index)
			{
			  case  0: return this.Palm;
			  case  1: return this.Wrist;
			  case  2: return this.ThumbMetacarpal;
			  case  3: return this.ThumbProximal;
			  case  4: return this.ThumbDistal;
			  case  5: return this.ThumbTip;
			  case  6: return this.IndexMetacarpal;
			  case  7: return this.IndexProximal;
			  case  8: return this.IndexIntermediate;
			  case  9: return this.IndexDistal;
			  case 10: return this.IndexTip;
			  case 11: return this.MiddleMetacarpal;
			  case 12: return this.MiddleProximal;
			  case 13: return this.MiddleIntermediate;
			  case 14: return this.MiddleDistal;
			  case 15: return this.MiddleTip;
			  case 16: return this.RingMetacarpal;
			  case 17: return this.RingProximal;
			  case 18: return this.RingIntermediate;
			  case 19: return this.RingDistal;
			  case 20: return this.RingTip;
			  case 21: return this.LittleMetacarpal;
			  case 22: return this.LittleProximal;
			  case 23: return this.LittleIntermediate;
			  case 24: return this.LittleDistal;
			  case 25: return this.LittleTip;
			  default: throw new System.IndexOutOfRangeException("Invalid index!");
			}
		  }
		  set
		  {
			switch (index)
			{
			  case  0: this.Palm               = value; break;
			  case  1: this.Wrist              = value; break;
			  case  2: this.ThumbMetacarpal    = value; break;
			  case  3: this.ThumbProximal      = value; break;
			  case  4: this.ThumbDistal        = value; break;
			  case  5: this.ThumbTip           = value; break;
			  case  6: this.IndexMetacarpal    = value; break;
			  case  7: this.IndexProximal      = value; break;
			  case  8: this.IndexIntermediate  = value; break;
			  case  9: this.IndexDistal        = value; break;
			  case 10: this.IndexTip           = value; break;
			  case 11: this.MiddleMetacarpal   = value; break;
			  case 12: this.MiddleProximal     = value; break;
			  case 13: this.MiddleIntermediate = value; break;
			  case 14: this.MiddleDistal       = value; break;
			  case 15: this.MiddleTip          = value; break;
			  case 16: this.RingMetacarpal     = value; break;
			  case 17: this.RingProximal       = value; break;
			  case 18: this.RingIntermediate   = value; break;
			  case 19: this.RingDistal         = value; break;
			  case 20: this.RingTip            = value; break;
			  case 21: this.LittleMetacarpal   = value; break;
			  case 22: this.LittleProximal     = value; break;
			  case 23: this.LittleIntermediate = value; break;
			  case 24: this.LittleDistal       = value; break;
			  case 25: this.LittleTip          = value; break;
			  default: throw new System.IndexOutOfRangeException("Invalid index!");
			}
		  }
		}
	}

	// TODO: Array.Copy(glbData, stride * 7 + chunk0Length, gltfObject.buffers[0].BufferData, 0, chunk1Length);

	// NOTE: structs should be sequential by default, actually, so we might not need this.
	//[StructLayout(LayoutKind.Sequential)]
	public struct HlrHandPose
	{
		// TODO: it would be really nice if this would work
		//[MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)HandJointKind.Count/*, ArraySubType = UnmanagedType.?*/)]
		//public JointPose[] JointPoses; // 100 bytes

		//public fixed JointPose JointPoses[(int)HandJointKind.Count]; // 100 bytes

		public JointPoseArray JointPoses;

		public bool TryGetJoint( /*coordsystem, */ in HlrHandJointKind jointIndex, out HlrJointPose jointPose)
		{
			jointPose = JointPoses[(int)jointIndex];
			return true;
		}

		public bool TryGetJoints( /*coordsystem, */ HlrHandJointKind[] jointIndices, HlrJointPose[] jointPoses)
		{
			for (int i = 0; i < jointIndices.Length; i++)
			{
				var index = (int)jointIndices[i];
				jointPoses[i] = JointPoses[index];
			}
			return true;
		}
	}
}