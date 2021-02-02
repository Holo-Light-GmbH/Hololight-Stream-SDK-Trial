/*
 * Copyright 2019 Holo-Light GmbH. All Rights Reserved.
 */

namespace HoloLight.Isar.Native.Input
{

	public enum HandJointKind : int
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

	public enum JointPoseAccuracy
	{
		High = 0,
		Approximate = 1,
	}

	// TODO: encode Accuracy (0/1) into radius sign bit (can't have negative radius)
	public struct JointPose
	{
		public Quaternion Orientation; // 16 bytes
		public Vector3 Position;
		public float Radius;
		public JointPoseAccuracy Accuracy;

		public JointPose(Quaternion orientation, Vector3 position, float radius, JointPoseAccuracy accuracy)
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
		private JointPose Palm;
		private JointPose Wrist;
		private JointPose ThumbMetacarpal;
		private JointPose ThumbProximal;
		private JointPose ThumbDistal;
		private JointPose ThumbTip;
		private JointPose IndexMetacarpal;
		private JointPose IndexProximal;
		private JointPose IndexIntermediate;
		private JointPose IndexDistal;
		private JointPose IndexTip;
		private JointPose MiddleMetacarpal;
		private JointPose MiddleProximal;
		private JointPose MiddleIntermediate;
		private JointPose MiddleDistal;
		private JointPose MiddleTip;
		private JointPose RingMetacarpal;
		private JointPose RingProximal;
		private JointPose RingIntermediate;
		private JointPose RingDistal;
		private JointPose RingTip;
		private JointPose LittleMetacarpal;
		private JointPose LittleProximal;
		private JointPose LittleIntermediate;
		private JointPose LittleDistal;
		private JointPose LittleTip;

		public int Length
		{
			get { return (int)HandJointKind.Count; }
		}

		[System.Runtime.CompilerServices.IndexerName("data")]
		public JointPose this[int index]
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
	public struct HandPose
	{
		// TODO: it would be really nice if this would work
		//[MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)HandJointKind.Count/*, ArraySubType = UnmanagedType.?*/)]
		//public JointPose[] JointPoses; // 100 bytes

		//public fixed JointPose JointPoses[(int)HandJointKind.Count]; // 100 bytes

		public JointPoseArray JointPoses;

		public bool TryGetJoint( /*coordsystem, */ in HandJointKind jointIndex, out JointPose jointPose)
		{
			jointPose = JointPoses[(int)jointIndex];
			return true;
		}

		public bool TryGetJoints( /*coordsystem, */ HandJointKind[] jointIndices, JointPose[] jointPoses)
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