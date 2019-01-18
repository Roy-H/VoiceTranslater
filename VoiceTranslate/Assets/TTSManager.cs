using CognitiveServicesTTS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using UnityEngine;

public class TTSManager : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //OAuth();
    }
    private void OnDestroy()
    {
        //cortana.OnAudioAvailable -= PlayAudio;
        //cortana.OnError -= ErrorHandler;
    }

    private void OAuth()
    {
        auth = new Authentication("https://eastasia.api.cognitive.microsoft.com/sts/v1.0/issueToken", "2a5951a29d164691af4ff0f05eb7457a");
        
        try
        {
            accessToken = auth.GetAccessToken();
            Debug.Log(string.Format("Token: {0}\n", accessToken));
        }
        catch (Exception ex)
        {
            Debug.Log("Failed authentication.");
            Debug.Log(ex.ToString());
            Debug.Log(ex.Message);
            return;
        }
        //cortana = new Synthesize();
        //cortana.OnAudioAvailable += PlayAudio;
        //cortana.OnError += ErrorHandler;
    }

    Synthesize cortana;
    Authentication auth;
    string accessToken;
    string requestUri = "https://eastasia.tts.speech.microsoft.com/cognitiveservices/v1";

    public static void Speak(string text,string toLang)
    {
        string accessToken;

        // Issue token uri for old Bing Speech API "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
        // Issue token uri for new unified SpeechService API "https://westus.api.cognitive.microsoft.com/sts/v1.0/issueToken". 
        // Note: new unified SpeechService API key and issue token uri is per region

        // The way to get api key:
        // Unified Speech Service key
        // Free: https://azure.microsoft.com/en-us/try/cognitive-services/?api=speech-services
        // Paid: https://go.microsoft.com/fwlink/?LinkId=872236&clcid=0x409 
        Authentication auth = new Authentication("https://eastasia.api.cognitive.microsoft.com/sts/v1.0/issueToken", "2a5951a29d164691af4ff0f05eb7457a");

        try
        {
            accessToken = auth.GetAccessToken();
            Console.WriteLine("Token: {0}\n", accessToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed authentication.");
            Console.WriteLine(ex.ToString());
            Console.WriteLine(ex.Message);
            return;
        }

        Console.WriteLine("Starting TTSSample request code execution.");
        // For Unified SpeechService API: https://eastasia.tts.speech.microsoft.com/cognitiveservices/v1
        // Note: new unified SpeechService API synthesis endpoint is per region, choose the region close to your service to minimize the latency
        string requestUri = "https://eastasia.tts.speech.microsoft.com/cognitiveservices/v1";
        var cortana = new Synthesize();

        cortana.OnAudioAvailable += PlayAudio;
        cortana.OnError += ErrorHandler;

        // Reuse Synthesize object to minimize latency
        cortana.Speak(CancellationToken.None, new Synthesize.InputOptions()
        {
            RequestUri = new Uri(requestUri),
            // Text to be spoken.
            Text = text,
            VoiceType = Gender.Female,
            // Refer to the documentation for complete list of supported locales.
            //Locale = "en-US",
            Locale = toLang,
            // You can also customize the output voice. Refer to the documentation to view the different
            // voices that the TTS service can output.
            // VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, Jessa24KRUS)",
            //VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, Guy24KRUS)",
            //VoiceName = "Microsoft Server Speech Text to Speech Voice (zh-CN, Yaoyao, Apollo)",
            VoiceName = "Microsoft Server Speech Text to Speech Voice (ja-JP, Ayumi, Apollo)",
            // VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)",

            // Service can return audio in different output format.
            OutputFormat = AudioOutputFormat.Riff24Khz16BitMonoPcm,
            AuthorizationToken = "Bearer " + accessToken,
        }).Wait();
    }

    public void Speak()
    {
        // Reuse Synthesize object to minimize latency
        cortana.Speak(CancellationToken.None, new Synthesize.InputOptions()
        {
            RequestUri = new Uri(requestUri),
            // Text to be spoken.
            Text = "Hello",
            VoiceType = Gender.Female,
            // Refer to the documentation for complete list of supported locales.
            Locale = "en-US",
            // You can also customize the output voice. Refer to the documentation to view the different
            // voices that the TTS service can output.
            // VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, Jessa24KRUS)",
            //VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, Guy24KRUS)",
            //VoiceName = "Microsoft Server Speech Text to Speech Voice (zh-CN, Yaoyao, Apollo)",
            VoiceName = "Microsoft Server Speech Text to Speech Voice (ja-JP, Ayumi, Apollo)",
            // VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)",

            // Service can return audio in different output format.
            OutputFormat = AudioOutputFormat.Riff24Khz16BitMonoPcm,
            AuthorizationToken = "Bearer " + accessToken,
        }).Wait();

    }

    private static void PlayAudio(object sender, GenericEventArgs<Stream> args)
    {
        Console.WriteLine(args.EventData);

        // For SoundPlayer to be able to play the wav file, it has to be encoded in PCM.
        // Use output audio format AudioOutputFormat.Riff16Khz16BitMonoPcm to do that.
        SoundPlayer player = new SoundPlayer(args.EventData);

        player.PlaySync();
        //if (args.EventData.Length > 0)
        //{
        //    audioData = args.EventData;
        //    isNewAudio = true;
        //}
        
    }
    bool isPlaying = false;
    bool isNewAudio;
    Stream audioData;
    private void Update()
    {
        //if (isNewAudio&&!isPlaying)
        //{
        //    StartCoroutine(Play(audioData));
        //}           
    }

    private IEnumerator Play(Stream audioData)
    {
        if (isPlaying)
        {
            yield break;
        }
        isPlaying = true;

        audioSource.Play();
        var audio = StreamToBytes(audioData);
        if (audio == null)
        {
            isNewAudio = false;
            isPlaying = false;
            yield break;
        }
            
        var clip = WavUtility.ToAudioClip(audio);
        float time = clip.length;
        while (time > 0)
        {
            yield return null;
            time -= Time.deltaTime;
        }
        audioData.Dispose();
        audioData = null;
        isNewAudio = false;
        isPlaying = false;
    }

    public byte[] StreamToBytes(Stream stream)
    {
        if (stream.Length <= 0)
            return null;
        byte[] bytes = new byte[stream.Length];

        stream.Read(bytes, 0, bytes.Length);

        // 设置当前流的位置为流的开始 

        stream.Seek(0, SeekOrigin.Begin);

        return bytes;

    }

    /// <summary>
    /// Handler an error when a TTS request failed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="GenericEventArgs{Exception}"/> instance containing the event data.</param>
    private static void ErrorHandler(object sender, GenericEventArgs<Exception> e)
    {
        Debug.Log("Unable to complete the TTS request: [{0}]"+ e.ToString());
    }

}
