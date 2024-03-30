using System.Buffers.Text;

namespace ChatbotClient;

public interface IChatbotClient
{
    string Model { get; }

    int TotalPromptTokens { get; }
    
    int TotalCompletionTokens { get; }

    Task<string> GetResponseAsync(string message);

    Task<ChatbotMessage> GetResponseAsync(IEnumerable<ChatbotMessage> messages);
}
