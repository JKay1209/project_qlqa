using Shared.DTOs.Auth;

namespace Server.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
        {
            // client tự hủy request, không phải lỗi
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi không xử lý: {Method} {Path}", ctx.Request.Method, ctx.Request.Path);

            if (ctx.Response.HasStarted) throw;
            ctx.Response.Clear();
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(new ErrorResponse("Lỗi hệ thống. Vui lòng thử lại sau."));
        }
    }
}
