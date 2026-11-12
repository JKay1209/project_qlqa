using System;
using System.Net.Http;

namespace Client.Network
{
    internal static class HttpClientManager
    {
        private static readonly HttpClient _client = CreateClient();

        private static HttpClient CreateClient()
        {
            var client = new HttpClient()
            {
                BaseAddress =
                    new Uri("https://reawake-judiciary-thread.ngrok-free.dev/")
            };

            // Bỏ qua trang cảnh báo của ngrok (gói miễn phí)
            client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");

            return client;
        }

        public static HttpClient Client => _client;
    }
}