using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Linq;

public partial class MainWindow : Window
{
    private readonly IAIService _aiService;
    private readonly ISearchService _localSearchService;
    private readonly ISearchService _webSearchService;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Inicjalizacja serwisów (klucze z konfiguracji)
        _aiService = new GeminiService("YOUR_GEMINI_API_KEY");
        _localSearchService = new WindowsSearchService();
        _webSearchService = new GoogleSearchService("YOUR_GOOGLE_SEARCH_API_KEY", "YOUR_CSE_ID");
    }
    
    private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = SearchBox.Text;
        
        if (string.IsNullOrEmpty(query))
        {
            ResultsList.ItemsSource = null;
            return;
        }
        
        // Wyszukiwanie równoległe
        var localResults = _localSearchService.SearchAsync(query);
        var webResults = _webSearchService.SearchAsync(query);
        var aiResult = _aiService.AskAsync(query);
        
        await Task.WhenAll(localResults, webResults, aiResult);
        
        var results = new List<SearchResult>();
        results.AddRange(localResults.Result);
        results.AddRange(webResults.Result);
        results.Add(new SearchResult
        {
            Title = "AI Suggestion",
            Description = aiResult.Result,
            Type = ResultType.AI
        });
        
        ResultsList.ItemsSource = results;
    }
}
