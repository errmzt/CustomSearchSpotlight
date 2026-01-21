// Services/GeminiService.cs
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CustomSearchSpotlight.Services
{
    public interface IAIService
    {
        Task<string> GetCompletionAsync(string prompt, List<ChatMessage> conversationHistory = null);
        Task<Stream> GetCompletionStreamAsync(string prompt);
        Task<bool> TestConnectionAsync();
    }

    public class GeminiService : IAIService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly string _model = "gemini-1.5-pro-latest"; // lub gemini-1.5-flash-latest
        
        public GeminiService(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> GetCompletionAsync(string prompt, List<ChatMessage> conversationHistory = null)
        {
            try
            {
                var requestBody = new
                {
                    contents = BuildContents(prompt, conversationHistory),
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 2048,
                    },
                    safetySettings = new[]
                    {
                        new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                        new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                        new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                        new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
                    }
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Gemini API error: {response.StatusCode} - {error}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseString);

                if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Length == 0)
                    return "No response from AI";

                return geminiResponse.Candidates[0].Content.Parts[0].Text;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<Stream> GetCompletionStreamAsync(string prompt)
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
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 2048,
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:streamGenerateContent?key={_apiKey}&alt=sse",
                content);

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var testPrompt = "Hello, respond with 'OK' if you can hear me.";
                var response = await GetCompletionAsync(testPrompt);
                return !string.IsNullOrEmpty(response) && response.Contains("OK");
            }
            catch
            {
                return false;
            }
        }

        private dynamic[] BuildContents(string prompt, List<ChatMessage> history)
        {
            var contents = new List<dynamic>();

            // Dodaj historię konwersacji jeśli istnieje
            if (history != null)
            {
                foreach (var message in history)
                {
                    contents.Add(new
                    {
                        role = message.Role == ChatRole.User ? "user" : "model",
                        parts = new[] { new { text = message.Content } }
                    });
                }
            }

            // Dodaj aktualny prompt
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = prompt } }
            });

            return contents.ToArray();
        }
    }

    public class GeminiResponse
    {
        public Candidate[] Candidates { get; set; }
        
        public class Candidate
        {
            public Content Content { get; set; }
        }
        
        public class Content
        {
            public Part[] Parts { get; set; }
        }
        
        public class Part
        {
            public string Text { get; set; }
        }
    }

    public class ChatMessage
    {
        public ChatRole Role { get; set; }
        public string Content { get; set; }
    }

    public enum ChatRole
    {
        User,
        Assistant
    }
}
