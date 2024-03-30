namespace ChatbotClient;

public class CustomClient : ChatbotClientBase
{
    public CustomClient(string baseAddress, string apiKey, string model) : base(baseAddress, apiKey)
    {
        Model = model;
    }
}
