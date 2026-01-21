using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CustomSearchSpotlight.Services
{
    public interface IAIService
    {
        Task<string> GetCompletionAsync(string prompt);
    }

    public class GeminiService : IAIService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public GeminiService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetCompletionAsync(string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}",
                content);

            if (!response.IsSuccessStatusCode)
            {
                return "Error: Unable to get response from AI.";
            }

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);

            return jsonResponse.candidates[0].content.parts[0].text;
        }
    }
}
