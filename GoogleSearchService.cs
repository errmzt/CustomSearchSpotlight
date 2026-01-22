using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CustomSearchApp
{
    public class GoogleSearchService
    {
        private readonly string _apiKey;
        private readonly string _searchEngineId;
        private readonly HttpClient _httpClient;
        
        public GoogleSearchService(string apiKey, string searchEngineId)
        {
            _apiKey = apiKey;
            _searchEngineId = searchEngineId;
            _httpClient = new HttpClient();
        }
        
        public async Task<List<WebSearchResult>> SearchAsync(string query, int maxResults = 10)
        {
            var results = new List<WebSearchResult>();
            
            try
            {
                string url = $"https://www.googleapis.com/customsearch/v1?key={_apiKey}&cx={_searchEngineId}&q={Uri.EscapeDataString(query)}&num={maxResults}";
                
                var response = await _httpClient.GetStringAsync(url);
                var searchResult = JsonConvert.DeserializeObject<GoogleSearchResponse>(response);
                
                if (searchResult?.Items != null)
                {
                    results = searchResult.Items.Select(item => new WebSearchResult
                    {
                        Title = item.Title,
                        Url = item.Link,
                        Snippet = item.Snippet,
                        DisplayUrl = item.DisplayLink
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google Search error: {ex.Message}");
            }
            
            return results;
        }
    }
    
    public class WebSearchResult
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Snippet { get; set; }
        public string DisplayUrl { get; set; }
        
        public string Description => $"{Snippet}";
    }
    
    public class GoogleSearchResponse
    {
        public List<GoogleSearchItem> Items { get; set; }
    }
    
    public class GoogleSearchItem
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Snippet { get; set; }
        public string DisplayLink { get; set; }
    }
}
