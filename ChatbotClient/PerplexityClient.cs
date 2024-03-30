namespace ChatbotClient;

public class PerplexityClient : ChatbotClientBase
{
    public PerplexityClient(string model = "mistral-7b-instruct") : base("https://api.perplexity.ai/", Environment.GetEnvironmentVariable("MY_PPLXK") ?? throw new Exception($"The environment variable 'MY_PPLXK' could not be read."))
    {
        Model = model;
    }
}
