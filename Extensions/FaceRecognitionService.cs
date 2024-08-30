using Newtonsoft.Json;
using System.Text;

public class FaceRecognitionService
{
    private readonly HttpClient _httpClient;

    public FaceRecognitionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> SendImageAsync(string imageData)
    {
        var requestData = new
        {
            image = imageData
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync("https://192.168.1.9:5000/predict", content);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
            else
            {
                // Log error response for debugging
                string errorResponse = await response.Content.ReadAsStringAsync();
                return $"Error: {response.StatusCode}, Details: {errorResponse}";
            }
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }
}
