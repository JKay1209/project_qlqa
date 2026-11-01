namespace Shared.Constants;

public static class ApiRoutes
{
    public const string Auth = "/api/auth";
    public const string Login = Auth + "/login";
    public const string Me = Auth + "/me";

    public const string Tables = "/api/tables";
    public const string Menu = "/api/menu";
    public const string Orders = "/api/orders";
    public const string Payments = "/api/payments";
    public const string Employees = "/api/employees";
}
