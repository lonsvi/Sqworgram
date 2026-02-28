using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public class TranslationService
    {
        private readonly HttpClient _httpClient;

        public TranslationService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
        {
            try
            {
                var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair={sourceLanguage}|{targetLanguage}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using (var document = JsonDocument.Parse(jsonResponse))
                {
                    var root = document.RootElement;

                    if (root.TryGetProperty("responseStatus", out var status) && status.GetInt32() == 200)
                    {
                        var translatedText = root.GetProperty("responseData").GetProperty("translatedText").GetString();
                        return translatedText ?? "Ошибка перевода: пустой результат.";
                    }
                    else
                    {
                        var errorMessage = root.TryGetProperty("responseDetails", out var details)
                            ? details.GetString()
                            : "Неизвестная ошибка перевода.";
                        return $"Ошибка перевода: {errorMessage}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка перевода: {ex.Message}";
            }
        }
    }
}