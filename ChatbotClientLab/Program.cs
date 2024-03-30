// See https://aka.ms/new-console-template for more information
using ChatbotClient;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

Console.WriteLine("Hello, World!");
Console.WriteLine();

ChatbotMessage[] messages = [
    new ("system", "Be precise and concise in your responses."),
    new ("user", "How many stars are there in our galaxy?")
];

foreach (var message in messages)
{
    Console.WriteLine($"{message.Role}: {message.Text}");
}
Console.WriteLine();

await TestClientAsync(new PerplexityClient(), "Perplexity.ai");

await TestClientAsync(new GroqClient(), "Groq");

await TestClientAsync(new OpenAIClient(), "OpenAI");

await TestClientAsync(new CustomClient(
    "http://localhost:1234/v1/",
    string.Empty,
    "PsiPi/liuhaotian_llava-v1.5-13b-GGUF/llava-v1.5-13b-Q5_K_M.gguf"), "Local lm-studio");

await TestClientWithImageAsync(new CustomClient(
    "http://localhost:1234/v1/",
    string.Empty,
    "PsiPi/liuhaotian_llava-v1.5-13b-GGUF/llava-v1.5-13b-Q5_K_M.gguf"), "Local lm-studio");


async Task TestClientAsync(IChatbotClient client, string name)
{
    Console.WriteLine($"{name}, {client.Model}");
    var result = await client.GetResponseAsync(messages);
    Console.WriteLine($"{result.Role}: {result.Text}");
    Console.WriteLine($"Stats - prompt tokens: {client.TotalPromptTokens}, completion tokes: {client.TotalCompletionTokens}");
    Console.WriteLine();
}

async Task TestClientWithImageAsync(IChatbotClient client, string name)
{
    // TODO: explore what support exists for passing a local file path to the client (will not work for GPT-4)
    // should move base64 encoding out of the serialization at least
    var imageBytes = await File.ReadAllBytesAsync("robot.jpg");

    using var image = Image.Load(imageBytes);
    image.Mutate(x => x.Resize(256, 256)); // having issues with larger image sizes (probably due to the base64 encoding)
    using var ms = new MemoryStream();
    image.SaveAsJpeg(ms);
    imageBytes = ms.ToArray();

    Console.WriteLine($"{name}, {client.Model}");
    var result = await client.GetResponseAsync([
        new ChatbotMessage("system", "This is a chat between a user and an assistant. The assistant is helping the user to describe an image."),
        new ChatbotMessage("user", "What is in this image?", imageBytes)
        ]);
    Console.WriteLine($"{result.Role}: {result.Text}");
    Console.WriteLine($"Stats - prompt tokens: {client.TotalPromptTokens}, completion tokes: {client.TotalCompletionTokens}");
    Console.WriteLine();
}