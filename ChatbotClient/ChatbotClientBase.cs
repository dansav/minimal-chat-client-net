using System.Net.Http.Headers;
using System.Text;

namespace ChatbotClient;

public class ChatbotClientBase : IChatbotClient
{
    private readonly string _apiKey;
    private readonly HttpClient _http;

    private int _totalPromptTokens;
    private int _totalCompletionTokens;

    public ChatbotClientBase(string baseAddress, string apiKey)
    {
        _apiKey = apiKey;

        _http = new HttpClient();
        _http.BaseAddress = new Uri(baseAddress);
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public virtual string Model { get; protected set; } = string.Empty;

    public virtual int TotalPromptTokens => _totalPromptTokens;

    public virtual int TotalCompletionTokens => _totalCompletionTokens;

    public virtual async Task<string> GetResponseAsync(string message)
    {
        var msg = await GetResponseAsync([ new ("user", message)]);
        return msg.Text;
    }

    public virtual async Task<ChatbotMessage> GetResponseAsync(IEnumerable<ChatbotMessage> messages)
    {
        var requestData = new
        {
            model = Model,
            stream = false,
            max_tokens = 1024,
            frequency_penalty = 1,
            temperature = 0.0,
            messages = messages.ToArray()
        };

        var json = System.Text.Json.JsonSerializer.Serialize(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("chat/completions", content);
        var jsonResult = await response.Content.ReadAsStringAsync();

        var result = System.Text.Json.JsonSerializer.Deserialize<JsonResponse>(jsonResult);

        _totalPromptTokens += result?.usage?.prompt_tokens ?? 0;
        _totalCompletionTokens += result?.usage?.completion_tokens ?? 0;

        var msg = result?.choices[0]?.message;

        return new ChatbotMessage(msg?.role ?? string.Empty, msg?.content ?? string.Empty);
    }

    private record JsonResponse(string id, string model, string @object, int created, JsonChoice[] choices, JsonUsage usage);

    private record JsonChoice(int index, string finish_reason, JsonMessage message, JsonMessage delta);

    private record JsonMessage(string role, string content);

    private record JsonUsage(int prompt_tokens, int completion_tokens, int total_tokens);
}
