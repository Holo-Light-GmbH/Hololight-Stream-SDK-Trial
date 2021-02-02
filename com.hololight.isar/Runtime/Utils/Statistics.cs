using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace HoloLight.Isar.Utils
{
	// TODO: TextMesh doesn't really work well in VR without a background
	// or details to help the eye focus on the text
	// TODO: implement this as EditorWindow
	[RequireComponent(typeof(TextMesh))]
	public class Statistics : MonoBehaviour
	{
		internal TextMesh _textMesh;

		public class State
		{
			public static readonly int MAX_NUM_MEASUREMENTS = 2048;
			//public static double[] measurements1 = new double[MAX_NUM_MEASUREMENTS];
			public List<double> measurements = new List<double>(MAX_NUM_MEASUREMENTS);
			public int currentIndex = 0;
			public double duration = 0;
			public Stopwatch Timer = new Stopwatch();

			public void Safe()
			{
				duration = Timer.Elapsed.TotalMilliseconds;
				//duration = after - before;

				if (measurements.Count == MAX_NUM_MEASUREMENTS)
				{
					measurements[currentIndex] = duration;
				}
				else
				{
					measurements.Add(duration);
				}

				currentIndex++;
				if (currentIndex == MAX_NUM_MEASUREMENTS)
				{
					currentIndex = 0;
				}
				//if (count < MAX_NUM_MEASUREMENTS)
				//{
				//	count++;
				//}
			}

			public double avg = 0, median = 0, slowest = 0, fastest = 0, mode = 0;
			public void Update()
			{
				int count = measurements.Count();

				if (count > 0)
				{
					int halfIndex = count / 2;
					var sortedMeasurements = measurements.OrderBy(n => n);
					if ((count & 1) == 0) // even
					{
						median = (sortedMeasurements.ElementAt(halfIndex) +
						          sortedMeasurements.ElementAt(halfIndex - 1)) / 2;
					}
					else // odd
					{
						median = sortedMeasurements.ElementAt(halfIndex);
					}

					fastest = sortedMeasurements.First();
					slowest = sortedMeasurements.Last();

					avg = sortedMeasurements.Average(); // LINQ

					mode = measurements.GroupBy(n => n).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
				}
			}

		}
		public static State A = new State();
		public static State B = new State();

		// Start is called before the first frame update
		void Start()
		{
			_textMesh = GetComponent<TextMesh>();
		}

		// Update is called once per frame
		void Update()
		{
			//Debug.Log($"Matrix to Quaternion took {MathUtils.duration.ToString("F6")} seconds (Avg: {MathUtils.Avg().ToString("F6")})");

			//var measurements = MathUtils.measurements;

			A.Update();
			B.Update();

			_textMesh.text = $"Current: {A.duration:F6} | {B.duration:F6} \n" +
			                 $"N:       {A.measurements.Count()} | {B.measurements.Count()} / {State.MAX_NUM_MEASUREMENTS}\n" +
			                 $"Avg:     {A.avg:F6} | {B.avg:F6}\n" +
			                 $"Slowest: {A.slowest:F6} | {B.slowest:F6}\n" +
			                 $"Median:  {A.median:F6} | {B.median:F6}\n" +
			                 $"Fastest: {A.fastest:F6} | {B.fastest:F6}\n" +
			                 $"Mode:    {A.mode:F6} | {B.mode:F6}";
		}
	}
}
