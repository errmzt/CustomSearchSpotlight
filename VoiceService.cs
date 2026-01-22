using System;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.IO;

namespace CustomSearchApp
{
    public class VoiceService : IDisposable
    {
        private SpeechRecognitionEngine _recognizer;
        private SpeechSynthesizer _synthesizer;
        private bool _isListening;
        
        public event EventHandler<string> SpeechRecognized;
        public event EventHandler<string> ListeningStarted;
        public event EventHandler ListeningStopped;
        
        public VoiceService()
        {
            InitializeSpeechRecognition();
            InitializeSpeechSynthesis();
        }
        
        private void InitializeSpeechRecognition()
        {
            try
            {
                _recognizer = new SpeechRecognitionEngine();
                
                // Load grammar
                var grammarBuilder = new GrammarBuilder();
                grammarBuilder.AppendDictation();
                
                var grammar = new Grammar(grammarBuilder);
                _recognizer.LoadGrammar(grammar);
                
                // Configure recognition
                _recognizer.SetInputToDefaultAudioDevice();
                _recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
                _recognizer.SpeechHypothesized += Recognizer_SpeechHypothesized;
                
                // Optional: Add custom commands
                AddCustomCommands();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Speech recognition init error: {ex.Message}");
            }
        }
        
        private void InitializeSpeechSynthesis()
        {
            try
            {
                _synthesizer = new SpeechSynthesizer();
                _synthesizer.SetOutputToDefaultAudioDevice();
                _synthesizer.Rate = 1; // Speed: -10 to 10
                _synthesizer.Volume = 100; // 0-100
                
                // Use a nicer voice if available
                SelectBestVoice();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Speech synthesis init error: {ex.Message}");
            }
        }
        
        private void SelectBestVoice()
        {
            foreach (var voice in _synthesizer.GetInstalledVoices())
            {
                var info = voice.VoiceInfo;
                // Prefer female voices (they often sound better for assistants)
                if (info.Name.Contains("Zira") || info.Name.Contains("David") || 
                    info.Name.Contains("Hazel") || info.Name.Contains("Ewa"))
                {
                    _synthesizer.SelectVoice(info.Name);
                    break;
                }
            }
        }
        
        private void AddCustomCommands()
        {
            // Create grammar for specific commands
            var commands = new Choices();
            commands.Add("hej asystent");
            commands.Add("okay asystent");
            commands.Add("zamknij");
            commands.Add("ukryj");
            commands.Add("pokaż się");
            commands.Add("wyjdź");
            commands.Add("pomoc");
            
            var gb = new GrammarBuilder();
            gb.Append(commands);
            var commandGrammar = new Grammar(gb);
            
            _recognizer.LoadGrammar(commandGrammar);
        }
        
        public void StartListening()
        {
            if (_recognizer == null || _isListening) return;
            
            try
            {
                _recognizer.RecognizeAsync(RecognizeMode.Multiple);
                _isListening = true;
                ListeningStarted?.Invoke(this, "Nasłuchiwanie rozpoczęte...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Start listening error: {ex.Message}");
            }
        }
        
        public void StopListening()
        {
            if (_recognizer == null || !_isListening) return;
            
            try
            {
                _recognizer.RecognizeAsyncStop();
                _isListening = false;
                ListeningStopped?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stop listening error: {ex.Message}");
            }
        }
        
        public bool IsListening => _isListening;
        
        public async Task SpeakAsync(string text)
        {
            if (_synthesizer == null || string.IsNullOrWhiteSpace(text)) return;
            
            await Task.Run(() =>
            {
                try
                {
                    _synthesizer.Speak(text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Speech error: {ex.Message}");
                }
            });
        }
        
        public void Speak(string text)
        {
            if (_synthesizer == null || string.IsNullOrWhiteSpace(text)) return;
            
            try
            {
                _synthesizer.SpeakAsync(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Speech error: {ex.Message}");
            }
        }
        
        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            var text = e.Result.Text;
            Console.WriteLine($"Rozpoznano: {text}");
            
            SpeechRecognized?.Invoke(this, text);
            
            // Handle commands
            HandleVoiceCommand(text);
        }
        
        private void Recognizer_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            // Real-time text while speaking
            // Console.WriteLine($"W trakcie: {e.Result.Text}");
        }
        
        private void HandleVoiceCommand(string command)
        {
            command = command.ToLower();
            
            switch (command)
            {
                case "hej asystent":
                case "okay asystent":
                    Speak("Tak, słucham?");
                    break;
                    
                case "zamknij":
                case "wyjdź":
                    Speak("Zamykam aplikację");
                    System.Windows.Application.Current.Shutdown();
                    break;
                    
                case "ukryj":
                    Speak("Chowam okno");
                    // Hide window logic
                    break;
                    
                case "pokaż się":
                    Speak("Otwieram wyszukiwarkę");
                    // Show window logic
                    break;
                    
                case "pomoc":
                    Speak("Możesz mówić: hej asystent, zamknij, ukryj, pokaż się, lub dyktować zapytania");
                    break;
            }
        }
        
        public void Dispose()
        {
            _recognizer?.Dispose();
            _synthesizer?.Dispose();
        }
    }
}
using System.Speech.Recognition;
var recognizer = new SpeechRecognitionEngine();
recognizer.LoadGrammar(new DictationGrammar());
recognizer.SpeechRecognized += (s, e) => {
    // e.Result.Text - przekaż tekst do pola wyszukiwania
};
recognizer.SetInputToDefaultAudioDevice();
recognizer.RecognizeAsync(RecognizeMode.Multiple);
