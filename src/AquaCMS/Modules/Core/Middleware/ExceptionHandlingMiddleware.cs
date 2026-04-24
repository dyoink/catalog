using System.Net;

namespace AquaCMS.Modules.Core.Middleware;

/// <summary>
/// Global exception handler — bắt tất cả unhandled exceptions,
/// log lỗi chi tiết bằng Serilog, trả về error page thân thiện.
/// Ngăn app crash và leak stack trace ra client.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            // Handle 404 — redirect to error page nếu chưa bắt đầu write response
            if (context.Response.StatusCode == (int)HttpStatusCode.NotFound
                && !context.Response.HasStarted)
            {
                context.Response.Redirect("/Home/Error?code=404");
            }
        }
        catch (OperationCanceledException)
        {
            // Client hủy request — không cần log lỗi
            _logger.LogDebug("Request cancelled: {Path}", context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception — Method: {Method}, Path: {Path}, QueryString: {QueryString}, User: {User}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                context.User?.Identity?.Name ?? "anonymous");

            await HandleExceptionAsync(context);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Nếu là AJAX request → trả JSON
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest"
            || context.Request.Headers.Accept.ToString().Contains("application/json"))
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau."
            });
        }
        else
        {
            // Redirect to error page
            context.Response.Redirect("/Home/Error?code=500");
        }
    }
}

public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
