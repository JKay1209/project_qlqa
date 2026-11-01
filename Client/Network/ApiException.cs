using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Client.Network;

// Server trả mã lỗi HTTP (400, 401, 500...).
public class ApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

// Không kết nối được tới server (tắt server, sai địa chỉ, mất mạng).
public class ApiConnectionException(Exception inner)
    : Exception("Không kết nối được tới server.", inner);

// Hết thời gian chờ phản hồi.
public class ApiTimeoutException() : Exception("Hết thời gian chờ server.");

