using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware
{
    public class ValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationExceptionMiddleware> _logger;

        public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
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
            catch (ValidationException ex)
            {
                await HandleValidationExceptionAsync(context, ex);
            }
            catch (DomainException ex)
            {
                await HandleExceptionAsync(context, ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (KeyNotFoundException ex)
            {
                await HandleExceptionAsync(context, ex.Message, StatusCodes.Status404NotFound);
            }
            catch (ArgumentException ex)
            {
                await HandleExceptionAsync(context, ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (UnauthorizedAccessException ex)
            {
                await HandleExceptionAsync(context, ex.Message, StatusCodes.Status401Unauthorized);
            }
            catch (InvalidOperationException ex)
            {
                await HandleExceptionAsync(context, ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error on {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                var message = ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                    ? "A record with this value already exists. Please use a unique identifier."
                    : "A database error occurred. Please check your data and try again.";

                await HandleExceptionAsync(context, message, StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, "An unexpected error occurred.", StatusCodes.Status500InternalServerError);
            }
        }

        private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var response = new ApiResponse
            {
                Success = false,
                Message = "Validation Failed",
                Errors = exception.Errors
                    .Select(error => (ValidationErrorDetail)error)
            };

            return WriteResponseAsync(context, response);
        }

        private static Task HandleExceptionAsync(HttpContext context, string message, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = Enumerable.Empty<ValidationErrorDetail>()
            };

            return WriteResponseAsync(context, response);
        }

        private static Task WriteResponseAsync(HttpContext context, ApiResponse response)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}