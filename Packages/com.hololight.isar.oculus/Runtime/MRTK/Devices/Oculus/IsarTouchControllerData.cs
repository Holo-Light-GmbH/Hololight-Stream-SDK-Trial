
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;
using Newtonsoft.Json;
using UnityEngine;

namespace HoloLight.Isar.Runtime.MRTK
{

	[JsonObject]
	public class IsarTouchControllerDataParent
	{
		[JsonProperty("Left")]
		public IsarTouchControllerData Left { get; set; } = new IsarTouchControllerData(Handedness.Left);
		[JsonProperty("Right")]
		public IsarTouchControllerData Right { get; set; } = new IsarTouchControllerData(Handedness.Right);
	}

	[JsonObject]
	public class IsarTouchControllerData
	{
		[JsonProperty("IsTracked")]
		public bool IsTracked = false;

		public IsarTouchControllerData(Handedness handedness)
		{
			this.Handedness = handedness;
		}

		[JsonIgnore]
		public Handedness Handedness = Handedness.Left;

		/// <summary>
		/// Pose Data
		/// </summary>
		[JsonProperty("Pos")]
		public Vector3 Position = Vector3.zero;
		[JsonProperty("Rot")]
		public Quaternion Rotation = Quaternion.identity;

		[JsonIgnore]
		public MixedRealityPose PointerPose;

		/// <summary> 
		/// Joystick
		/// </summary>
		[JsonProperty("Joystick")]
		public Joystick JoystickData { get; set; } = new Joystick();


		[JsonProperty("GripTriggerPress")]
		public float GripTrigger = 0f;

		[JsonProperty("IndexTriggerPress")]
		public float IndexTrigger = 0f;

		[JsonProperty("FirstBtn")]
		public ControllerButton Primary { get; set; } = new ControllerButton();

		[JsonProperty("SecondBtn")]
		public ControllerButton Secondary { get; set; } = new ControllerButton();
	}


	[JsonObject]
	public class Joystick
	{
		[JsonProperty("Pos")]
		public Vector2 Position = Vector2.zero;
		[JsonProperty("press")]
		public bool Press = false;
		[JsonProperty("touch")]
		public bool Touch = false;
	}

	[JsonObject]
	public class ControllerButton
	{
		[JsonProperty("press")]
		public bool Press = false;
		[JsonProperty("touch")]
		public bool Touch = false;
	}
}