using System;
using System.Windows;
using System.Windows.Input;

namespace CustomSearchApp
{
    public partial class MainWindow : GlassWindow
    {
        private HotkeyManager _hotkeyManager;
        private GeminiService _geminiService;
        
        public MainWindow()
        {
            InitializeComponent();
            SetupHotkey();
            SetupGemini();
        }
        
        private void SetupHotkey()
        {
            _hotkeyManager = new HotkeyManager();
        }
        
        private void SetupGemini()
        {
            // TU WPISZ SWÓJ KLUCZ API GEMINI!
            string apiKey = "AIzaSyTwojKluczAPIWpiszTutaj";
            _geminiService = new GeminiService(apiKey);
        }
        
        private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var question = SearchBox.Text;
                if (!string.IsNullOrWhiteSpace(question))
                {
                    await AskGeminiAsync(question);
                }
            }
        }
        
        private async void ExampleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string question)
            {
                SearchBox.Text = question;
                await AskGeminiAsync(question);
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
            }
            catch (Exception ex)
            {
                AiResponseText.Text = $"Błąd: {ex.Message}";
                StatusText.Text = "❌ Wystąpił błąd";
            }
        }
        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            var windowHelper = new WindowInteropHelper(this);
            _hotkeyManager.Register(windowHelper.Handle);
            
            var source = HwndSource.FromHwnd(windowHelper.Handle);
            source?.AddHook(HwndHook);
        }
        
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            
            if (msg == WM_HOTKEY && wParam.ToInt32() == 9000)
            {
                if (IsVisible)
                {
                    Hide();
                }
                else
                {
                    Show();
                    Activate();
                    Topmost = true;
                    SearchBox.Focus();
                    SearchBox.SelectAll();
                }
                handled = true;
            }
            
            return IntPtr.Zero;
        }
        
        protected override void OnClosed(EventArgs e)
        {
            _hotkeyManager.Unregister();
            base.OnClosed(e);
        }
    }
}
private void AddToStartup()
{
    var startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
    var shortcutPath = System.IO.Path.Combine(startupPath, "CustomSearch Spotlight.lnk");
    
    if (!System.IO.File.Exists(shortcutPath))
    {
    }
}
