namespace Manage_CLB_HTSV.Extensions
{
    public interface IFlaskService
    {
    }
    public class FlaskService : IFlaskService
    {
        private readonly HttpClient _httpClient;

        public FlaskService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("FlaskClient");
        }

        public async Task<string> GetFlaskDataAsync()
        {
            var response = await _httpClient.GetAsync("https://192.168.1.9:5000/predict");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
