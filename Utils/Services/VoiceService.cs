// Services/VoiceService.cs
using System.Speech.Recognition;
using System.Speech.Synthesis;

public class VoiceService : IVoiceService
{
    private SpeechRecognitionEngine _recognizer;
    private SpeechSynthesizer _synthesizer;
    
    public event Action<string> OnSpeechRecognized;
    
    public VoiceService()
    {
        _recognizer = new SpeechRecognitionEngine();
        _synthesizer = new SpeechSynthesizer();
        
        // Konfiguracja rozpoznawania
        var choices = new Choices();
        choices.Add("hello", "search for", "what is");
        var grammarBuilder = new GrammarBuilder(choices);
        var grammar = new Grammar(grammarBuilder);
        
        _recognizer.LoadGrammar(grammar);
        _recognizer.SpeechRecognized += (s, e) =>
        {
            OnSpeechRecognized?.Invoke(e.Result.Text);
        };
    }
    
    public void StartListening()
    {
        _recognizer.SetInputToDefaultAudioDevice();
        _recognizer.RecognizeAsync(RecognizeMode.Multiple);
    }
    
    public void StopListening()
    {
        _recognizer.RecognizeAsyncStop();
    }
    
    public void Speak(string text)
    {
        _synthesizer.SpeakAsync(text);
    }
}
