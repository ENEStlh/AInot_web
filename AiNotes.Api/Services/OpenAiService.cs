using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AiNotes.Api.Services;

public sealed class OpenAiService(
    HttpClient httpClient,
    IConfiguration configuration) : IAiService
{
    private const int MaxAiContentLength = 20000;

    public async Task<string> Summarize(
        string content,
        CancellationToken cancellationToken)
    {
        var responseContent = await SendChatRequest(
            "You summarize notes clearly and concisely. Return only the summary text.",
            content,
            cancellationToken);

        return responseContent.Trim();
    }

    public async Task<IReadOnlyList<string>> SuggestTags(
        string content,
        CancellationToken cancellationToken)
    {
        var responseContent = await SendChatRequest(
            "Suggest at most 5 short, relevant tags for this note. Return only valid JSON in this exact shape: {\"tags\":[\"tag1\",\"tag2\"]}.",
            content,
            cancellationToken,
            jsonMode: true);

        try
        {
            using var document = JsonDocument.Parse(responseContent);
            if (!document.RootElement.TryGetProperty("tags", out var tagsElement) ||
                tagsElement.ValueKind != JsonValueKind.Array)
            {
                throw new AiServiceException("OpenAI etiket yanıtı beklenen JSON formatında değil.");
            }

            return tagsElement.EnumerateArray()
                .Where(tag => tag.ValueKind == JsonValueKind.String)
                .Select(tag => tag.GetString()?.Trim())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag!.ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .ToList();
        }
        catch (JsonException exception)
        {
            throw new AiServiceException("OpenAI etiket yanıtı geçerli JSON değil.", exception);
        }
    }

    private async Task<string> SendChatRequest(
        string systemPrompt,
        string content,
        CancellationToken cancellationToken,
        bool jsonMode = false)
    {
        if (content.Length > MaxAiContentLength)
        {
            throw new AiServiceException(
                "AI işlemleri için not içeriği en fazla 20000 karakter olabilir.");
        }

        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new AiServiceException("OpenAI API anahtarı user-secrets içinde tanımlı değil.");
        }

        var requestBody = new
        {
            model = configuration["OpenAI:Model"] ?? "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content }
            },
            temperature = 0.2,
            response_format = jsonMode ? new { type = "json_object" } : null
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new AiServiceException("OpenAI servisine bağlanılamadı.", exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AiServiceException("OpenAI isteği zaman aşımına uğradı.", exception);
        }

        using (response)
        {
        if (!response.IsSuccessStatusCode)
        {
            throw new AiServiceException(
                $"OpenAI isteği başarısız oldu. HTTP {(int)response.StatusCode}.");
        }

        try
        {
            var completion = await response.Content
                .ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken);
            var result = completion?.Choices.FirstOrDefault()?.Message.Content;

            if (string.IsNullOrWhiteSpace(result))
            {
                throw new AiServiceException("OpenAI boş bir yanıt döndürdü.");
            }

            return result;
        }
        catch (JsonException exception)
        {
            throw new AiServiceException("OpenAI yanıtı okunamadı.", exception);
        }
        }
    }

    private sealed record ChatCompletionResponse(IReadOnlyList<Choice> Choices);
    private sealed record Choice(Message Message);
    private sealed record Message(string Content);
}