using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace _1ДЛЯ_ТЕСТА_ДИЗАЙНА_ПРОСТО
{
    public class ImageUploader
    {
        private readonly string apiKey = "78a5754b8e7b765336fa4f72d40c7098"; // Замени на свой API-ключ от ImgBB
        private readonly HttpClient httpClient;

        public ImageUploader()
        {
            httpClient = new HttpClient();
        }

        public async Task<string> UploadImageAsync(string filePath)
        {
            try
            {
                // Читаем файл в массив байтов
                byte[] imageBytes = File.ReadAllBytes(filePath);
                string base64Image = Convert.ToBase64String(imageBytes);

                // Формируем запрос к ImgBB
                var formData = new MultipartFormDataContent
                {
                    { new StringContent(apiKey), "key" },
                    { new StringContent(base64Image), "image" }
                };

                var response = await httpClient.PostAsync("https://api.imgbb.com/1/upload", formData);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = System.Text.Json.JsonDocument.Parse(responseContent);
                string imageUrl = jsonResponse.RootElement.GetProperty("data").GetProperty("url").GetString();

                return imageUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки изображения на ImgBB: {ex.Message}", ex);
            }
        }
    }
}
