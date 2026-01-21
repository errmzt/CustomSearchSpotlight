using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CustomSearchSpotlight.Models;
using CustomSearchSpotlight.Services;

namespace CustomSearchSpotlight.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _searchQuery;
        private bool _showSearchHint = true;
        private ObservableCollection<SearchResult> _searchResults;
        private SearchResult _selectedResult;
        private readonly ISearchService _searchService;
        private readonly IAIService _aiService;

        public MainViewModel()
        {
            _searchService = new WindowsSearchService();
            _aiService = new GeminiService("YOUR_API_KEY"); // Replace with your API key
            SearchResults = new ObservableCollection<SearchResult>();
            ExecuteSearchCommand = new RelayCommand(ExecuteSearch);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                ShowSearchHint = string.IsNullOrEmpty(value);
                // Trigger search as you type
                ExecuteSearch(null);
            }
        }

        public bool ShowSearchHint
        {
            get => _showSearchHint;
            set
            {
                _showSearchHint = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<SearchResult> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                OnPropertyChanged();
            }
        }

        public SearchResult SelectedResult
        {
            get => _selectedResult;
            set
            {
                _selectedResult = value;
                OnPropertyChanged();
                if (value != null)
                {
                    // Handle selection
                    ExecuteResult(value);
                }
            }
        }

        public ICommand ExecuteSearchCommand { get; }

        private async void ExecuteSearch(object parameter)
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchResults.Clear();
                return;
            }

            var results = await _searchService.SearchAsync(SearchQuery);
            SearchResults = new ObservableCollection<SearchResult>(results);

            // If no results from local search, you might want to show AI suggestions
            if (SearchResults.Count == 0)
            {
                // Optionally, use AI to generate a response
                var aiResponse = await _aiService.GetCompletionAsync(SearchQuery);
                SearchResults.Add(new SearchResult
                {
                    Title = "AI Suggestion",
                    Description = aiResponse,
                    Type = ResultType.AI
                });
            }
        }

        private void ExecuteResult(SearchResult result)
        {
            // Handle opening the result (file, app, etc.)
            // For now, just a placeholder
            System.Diagnostics.Process.Start(result.Path);
        }
    }

    public abstract class ViewModelBase : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);
    }
}
