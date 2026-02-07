using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Sestamk.Classes
{
    public static class GeminiAI
    {
        // ضع API Key الخاص بك هنا
        private static readonly string apiKey = "AIzaSyCxnP6qJu72QTQsDZJO-8ZsidyPXu3B3Y8";

        // استخدام نفس الـ model من المثال الرسمي
        private static readonly string modelName = "gemini-3-flash-preview";
        private static readonly string baseUrl = "https://generativelanguage.googleapis.com/v1beta";

        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        static GeminiAI()
        {
            client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
            client.DefaultRequestHeaders.Add("User-Agent", "Sestamk-GeminiClient/1.0");
        }

        /// <summary>
        /// توليد نص من Gemini AI
        /// </summary>
        public static async Task<string> GenerateText(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("النص المدخل لا يمكن أن يكون فارغاً", nameof(prompt));

            try
            {
                var requestBody = new GeminiRequest
                {
                    Contents = new[]
                    {
                        new Content
                        {
                            Parts = new[]
                            {
                                new Part { Text = prompt }
                            }
                        }
                    }
                };

                var url = $"{baseUrl}/models/{modelName}:generateContent";
                var response = await client.PostAsJsonAsync(url, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"خطأ من Gemini API - الكود: {response.StatusCode}\nالتفاصيل: {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

                var text = result?.Candidates?.FirstOrDefault()
                    ?.Content?.Parts?.FirstOrDefault()
                    ?.Text;

                if (string.IsNullOrEmpty(text))
                    throw new InvalidOperationException("لم يتم استلام رد صحيح من Gemini API");

                return text;
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("انتهت مهلة الاتصال بـ Gemini API");
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }

        /// <summary>
        /// توليد نص مع خيارات متقدمة
        /// </summary>
        public static async Task<string> GenerateText(
            string prompt,
            double temperature = 1.0,
            int maxTokens = 8192,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("النص المدخل لا يمكن أن يكون فارغاً", nameof(prompt));

            try
            {
                var requestBody = new GeminiRequest
                {
                    Contents = new[]
                    {
                        new Content
                        {
                            Parts = new[]
                            {
                                new Part { Text = prompt }
                            }
                        }
                    },
                    GenerationConfig = new GenerationConfig
                    {
                        Temperature = temperature,
                        MaxOutputTokens = maxTokens,
                        TopK = 40,
                        TopP = 0.95
                    }
                };

                var url = $"{baseUrl}/models/{modelName}:generateContent";
                var response = await client.PostAsJsonAsync(url, requestBody, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException(
                        $"خطأ من Gemini API - الكود: {response.StatusCode}\nالتفاصيل: {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(
                    cancellationToken: cancellationToken);

                var text = result?.Candidates?.FirstOrDefault()
                    ?.Content?.Parts?.FirstOrDefault()
                    ?.Text;

                if (string.IsNullOrEmpty(text))
                    throw new InvalidOperationException("لم يتم استلام رد صحيح من Gemini API");

                return text;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException("انتهت مهلة الاتصال بـ Gemini API");
            }
        }

        #region Data Models

        private class GeminiRequest
        {
            [JsonPropertyName("contents")]
            public Content[] Contents { get; set; }

            [JsonPropertyName("generationConfig")]
            public GenerationConfig GenerationConfig { get; set; }
        }

        private class Content
        {
            [JsonPropertyName("parts")]
            public Part[] Parts { get; set; }
        }

        private class Part
        {
            [JsonPropertyName("text")]
            public string Text { get; set; }
        }

        private class GenerationConfig
        {
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; } = 1.0;

            [JsonPropertyName("maxOutputTokens")]
            public int MaxOutputTokens { get; set; } = 8192;

            [JsonPropertyName("topK")]
            public int TopK { get; set; } = 40;

            [JsonPropertyName("topP")]
            public double TopP { get; set; } = 0.95;
        }

        private class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public Candidate[] Candidates { get; set; }

            [JsonPropertyName("usageMetadata")]
            public UsageMetadata UsageMetadata { get; set; }
        }

        private class Candidate
        {
            [JsonPropertyName("content")]
            public Content Content { get; set; }

            [JsonPropertyName("finishReason")]
            public string FinishReason { get; set; }

            [JsonPropertyName("safetyRatings")]
            public SafetyRating[] SafetyRatings { get; set; }
        }

        private class SafetyRating
        {
            [JsonPropertyName("category")]
            public string Category { get; set; }

            [JsonPropertyName("probability")]
            public string Probability { get; set; }
        }

        private class UsageMetadata
        {
            [JsonPropertyName("promptTokenCount")]
            public int PromptTokenCount { get; set; }

            [JsonPropertyName("candidatesTokenCount")]
            public int CandidatesTokenCount { get; set; }

            [JsonPropertyName("totalTokenCount")]
            public int TotalTokenCount { get; set; }
        }

        #endregion
    }
}