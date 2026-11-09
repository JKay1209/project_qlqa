using System;
using System.Net.Http;

namespace Client.Network
{
    internal static class HttpClientManager
    {
        private static readonly HttpClient _client =
            new HttpClient()
            {
                BaseAddress =
                    new Uri("http://localhost:51280/")

            };

        public static HttpClient Client => _client;
    }
}