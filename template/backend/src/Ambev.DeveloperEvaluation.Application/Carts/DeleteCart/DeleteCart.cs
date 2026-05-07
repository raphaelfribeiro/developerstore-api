using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Carts.DeleteCart;

public record DeleteCartCommand(Guid Id) : IRequest<DeleteCartResult>;

public class DeleteCartValidator : AbstractValidator<DeleteCartCommand>
{
    public DeleteCartValidator()
    {
        RuleFor(c => c.Id).NotEmpty().WithMessage("Cart ID is required.");
    }
}

public class DeleteCartResult
{
    public bool Success { get; set; }
}

public class DeleteCartHandler : IRequestHandler<DeleteCartCommand, DeleteCartResult>
{
    private readonly ICartRepository _cartRepository;

    public DeleteCartHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<DeleteCartResult> Handle(DeleteCartCommand command, CancellationToken cancellationToken)
    {
        var validation = await new DeleteCartValidator().ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var success = await _cartRepository.DeleteAsync(command.Id, cancellationToken);
        if (!success)
            throw new KeyNotFoundException($"Cart with ID {command.Id} not found.");

        return new DeleteCartResult { Success = true };
    }
}
