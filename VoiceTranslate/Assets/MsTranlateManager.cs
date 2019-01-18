using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Translation;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MsTranlateManager : MonoBehaviour
{
    private AudioSource mAudioSource;
    private TranslationRecognizer recognizer;
    private SpeechTranslationConfig config;
    public InputField mResult;
    public InputField mResultTTS;
    public Text textResult;


    private string subscriptionKey = "2a5951a29d164691af4ff0f05eb7457a";
    private bool started;
    private string voice;
    private Dictionary<string, string> voiceMap;
    /// <summary>
    /// Gets or sets Subscription Key
    /// </summary>
    public string SubscriptionKey
    {
        get
        {
            return this.subscriptionKey;
        }

        set
        {
            this.subscriptionKey = value?.Trim();
            
        }
    }

    /// <summary>
    /// Gets or sets Region
    /// </summary>
    public string Region { get; set; }

    /// <summary>
    /// Gets or sets From Language
    /// </summary>
    public string FromLanguage { get; set; }

    /// <summary>
    /// Gets or sets To Languages
    /// </summary>
    public List<string> ToLanguages { get; set; }

    /// <summary>
    /// Gets or sets Voice Parameter's Language
    /// </summary>
    public string VoiceLanguage { get; set; }

    private void CreateRecognizer()
    {
        //Microsoft.CognitiveServices.Speech.Internal.carbon_csharp a = new car();
        
        this.config = SpeechTranslationConfig.FromSubscription(SubscriptionKey, Region);
        this.config.SpeechRecognitionLanguage = FromLanguage;
        this.config.VoiceName = voice;
        ToLanguages.ForEach(l => this.config.AddTargetLanguage(l));

        this.recognizer = new TranslationRecognizer(this.config);

        this.recognizer.Recognizing += this.OnRecognizingEventHandler;
        this.recognizer.Recognized += this.OnRecognizedEventHandler;
        this.recognizer.Synthesizing += this.OnSynthesizingEventHandler;
        this.recognizer.Canceled += this.OnCanceledEventHandler;
    }

    #region Recognition Event Handlers

    /// <summary>
    /// Called when a partial response is received.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="TranslationRecognitionEventArgs"/> instance containing the event data.</param>
    private void OnRecognizingEventHandler(object sender, TranslationRecognitionEventArgs e)
    {
        //string text = e.Result.Text;
        //foreach (var t in e.Result.Translations)
        //{
        //    text += $"\nSame in {t.Key}: {t.Value}";
        //}

        //this.SetCurrentText(this.crisCurrentText, text);
    }

    /// <summary>
    /// Called on final response.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="TranslationRecognitionEventArgs"/> instance containing the event data.</param>
    private void OnRecognizedEventHandler(object sender, TranslationRecognitionEventArgs e)
    {
        //if (e.Result.Text.Length == 0)
        //{
        //    this.WriteLine(this.crisLogText, "Reason: " + e.Result.Reason);
        //    this.WriteLine(this.crisLogText, "No phrase response is available.");
        //}
        //else
        //{
        //    string text = e.Result.Text;
        //    foreach (var t in e.Result.Translations)
        //    {
        //        text += $"\nSame in {t.Key}: {t.Value}";
        //    }

        //    this.SetCurrentText(this.crisCurrentText, text);
        //    text += "\n";
        //    this.WriteLine(this.crisLogText, text);
        //}

        orignalText = e.Result.Text;
        foreach (var t in e.Result.Translations)
        {
            resultText = t.Value;
            break;
            //text = $"\nSame in {t.Key}: {t.Value}";
        }

        //resultText = e.Result.Translations[e.Result.Translations];
        Debug.Log(e.Result.Text);

    }

    /// <summary>
    /// Called when translation is canceled.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="TranslationRecognitionCanceledEventArgs"/> instance containing the event data.</param>
    private void OnCanceledEventHandler(object sender, TranslationRecognitionCanceledEventArgs e)
    {
        //string text = $"Speech recognition: canceled. Reason: {e.Reason}, ErrorDetails: {e.ErrorDetails}";
        //this.SetCurrentText(this.crisCurrentText, text);
        //text += "\n";
        //this.WriteLine(this.crisLogText, text);
        //if (this.started)
        //{
        //    this.recognizer.StopContinuousRecognitionAsync().Wait();
        //    this.started = false;
        //}
    }

    /// <summary>
    /// Called on availability of synthesized data.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="TranslationSynthesisEventArgs"/> instance containing the event data.</param>
    private void OnSynthesizingEventHandler(object sender, TranslationSynthesisEventArgs e)
    {
        
        var Clip = e.Result.GetAudio();
        if (Clip.Length > 0)
        {
            Audios.Enqueue(Clip);
            lock (syncRoot)
            {
                isAudioNew = true;
            }
        }
        
        
        

        //var audio = e.Result.GetAudio();
        //if (audio.Length > 0)
        //{
        //    using (var m = new MemoryStream(audio))
        //    {
        //        SoundPlayer simpleSound = new SoundPlayer(m);
        //        simpleSound.Play();
        //    }
        //}
    }
    bool isAudioNew = false;
    Queue<byte[]> Audios = new Queue<byte[]>();
    byte[] currentClip = null;
    string resultText;
    string orignalText;
    private object syncRoot = new object();
    private void Update()
    {
        if (resultText != null)
        {
            textResult.text = orignalText;
            mResult.text = resultText;
            mResultTTS.text = resultText;
        }
            
        if (isAudioNew&& Audios.Count > 0)
        {
            
            StartCoroutine(PlayAudio());
            
            
        }
    }

    private IEnumerator PlayAudio()
    {
        mAudioSource.clip = WavUtility.ToAudioClip(Audios.Dequeue());
        float time = mAudioSource.clip.length;
        mAudioSource.Play();
        while (time > 0)
        {
            yield return null;
            time -= Time.deltaTime;
        }        
        isAudioNew = false;
    }

    private void Start()
    {
        InitializeVoiceMap();
        mAudioSource = GetComponent<AudioSource>();

    }
    private void InitializeVoiceMap()
    {
        voiceMap = new Dictionary<string, string>();
        voiceMap.Add("en-US", "Microsoft Server Speech Text to Speech Voice (en-US, JessaRUS)");
        voiceMap.Add("ar-EG", "Microsoft Server Speech Text to Speech Voice (ar-EG, Hoda)");
        voiceMap.Add("zh-CN", "Microsoft Server Speech Text to Speech Voice (zh-CN, Yaoyao, Apollo)");
        voiceMap.Add("fr-FR", "Microsoft Server Speech Text to Speech Voice (fr-FR, Julie, Apollo)");
        voiceMap.Add("de-DE", "Microsoft Server Speech Text to Speech Voice (de-DE, Hedda)");
        voiceMap.Add("it-IT", "Microsoft Server Speech Text to Speech Voice (it-IT, Cosimo, Apollo)");
        voiceMap.Add("ja-JP", "Microsoft Server Speech Text to Speech Voice (ja-JP, Ayumi, Apollo)");
        voiceMap.Add("pt-BR", "Microsoft Server Speech Text to Speech Voice (pt-BR, Daniel, Apollo)");
        voiceMap.Add("ru-RU", "Microsoft Server Speech Text to Speech Voice (ru-RU, Irina, Apollo)");
        voiceMap.Add("es-ES", "Microsoft Server Speech Text to Speech Voice (es-ES, Laura, Apollo)");
    }
    #endregion

    private void OnDestroy()
    {
        if (this.config != null)
        {
            this.recognizer.Dispose();
            this.config = null;
        }
    }

    public async void StartTranslate(string region,string fromLang,string voiceLanguage,string[] toLang)
    {
        this.Region = region;
        this.FromLanguage = fromLang;
        this.ToLanguages = new List<string>();
        this.VoiceLanguage = voiceLanguage;

        this.voice = voiceMap[this.VoiceLanguage];
        this.ToLanguages.Add(this.VoiceLanguage);
        foreach (string selectedLanguage in toLang)
        {            
            if (!this.ToLanguages.Contains(selectedLanguage))
            {
                this.ToLanguages.Add(selectedLanguage);
            }
        }
        if (!this.started)
        {
            this.started = true;
            //this.LogRecognitionStart(this.crisLogText, this.crisCurrentText);
            this.CreateRecognizer();
            await Task.Run(async () => { await this.recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false); });
        }

        
    }

    public async void StopTranslate()
    {
        if (this.started)
        {
            if (recognizer == null)
                CreateRecognizer();
            await Task.Run(async () => { await this.recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false); });
            this.started = false;
        }
    }
}
