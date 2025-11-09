using System.Text;
using Newtonsoft.Json;

namespace Chatbot_Service.Services;

public class GeminiService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key not configured");
        _httpClient = new HttpClient();
        _logger = logger;
    }

    public async Task<string> GenerateResponse(string userMessage, string context)
    {
        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}";

            var prompt = $@"You are a helpful assistant for an auto service center. 
Context: {context}

User Question: {userMessage}

Please provide a friendly and helpful response. If time slots are mentioned in the context, present them clearly.
Keep your response concise and professional.";

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

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Gemini API Error: {responseContent}");
                return "I'm having trouble processing your request right now. Please try again later.";
            }

            var result = JsonConvert.DeserializeObject<GeminiResponse>(responseContent);
            return result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text
                   ?? "I couldn't generate a response. Please try again.";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error calling Gemini API: {ex.Message}");
            return "I'm having trouble connecting to the AI service. Please try again later.";
        }
    }
}

// Gemini API Response Models
public class GeminiResponse
{
    [JsonProperty("candidates")]
    public List<Candidate>? Candidates { get; set; }
}

public class Candidate
{
    [JsonProperty("content")]
    public Content? Content { get; set; }
}

public class Content
{
    [JsonProperty("parts")]
    public List<Part>? Parts { get; set; }
}

public class Part
{
    [JsonProperty("text")]
    public string? Text { get; set; }
}
