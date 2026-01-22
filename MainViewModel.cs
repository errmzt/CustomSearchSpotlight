using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CustomSearchApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _searchQuery;
        private string _aiResponse;
        private bool _showAiResponse;
        private bool _showFileResults;
        private string _statusText;
        
        private readonly GeminiService _geminiService;
        private readonly FileSearchService _fileSearchService;
        private readonly VoiceService _voiceService;
        
        public ObservableCollection<FileSearchResult> FileResults { get; }
        public ObservableCollection<object> SearchResults { get; }
        
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                OnSearchQueryChanged(value);
            }
        }
        
        public string AiResponse
        {
            get => _aiResponse;
            set
            {
                _aiResponse = value;
                OnPropertyChanged();
            }
        }
        
        public bool ShowAiResponse
        {
            get => _showAiResponse;
            set
            {
                _showAiResponse = value;
                OnPropertyChanged();
            }
        }
        
        public bool ShowFileResults
        {
            get => _showFileResults;
            set
            {
                _showFileResults = value;
                OnPropertyChanged();
            }
        }
        
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }
        
        public ICommand SearchCommand { get; }
        public ICommand VoiceCommand { get; }
        public ICommand OpenFileCommand { get; }
        
        public MainViewModel()
        {
            // Initialize services
            _geminiService = new GeminiService("YOUR_API_KEY");
            _fileSearchService = new FileSearchService();
            _voiceService = new VoiceService();
            
            // Initialize collections
            FileResults = new ObservableCollection<FileSearchResult>();
            SearchResults = new ObservableCollection<object>();
            
            // Initialize commands
            SearchCommand = new RelayCommand(async () => await ExecuteSearch());
            VoiceCommand = new RelayCommand(ToggleVoice);
            OpenFileCommand = new RelayCommand(OpenSelectedFile);
            
            // Setup voice events
            _voiceService.SpeechRecognized += (s, text) =>
            {
                SearchQuery = text;
                _ = ExecuteSearch();
            };
            
            StatusText = "Gotowy - mów lub pisz...";
        }
        
        private async void OnSearchQueryChanged(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                FileResults.Clear();
                ShowAiResponse = false;
                ShowFileResults = false;
                return;
            }
            
            // Show loading
            StatusText = "Szukam...";
            
            // Search files in parallel
            var fileTask = _fileSearchService.SearchFilesAsync(query, 10);
            
            // Check if it's a question for AI
            var isQuestion = query.Contains("?") || query.Contains("jak") || 
                            query.Contains("co to") || query.Contains("czy");
            
            if (isQuestion)
            {
                ShowAiResponse = true;
                AiResponse = "Myślę...";
                
                var aiTask = _geminiService.AskQuestionAsync(query);
                await Task.WhenAll(fileTask, aiTask);
                
                AiResponse = aiTask.Result;
                StatusText = "Znaleziono pliki i odpowiedź AI";
            }
            else
            {
                await fileTask;
                ShowAiResponse = false;
                StatusText = $"Znaleziono {fileTask.Result.Count} plików";
            }
            
            // Update file results
            FileResults.Clear();
            foreach (var file in fileTask.Result)
            {
                FileResults.Add(file);
            }
            
            ShowFileResults = FileResults.Count > 0;
        }
        
        private async Task ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery)) return;
            OnSearchQueryChanged(SearchQuery);
        }
        
        private void ToggleVoice()
        {
            if (_voiceService.IsListening)
            {
                _voiceService.StopListening();
                StatusText = "Głos wyłączony";
            }
            else
            {
                _voiceService.StartListening();
                StatusText = "Nasłuchiwanie... Mów teraz";
            }
        }
        
        private void OpenSelectedFile()
        {
            // Implementation for opening files
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    // Simple RelayCommand implementation
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        
        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        
        public void Execute(object parameter) => _execute();
        
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
