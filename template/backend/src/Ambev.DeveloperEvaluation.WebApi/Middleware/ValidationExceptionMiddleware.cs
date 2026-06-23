using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware
{
    public class ValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
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
                await HandleDomainExceptionAsync(context, ex);
            }
            catch (KeyNotFoundException ex)
            {
                await HandleKeyNotFoundExceptionAsync(context, ex);
            }
        }

        private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = "Validation Failed",
                Errors = exception.Errors
                    .Select(error => (ValidationErrorDetail)error)
            };

            return WriteResponseAsync(context, response);
        }

        private static Task HandleDomainExceptionAsync(HttpContext context, DomainException exception)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = "Validation Failed",
                Errors = new[]
                {
                    new ValidationErrorDetail
                    {
                        Type = "DomainValidation",
                        Error = "Business rule violation",
                        Detail = exception.Message
                    }
                }
            };

            return WriteResponseAsync(context, response);
        }

        private static Task HandleKeyNotFoundExceptionAsync(HttpContext context, KeyNotFoundException exception)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = exception.Message
            };

            return WriteResponseAsync(context, response, StatusCodes.Status404NotFound);
        }

        private static Task WriteResponseAsync(HttpContext context, ApiResponse response, int statusCode = StatusCodes.Status400BadRequest)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
