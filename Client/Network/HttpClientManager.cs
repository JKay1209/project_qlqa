<<<<<<< HEAD
﻿// Một HttpClient dùng chung cho toàn ứng dụng (tránh tạo liên tục).
public static class HttpClientManager
{
    // Tạm thời ghi cứng; sau này đưa vào file cấu hình
    public const string BaseUrl = "http://localhost:51280/";

    public static HttpClient Instance { get; } = Create();

    private static HttpClient Create()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(15)
        };
        client.DefaultRequestHeaders.Accept.Add(new("application/json"));
        return client;
=======
﻿using System;
using System;
using System.Net.Http;

namespace Client.Network
{
    internal static class HttpClientManager
    {
        private static readonly HttpClient _client =
            new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:51280/")
            };

        public static HttpClient Client => _client;
>>>>>>> c086b91 (Hoan thanh giao dien login)
    }
}