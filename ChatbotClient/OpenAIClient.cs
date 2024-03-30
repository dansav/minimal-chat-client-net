namespace ChatbotClient;

public class OpenAIClient : ChatbotClientBase
{
    public OpenAIClient(string model = "gpt-3.5-turbo") : base("https://api.openai.com/v1/", Environment.GetEnvironmentVariable("MY_OAIK") ?? throw new Exception($"The environment variable 'MY_OAIK' could not be read."))
    {
        Model = model;
    }
}