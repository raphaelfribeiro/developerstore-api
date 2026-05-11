using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Products.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<DeleteProductResult>;

public class DeleteProductValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductValidator()
    {
        RuleFor(c => c.Id).NotEmpty().WithMessage("Product ID is required.");
    }
}

public class DeleteProductResult
{
    public bool Success { get; set; }
}

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, DeleteProductResult>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var validation = await new DeleteProductValidator().ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var success = await _productRepository.DeleteAsync(command.Id, cancellationToken);
        if (!success)
            throw new KeyNotFoundException($"Product with ID {command.Id} not found.");

        await _unitOfWork.CommitAsync(cancellationToken);
        return new DeleteProductResult { Success = true };
    }
}
