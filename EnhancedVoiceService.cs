using System;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.IO;

namespace CustomSearchSpotlight
{
    public class EnhancedVoiceService : IDisposable
    {
        private SpeechRecognitionEngine _recognizer;
        private SpeechSynthesizer _synthesizer;
        private bool _isInitialized;
        
        public event Action<string> SpeechRecognized;
        public event Action<string> StatusChanged;
        
        public bool IsListening => _recognizer?.AudioState == AudioState.Monitoring;
        
        public EnhancedVoiceService()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            try
            {
                // Initialize speech synthesis
                _synthesizer = new SpeechSynthesizer();
                _synthesizer.SetOutputToDefaultAudioDevice();
                _synthesizer.Rate = 1;
                _synthesizer.Volume = 100;
                
                // Initialize speech recognition
                _recognizer = new SpeechRecognitionEngine();
                
                // Create grammar
                var grammarBuilder = new GrammarBuilder();
                grammarBuilder.AppendDictation();
                
                var grammar = new Grammar(grammarBuilder);
                _recognizer.LoadGrammar(grammar);
                
                // Configure recognizer
                _recognizer.SetInputToDefaultAudioDevice();
                _recognizer.BabbleTimeout = TimeSpan.FromSeconds(1);
                _recognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(2);
                
                // Events
                _recognizer.SpeechRecognized += OnSpeechRecognized;
                _recognizer.SpeechHypothesized += OnSpeechHypothesized;
                
                _isInitialized = true;
                StatusChanged?.Invoke("Voice service initialized");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"Voice initialization failed: {ex.Message}");
            }
        }
        
        public void StartListening()
        {
            if (!_isInitialized) return;
            
            try
            {
                _recognizer.RecognizeAsync(RecognizeMode.Multiple);
                StatusChanged?.Invoke("🎤 Nasłuchiwanie...");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"Error starting listening: {ex.Message}");
            }
        }
        
        public void StopListening()
        {
            if (!_isInitialized) return;
            
            try
            {
                _recognizer.RecognizeAsyncStop();
                StatusChanged?.Invoke("Voice listening stopped");
            }
            catch { }
        }
        
        public async Task SpeakAsync(string text)
        {
            if (!_isInitialized) return;
            
            await Task.Run(() =>
            {
                try
                {
                    _synthesizer.Speak(text);
                }
                catch { }
            });
        }
        
        public void Speak(string text)
        {
            if (!_isInitialized) return;
            
            try
            {
                _synthesizer.SpeakAsync(text);
            }
            catch { }
        }
        
        private void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            var text = e.Result?.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                SpeechRecognized?.Invoke(text);
                StatusChanged?.Invoke($"🎤 Rozpoznano: {text}");
            }
        }
        
        private void OnSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            // Optional: Handle partial results
        }
        
        public void Dispose()
        {
            _recognizer?.Dispose();
            _synthesizer?.Dispose();
        }
    }
}
