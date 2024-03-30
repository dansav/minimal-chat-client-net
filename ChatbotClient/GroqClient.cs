namespace ChatbotClient;

public class GroqClient : ChatbotClientBase
{
    public GroqClient(string model = "gemma-7b-it") : base("https://api.groq.com/openai/v1/", Environment.GetEnvironmentVariable("MY_GROQK") ?? throw new Exception($"The environment variable 'MY_GROQK' could not be read."))
    {
        Model = model;
    }
}