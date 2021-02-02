using UnityEngine;

namespace HoloLight.Isar.Utils
{
	[RequireComponent(typeof(TextMesh))]
	public class FrameCounter : MonoBehaviour
	{
		private TextMesh _textMesh;

		void Start()
		{
			_textMesh = GetComponent<TextMesh>();
		}

		void Update()
		{
			int fps = Mathf.RoundToInt(1.0f / Time.smoothDeltaTime);
			_textMesh.text = $"S: {fps} fps (Frame {Time.frameCount})";
		}
	}
}