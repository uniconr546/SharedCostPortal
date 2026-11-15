using System.Net;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SharedCost.Domain.Exceptions;

namespace SharedCost.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException dex)
        {
            _logger.LogWarning(dex, "Vi phạm quy tắc nghiệp vụ: {ErrorCode} - {Message}", dex.ErrorCode, dex.Message);
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, dex.ErrorCode, dex.Message);
        }
        catch (DbUpdateException dbEx) when (dbEx.InnerException is SqlException sqlEx)
        {
            _logger.LogWarning(sqlEx, "SQL Server Trigger / Constraint từ chối thao tác: {Message}", sqlEx.Message);
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, "SQL_CONSTRAINT_OR_TRIGGER_VIOLATION", sqlEx.Message);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogWarning(sqlEx, "Lỗi SQL Server: {Message}", sqlEx.Message);
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, "SQL_SERVER_ERROR", sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi hệ thống không xác định");
            await WriteErrorResponseAsync(context, HttpStatusCode.InternalServerError, "INTERNAL_SERVER_ERROR", ex.Message);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode statusCode, string errorCode, string message)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new
        {
            errorCode,
            message,
            timestamp = DateTime.Now
        });

        await context.Response.WriteAsync(payload);
    }
}
