using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatbotClient;

[JsonConverter(typeof(ChatbotMessageConverter))]
public record ChatbotMessage(string Role, string Text, byte[]? Image = default);

public class ChatbotMessageConverter : JsonConverter<ChatbotMessage>
{
    public override ChatbotMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string role = string.Empty;
        string text = string.Empty;
        byte[]? image = null;

        while (reader.Read()) 
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new ChatbotMessage(role, text, image);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString() ?? throw new NullReferenceException("Expected a property name string.");
                reader.Read();

                switch (propertyName)
                {
                    case "role":
                        role = reader.GetString() ?? throw new NullReferenceException("Expected a string value for 'role'.");
                        break;
                    case "content":
                        if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            ReadContentArray(ref reader, ref text, ref image);
                        }
                        else
                        {
                            text = reader.GetString() ?? throw new NullReferenceException("Expected a string value for 'text'.");
                        }
                        break;
                }
            }
        }

        // should never reach here
        return new ChatbotMessage(role, text, image);
    }

    private void ReadContentArray(ref Utf8JsonReader reader, ref string text, ref byte[]? image)
    {
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string contentPropertyName = reader.GetString() ?? throw new NullReferenceException("Expected a property name string.");
                        reader.Read();

                        switch (contentPropertyName)
                        {
                            case "text":
                                text = reader.GetString() ?? throw new NullReferenceException("Expected a string value for 'text'."); ;
                                break;
                            case "image_url":
                                if (reader.TokenType == JsonTokenType.StartObject)
                                {
                                    while (reader.Read())
                                    {
                                        if (reader.TokenType == JsonTokenType.EndObject)
                                        {
                                            break;
                                        }

                                        if (reader.TokenType == JsonTokenType.PropertyName)
                                        {
                                            string imageUrlPropertyName = reader.GetString();

                                            if (imageUrlPropertyName == "url")
                                            {
                                                reader.Read();

                                                var imageUrl = reader.GetString();
                                                if (imageUrl?.StartsWith("data:image/jpeg;base64,") == true)
                                                {
                                                    string base64Image = imageUrl.Split(',')[1];
                                                    image = Convert.FromBase64String(base64Image);
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, ChatbotMessage value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("role", value.Role);

        writer.WritePropertyName("content");

        if (value.Image is not null)
        {
            /*
            "content": [
              {"type": "text", "text": "What’s in this image?"},
              {
                "type": "image_url",
                "image_url": {
                  "url": f"data:image/jpeg;base64,{base64_image}"
                },
              },
            ],*/

            writer.WriteStartArray();
            
            writer.WriteStartObject();
            writer.WriteString("type", "text");
            writer.WriteString("text", value.Text);
            writer.WriteEndObject();

            writer.WriteStartObject();
            writer.WriteString("type", "image_url");

            writer.WritePropertyName("image_url");
            writer.WriteStartObject();
            writer.WriteString("url", $"data:image/jpeg;base64,{Convert.ToBase64String(value.Image)}");
            writer.WriteEndObject();

            writer.WriteEndObject();

            writer.WriteEndArray();
        }
        else
        {
            writer.WriteStringValue(value.Text);
        }

        writer.WriteEndObject();
    }
}
