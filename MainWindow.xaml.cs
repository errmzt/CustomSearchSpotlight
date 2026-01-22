using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace CustomSearchApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupWindow();
        }

        private void SetupWindow()
        {
            // Ustaw pozycję (wyśrodkowane)
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            // Obsługa klawiszy
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            
            // Auto-hide przy straceniu focusu
            Deactivated += (s, e) => Hide();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Wymuś acrylic efekt po załadowaniu
            EnableAcrylicBlur();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // ESC zamyka okno
            if (e.Key == Key.Escape)
            {
                Hide();
            }
        }

        // ============================
        // GLASS/ACRYLIC EFFECT - MAGIA!
        // ============================
        
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private void EnableAcrylicBlur()
        {
            try
            {
                var windowHelper = new WindowInteropHelper(this);
                var accent = new AccentPolicy();
                
                // Ustawienie acrylic blur
                accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
                accent.GradientColor = 0x99000000; // Kolor z alpha: #99000000
                
                var accentStructSize = Marshal.SizeOf(accent);
                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);
                
                var data = new WindowCompositionAttributeData();
                data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
                data.SizeOfData = accentStructSize;
                data.Data = accentPtr;
                
                SetWindowCompositionAttribute(windowHelper.Handle, ref data);
                
                Marshal.FreeHGlobal(accentPtr);
            }
            catch (Exception ex)
            {
                // Jeśli acrylic nie działa, pokaż komunikat
                SearchBox.Text = $"Glass efekt: {ex.Message}";
            }
        }

        // Struktury dla Win32 API
        internal enum AccentState
        {
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }
    }
}
public partial class MainWindow : GlassWindow
{
    private HotkeyManager _hotkeyManager;
    private GeminiService _geminiService;
    private VoiceService _voiceService;
    private bool _isVoiceActive;
    
    public MainWindow()
    {
        InitializeComponent();
        SetupHotkey();
        SetupGemini();
        SetupVoice();
    }
    
    private void SetupVoice()
    {
        _voiceService = new VoiceService();
        _voiceService.SpeechRecognized += VoiceService_SpeechRecognized;
        _voiceService.ListeningStarted += VoiceService_ListeningStarted;
        _voiceService.ListeningStopped += VoiceService_ListeningStopped;
        
        // Start listening in background
        _voiceService.StartListening();
        _isVoiceActive = true;
    }
    
    private void VoiceService_SpeechRecognized(object sender, string text)
    {
        // Update UI from voice thread
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = $"🎤: {text}";
            
            // If it's a question, send to AI
            if (text.EndsWith("?") || text.Contains("jak") || text.Contains("co to") || 
                text.Contains("czy") || text.Contains("dlaczego"))
            {
                SearchBox.Text = text;
                _ = AskGeminiAsync(text);
            }
        });
    }
    
    private void VoiceService_ListeningStarted(object sender, string message)
    {
        Dispatcher.Invoke(() =>
        {
            VoiceButton.Content = "🔴";
            VoiceButton.ToolTip = "Nasłuchiwanie włączone";
        });
    }
    
    private void VoiceService_ListeningStopped(object sender, EventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            VoiceButton.Content = "🎤";
            VoiceButton.ToolTip = "Nasłuchiwanie wyłączone";
        });
    }
    
    private void VoiceButton_Click(object sender, RoutedEventArgs e)
    {
        _isVoiceActive = !_isVoiceActive;
        
        if (_isVoiceActive)
        {
            _voiceService.StartListening();
            _voiceService.Speak("Asystent głosowy włączony");
        }
        else
        {
            _voiceService.StopListening();
            _voiceService.Speak("Asystent głosowy wyłączony");
        }
    }
    
    private async Task AskGeminiAsync(string question)
    {
        try
        {
            StatusText.Text = "🤔 Gemini myśli...";
            AiResponseBox.Visibility = Visibility.Visible;
            AiResponseText.Text = "Proszę czekać...";
            
            var response = await _geminiService.AskQuestionAsync(question);
            
            AiResponseText.Text = response;
            StatusText.Text = "✅ Odpowiedź otrzymana";
            
            // Speak the response
            if (_isVoiceActive)
            {
                // Truncate long responses for speech
                var speechText = response.Length > 200 ? 
                    response.Substring(0, 200) + "..." : response;
                _voiceService.Speak(speechText);
            }
        }
        catch (Exception ex)
        {
            AiResponseText.Text = $"Błąd: {ex.Message}";
            StatusText.Text = "❌ Wystąpił błąd";
        }
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _voiceService?.Dispose();
        base.OnClosed(e);
    }
}
