namespace Shared.Constants;

public static class ApiRoutes
{
    public const string Auth = "/api/auth";
    public const string Login = Auth + "/login";
    public const string Register = Auth + "/register";
    public const string ForgotPassword = Auth + "/forgot-password";
    public const string VerifyOtp = Auth + "/verify-otp";
    public const string ResetPassword = Auth + "/reset-password";
    public const string Me = Auth + "/me";

    public const string Tables = "/api/tables";
    public const string Menu = "/api/menu";
    public const string Orders = "/api/orders";
    public const string Payments = "/api/payments";
    public const string Employees = "/api/employees";
}
