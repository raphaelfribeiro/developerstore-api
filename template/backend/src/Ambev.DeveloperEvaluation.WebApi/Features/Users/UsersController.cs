using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.GetUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.GetUsers;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.DeleteUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.UpdateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.PatchUserRole;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUsers;
using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Application.Users.UpdateUser;
using Ambev.DeveloperEvaluation.Application.Users.PatchUserRole;
using Ambev.DeveloperEvaluation.Application.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users;

/// <summary>
/// Controller for managing user accounts.
/// Handles user creation, retrieval, profile updates, role management, and deletion.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public UsersController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new user account. No authentication required.
    /// </summary>
    /// <param name="request">User registration data including email, username, password, name, address and role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created user with its generated ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateUserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var validator = new CreateUserRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.First().ErrorMessage);

        var command = _mapper.Map<CreateUserCommand>(request);
        var response = await _mediator.Send(command, cancellationToken);
        var userResponse = _mapper.Map<CreateUserResponse>(response);

        return base.Created($"/api/users/{userResponse.Id}", new ApiResponseWithData<CreateUserResponse>
        {
            Success = true,
            Message = "User created successfully",
            Data = userResponse
        });
    }

    /// <summary>
    /// Returns a paginated list of all users. Requires authentication.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="size">Items per page (default: 10).</param>
    /// <param name="order">Ordering expression, e.g. "username asc, email desc".</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of users with total count and page metadata.</returns>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<GetUsersResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery(Name = "_page")] int page = 1,
        [FromQuery(Name = "_size")] int size = 10,
        [FromQuery(Name = "_order")] string? order = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersQuery { Page = page, Size = size, Order = order };
        var result = await _mediator.Send(query, cancellationToken);
        var mapped = _mapper.Map<List<GetUsersResponse>>(result.Data);

        return OkPaginated(new PaginatedResult<GetUsersResponse>(
            mapped, result.TotalCount, result.CurrentPage, result.PageSize));
    }

    /// <summary>
    /// Returns a single user by ID. Requires authentication.
    /// </summary>
    /// <param name="id">The user's unique identifier (GUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Full user profile including name, address, role and status.</returns>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var request = new GetUserRequest { Id = id };
        var validator = new GetUserRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.First().ErrorMessage);

        var command = _mapper.Map<GetUserCommand>(request.Id);
        var response = await _mediator.Send(command, cancellationToken);

        return Ok(_mapper.Map<GetUserResponse>(response), "User retrieved successfully");
    }

    /// <summary>
    /// Updates a user's full profile. Requires authentication.
    /// Only the account owner or an Admin can update a profile.
    /// The <c>role</c> and <c>status</c> fields are accepted per spec but only Admins can change them;
    /// non-admin callers who send a different value receive 403 Forbidden.
    /// </summary>
    /// <param name="id">The user's unique identifier (GUID).</param>
    /// <param name="request">Updated profile data. Omit <c>role</c>/<c>status</c> to keep current values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user profile.</returns>
    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("Request body is required.");

        var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && (!Guid.TryParse(callerId, out var callerGuid) || callerGuid != id))
            return Forbid();

        var validator = new UpdateUserRequestValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.First().ErrorMessage);

        var command = _mapper.Map<UpdateUserCommand>(request);
        command.Id = id;
        command.CallerIsAdmin = isAdmin;

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(_mapper.Map<UpdateUserResponse>(result), "User updated successfully");
    }

    /// <summary>
    /// Changes the role and status of any user. Restricted to Admin accounts only.
    /// </summary>
    /// <param name="id">The target user's unique identifier (GUID).</param>
    /// <param name="request">New role and status values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's updated role and status.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/role")]
    [ProducesResponseType(typeof(ApiResponseWithData<PatchUserRoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchUserRole(
        [FromRoute] Guid id,
        [FromBody] PatchUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<PatchUserRoleCommand>(request);
        command.Id = id;

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(_mapper.Map<PatchUserRoleResponse>(result), "User role updated successfully");
    }

    /// <summary>
    /// Permanently deletes a user by ID. Requires authentication.
    /// </summary>
    /// <param name="id">The user's unique identifier (GUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Confirmation message on success.</returns>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var request = new DeleteUserRequest { Id = id };
        var validator = new DeleteUserRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.First().ErrorMessage);

        var command = _mapper.Map<DeleteUserCommand>(request.Id);
        await _mediator.Send(command, cancellationToken);

        return Ok(new { }, "User deleted successfully");
    }
}
