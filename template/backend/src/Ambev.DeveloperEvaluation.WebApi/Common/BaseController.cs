using Ambev.DeveloperEvaluation.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected IActionResult Ok<T>(T data) =>
        base.Ok(new ApiResponseWithData<T> { Data = data, Success = true });

    protected IActionResult Ok<T>(T data, string message) =>
        base.Ok(new ApiResponseWithData<T> { Data = data, Success = true, Message = message });

    protected IActionResult BadRequest(string message) =>
        base.BadRequest(new ApiErrorResponse { Type = "ValidationError", Error = "Invalid request", Detail = message });

    protected IActionResult NotFound(string message = "Resource not found") =>
        base.NotFound(new ApiErrorResponse { Type = "ResourceNotFound", Error = "Resource not found", Detail = message });

    protected IActionResult OkPaginated<T>(PaginatedResult<T> result) =>
        base.Ok(new PaginatedResponse<T>
        {
            Data = result.Data,
            CurrentPage = result.CurrentPage,
            TotalPages = result.TotalPages,
            TotalItems = result.TotalCount,
            Success = true
        });
}
