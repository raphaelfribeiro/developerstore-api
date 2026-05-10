using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UpdateUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateUserHandler(IUserRepository userRepository, IMapper mapper, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<UpdateUserResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {request.Id} not found");

        if (!request.CallerIsAdmin)
        {
            if (request.Role.HasValue && request.Role.Value != user.Role)
                throw new ForbiddenException("Only admins can change the user role");
            if (request.Status.HasValue && request.Status.Value != user.Status)
                throw new ForbiddenException("Only admins can change the user status");
        }

        user.Username = request.Username;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.City = request.City;
        user.Street = request.Street;
        user.Number = request.Number;
        user.ZipCode = request.ZipCode;
        user.GeoLat = request.GeoLat;
        user.GeoLong = request.GeoLong;
        user.UpdatedAt = DateTime.UtcNow;

        if (request.Role.HasValue)
            user.Role = request.Role.Value;
        if (request.Status.HasValue)
            user.Status = request.Status.Value;

        if (!string.IsNullOrWhiteSpace(request.Password))
            user.Password = _passwordHasher.HashPassword(request.Password);

        var updated = await _userRepository.UpdateAsync(user, cancellationToken);
        return _mapper.Map<UpdateUserResult>(updated);
    }
}
