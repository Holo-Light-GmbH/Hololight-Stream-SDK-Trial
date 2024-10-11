# Speech Recognition

Hololight Stream supports [Speech Recognition](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk2/features/input/speech?view=mrtkunity-2022-05) for Hololens 2 and provides the required Speech and Dictation Providers for MRTK. Speech Recognition can be used to interact with a scene using a list of keywords. Dictation Recognition can be used to input a continuous sentence to an object in the scene, for example an input field.

## Getting Started

Recognition providers are enabled on Hololight Stream MRTK by default. For getting started with MRTK, follow the steps in [MRTK Extension](mrtk2_extension.md)

### Manual Scene Configuration

To enable Speech and Dictation, it is required to exchange the **Windows Speech Input** and **Windows Dictation Input** in the MRTK providers with **Isar Speech Input** and **Isar Dictation Input** respectively.

### Example

An example on how to use Speech and Dictation can be found in the [MRTK Examples Package](https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk2/features/input/speech?view=mrtkunity-2022-05#example-scene). Guides on the respective examples can be followed and then migrated into Hololight Stream by applying the steps above.

After following the [MRTK Extension Scene Configuration Step](mrtk2_extension.md#scene-configuration), the MRTK configuration profile will be overwritten, changing the `SpeechCommandsProfile`. Before running the example, set the `Input -> Speech` configuration to `Speech.MixedRealitySpeechCommandsProfile`. This profile can also be changed with a custom profile.

## Usage without MRTK

Speech recognition features can also be used without MRTK. For this, IsarKeywordRecognizer and IsarDictationRecognizer classes can be used.

### Keyword Recognition

Example code below can be followed for using keyword recognition with IsarKeywordRecognizer:

```csharp
using UnityEngine;
using HoloLight.Isar.SpeechRecognition;

/// <summary>
/// Example for showing the usage of <see cref="IsarKeywordRecognizer"/>.
/// This example assumes the <see cref="Hololight.Stream.Runtime.MRTK.SpeechInputProvider"/>, <see cref="Hololight.Stream.Runtime.MRTK.DictationInputProvider"/> and <see cref="IsarDictationRecognizer"/> are not used.
/// </summary>
public class KeywordRecognizerExample : MonoBehaviour
{
	public string[] Keywords;

	IsarKeywordRecognizer _recognizer;

	private void Start()
	{
		_recognizer = new IsarKeywordRecognizer(Keywords);

		_recognizer.OnKeywordRecognized += Recognizer_OnKeywordRecognizer;
		_recognizer.Start();
		_recognizer.AddConstraints();
		_recognizer.StartRecognition();
	}

	private void Update()
	{
		_recognizer.Update();
	}

	private void OnDisable()
	{
		_recognizer.StopRecognition();
		_recognizer.OnKeywordRecognized -= Recognizer_OnKeywordRecognizer;
		_recognizer.Stop();
	}

	private void Recognizer_OnKeywordRecognizer(IsarSpeechEventData args, string text)
	{
		Debug.Log(args);
		Debug.Log(text);
	}
}
```

### Dictation Recognition

To use dictation recognition with IsarDictationRecognizer, the code below can be followed:

```csharp
using UnityEngine;
using HoloLight.Isar.SpeechRecognition;
using UnityEngine.Windows.Speech;
using System.Threading.Tasks;

/// <summary>
/// Example for showing the usage of <see cref="IsarDictationRecognizer"/>.
/// This example assumes the <see cref="Hololight.Stream.Runtime.MRTK.DictationInputProvider"/>, <see cref="Hololight.Stream.Runtime.MRTK.SpeechInputProvider"/> and <see cref="IsarKeywordRecognizer"/> are not used.
/// </summary>
public class DictationRecognizerExample : MonoBehaviour
{
	public IsarDictationTopic Topic = IsarDictationTopic.DICTATION;
	public ConfidenceLevel ConfidenceLevel = ConfidenceLevel.Medium;

	public float InitialSilenceTimeout = 5.0f;
	public float AutoSilenceTimeout = 20.0f;
	public float EndTimeout = 1.0f;

	IsarDictationRecognizer _recognizer;

	private bool _isTransitioning = false;
	private bool _isListening = false;

    // Waiting time for status check of the recognizer
	private const int STATUS_WAIT_MS = 100;

	private void Start()
	{
		_recognizer = new IsarDictationRecognizer(ConfidenceLevel, Topic);

		_recognizer.OnDictationHypothesis += Recognizer_OnHypothesis;
		_recognizer.OnDictationCompleted += Recognizer_OnCompleted;
		_recognizer.OnDictationResult += Recognizer_OnResult;
		_recognizer.OnDictationError += Recognizer_OnError;

		_recognizer.Start();
	}

	// Can be called from outside, like a button event
	public async void StartRecognition()
	{
		_isTransitioning = true;
		_isListening = true;

		_recognizer.AddConstraints();
		_recognizer.SetTimeouts(InitialSilenceTimeout, AutoSilenceTimeout, EndTimeout);
		_recognizer.StartRecognition();

		await WaitUntilStatus(_recognizer, IsarSpeechRecognizerStatus.RECOGNIZER_CAPTURING);
	}

	// Can be called from outside, like a button event. If not, it will automatically stop with timeouts
	public async void StopRecognition()
	{
		_isListening = false;
		_isTransitioning = true;

		_recognizer.StopRecognition();

		await WaitUntilStatus(_recognizer, IsarSpeechRecognizerStatus.DEFAULT);

		_isTransitioning = false;
	}

	private void Update()
	{
		_recognizer.Update();

		// If the recognition stops as a result of timing out, make sure to manually stop the dictation recognizer.
		if (!_isTransitioning && _isListening && _recognizer.Status == IsarSpeechRecognizerStatus.DEFAULT)
		{
			StopRecognition();
		}
	}

	private void OnDisable()
	{
		StopRecognition();

		_recognizer.OnDictationHypothesis -= Recognizer_OnHypothesis;
		_recognizer.OnDictationCompleted -= Recognizer_OnCompleted;
		_recognizer.OnDictationResult -= Recognizer_OnResult;
		_recognizer.OnDictationError -= Recognizer_OnError;

		_recognizer.Stop();
	}

	private void Recognizer_OnHypothesis(string hypothesis) => Debug.Log(hypothesis);
	private void Recognizer_OnResult(string text, ConfidenceLevel confidence) => Debug.Log(text);
	private void Recognizer_OnCompleted(DictationCompletionCause cause) => Debug.Log(cause);
	private void Recognizer_OnError(string error) => Debug.Log(error);

	// Task to wait for recognizers to reach wanted status
	private static async Task WaitUntilStatus(IsarSpeechRecognizer recognizer, IsarSpeechRecognizerStatus statusToCheck)
	{
		while (recognizer.Status != statusToCheck)
		{
			await Task.Delay(STATUS_WAIT_MS);
		}
	}
}
```

This code is designed to work only by itself, without keyword recognition. To use both recognition types at the same time, user should make sure to stop one before starting another. As the keyword recognition is done continuously and the dictation recognition is done with a timeout, manual stopping is only needed for keyword recognition. To check the status of the recognizers, Status property can be used. Default status is the stopped recognition.
