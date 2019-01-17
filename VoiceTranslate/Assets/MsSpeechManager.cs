using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Threading.Tasks;
using System;
using UnityEngine.UI;

public class MsSpeechManager : MonoBehaviour
{
    public string Region { get; set; }

    public Text Result;
    public Dropdown LanguageSelection;
    public Dropdown ToLanguageSelection;

    /// <summary>
    /// Gets or sets recognition language
    /// </summary>
    public string RecognitionLanguage { get; set; }
    public bool IsFromFile { get; set; }

    private string subscriptionKey = "2a5951a29d164691af4ff0f05eb7457a";
    private string wavFileName= "D:/code/cognitive-services-speech-sdk-master/samples/csharp/dotnet-windows/speechtotext-wpf/speechtotext-wpf/audio/whatstheweatherlike.wav";
    private const string defaultLocale = "en-US";
    private string endpointId;    
    
    private string resultText;

    private TaskCompletionSource<int> stopBaseRecognitionTaskCompletionSource;

    private void Update()
    {
        Result.text = resultText;
    }


    private async Task CreateBaseReco()
    {
        this.Region = "eastasia";
        this.RecognitionLanguage = "en-US";
        var config = SpeechConfig.FromSubscription(subscriptionKey, this.Region);
        config.SpeechRecognitionLanguage = this.RecognitionLanguage;

        SpeechRecognizer basicRecognizer;

        if (IsFromFile)
        {
            using (var audioInput = AudioConfig.FromWavFileInput(wavFileName))
            {
                using (basicRecognizer = new SpeechRecognizer(config, audioInput))
                {
                    await this.RunRecognizer(basicRecognizer, RecoType.Base, stopBaseRecognitionTaskCompletionSource).ConfigureAwait(false);
                }
            }
        }
        else
        {
            using (basicRecognizer = new SpeechRecognizer(config))
            {
                await this.RunRecognizer(basicRecognizer, RecoType.Base, stopBaseRecognitionTaskCompletionSource).ConfigureAwait(false);
            }
        }

    }

    private async Task RunRecognizer(SpeechRecognizer recognizer, RecoType recoType, TaskCompletionSource<int> source)
    {
        //subscribe to events
        bool isChecked = false;
        

        EventHandler<SpeechRecognitionEventArgs> recognizingHandler = (sender, e) => RecognizedEventHandler(e, recoType);
        if (isChecked)
        {
            recognizer.Recognizing += recognizingHandler;
        }

        EventHandler<SpeechRecognitionEventArgs> recognizedHandler = (sender, e) => RecognizedEventHandler(e, recoType);
        EventHandler<SpeechRecognitionCanceledEventArgs> canceledHandler = (sender, e) => CanceledEventHandler(e, recoType, source);
        EventHandler<SessionEventArgs> sessionStartedHandler = (sender, e) => SessionStartedEventHandler(e, recoType);
        EventHandler<SessionEventArgs> sessionStoppedHandler = (sender, e) => SessionStoppedEventHandler(e, recoType, source);
        EventHandler<RecognitionEventArgs> speechStartDetectedHandler = (sender, e) => SpeechDetectedEventHandler(e, recoType, "start");
        EventHandler<RecognitionEventArgs> speechEndDetectedHandler = (sender, e) => SpeechDetectedEventHandler(e, recoType, "end");

        recognizer.Recognized += recognizedHandler;
        recognizer.Canceled += canceledHandler;
        recognizer.SessionStarted += sessionStartedHandler;
        recognizer.SessionStopped += sessionStoppedHandler;
        recognizer.SpeechStartDetected -= speechStartDetectedHandler;
        recognizer.SpeechEndDetected -= speechEndDetectedHandler;

        //start,wait,stop recognition
        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
        await source.Task.ConfigureAwait(false);
        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);       

        // unsubscribe from events
        if (isChecked)
        {
            recognizer.Recognizing -= recognizingHandler;
        }
        recognizer.Recognized -= recognizedHandler;
        recognizer.Canceled -= canceledHandler;
        recognizer.SessionStarted -= sessionStartedHandler;
        recognizer.SessionStopped -= sessionStoppedHandler;
        recognizer.SpeechStartDetected -= speechStartDetectedHandler;
        recognizer.SpeechEndDetected -= speechEndDetectedHandler;
    }

    private void SpeechDetectedEventHandler(RecognitionEventArgs e, RecoType rt, string eventType)
    {
        //var log = (rt == RecoType.Base) ? this.baseModelLogText : this.customModelLogText;
        //this.WriteLine(log, String.Format(CultureInfo.InvariantCulture, "Speech recognition: Speech {0} detected event: {1}.",
        //    eventType, e.ToString()));
        Debug.Log("SpeechDetectedEventHandler");
    }

    private void SessionStoppedEventHandler(SessionEventArgs e, RecoType rt, TaskCompletionSource<int> source)
    {
        //var log = (rt == RecoType.Base) ? this.baseModelLogText : this.customModelLogText;
        //this.WriteLine(log, String.Format(CultureInfo.InvariantCulture, "Speech recognition: Session stopped event: {0}.", e.ToString()));
        //source.TrySetResult(0);
        Debug.Log("SessionStoppedEventHandler");
    }

    private void SessionStartedEventHandler(SessionEventArgs e, RecoType rt)
    {
        Debug.Log("SessionStartedEventHandler");
        //var log = (rt == RecoType.Base) ? this.baseModelLogText : this.customModelLogText;
        //this.WriteLine(log, String.Format(CultureInfo.InvariantCulture, "Speech recognition: Session started event: {0}.", e.ToString()));
    }

    private void CanceledEventHandler(SpeechRecognitionCanceledEventArgs e, RecoType rt, TaskCompletionSource<int> source)
    {
        //var log = (rt == RecoType.Base) ? this.baseModelLogText : this.customModelLogText;
        //source.TrySetResult(0);
        //this.WriteLine(log, "--- recognition canceled ---");
        //this.WriteLine(log, $"CancellationReason: {e.Reason.ToString()}. ErrorDetails: {e.ErrorDetails}.");
        //this.WriteLine(log);
        Debug.Log("CanceledEventHandler");
    }

    private void RecognizedEventHandler(SpeechRecognitionEventArgs e, RecoType rt)
    {
        
        if (rt == RecoType.Base)
        {
            //log = this.baseModelLogText;
            //this.SetCurrentText(this.baseModelCurrentText, e.Result.Text);
        }
        else
        {
            //log = this.customModelLogText;
            //this.SetCurrentText(this.customModelCurrentText, e.Result.Text);
        }

        //this.WriteLine(log);
        //this.WriteLine(log, $" --- Final result received. Reason: {e.Result.Reason.ToString()}. --- ");
        //if (!string.IsNullOrEmpty(e.Result.Text))
        //{
        //    this.WriteLine(log, e.Result.Text);
        //}
        
        Debug.Log("RecognizedEventHandler"+ e.Result.Text);
        resultText = e.Result.Text;
    }

    enum RecoType
    {
        Base = 1,
        Custom = 2
    }

    public void StartDetect(string fileName)
    {    
        wavFileName = fileName;
        stopBaseRecognitionTaskCompletionSource = new TaskCompletionSource<int>();
        //this.Region = "eastasia";
        //this.RecognitionLanguage = "en-US";
        //var config = SpeechConfig.FromSubscription(subscriptionKey, this.Region);
        Task.Run(async () => { await CreateBaseReco().ConfigureAwait(false); });
    }

    public void StopDtect()
    {
        stopBaseRecognitionTaskCompletionSource.TrySetResult(0);
    }

    
}
