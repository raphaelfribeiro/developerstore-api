using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

/// <summary>
/// Provides methods for generating test data for CreateSaleHandler tests.
/// </summary>
public static class CreateSaleHandlerTestData
{
    private static readonly Faker Faker = new();

    private static readonly Faker<CreateSaleItemCommand> ItemFaker = new Faker<CreateSaleItemCommand>()
        .RuleFor(i => i.ProductId, _ => Guid.NewGuid())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 9))
        .RuleFor(i => i.UnitPrice, f => f.Random.Decimal(10m, 500m));

    private static readonly Faker<CreateSaleCommand> CommandFaker = new Faker<CreateSaleCommand>()
        .RuleFor(s => s.SaleNumber, f => $"SALE-{f.Random.Number(1000, 9999)}")
        .RuleFor(s => s.SaleDate, f => f.Date.Recent())
        .RuleFor(s => s.CustomerId, _ => Guid.NewGuid())
        .RuleFor(s => s.CustomerName, f => f.Person.FullName)
        .RuleFor(s => s.BranchId, _ => Guid.NewGuid())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName())
        .RuleFor(s => s.Items, _ => ItemFaker.Generate(1));

    /// <summary>
    /// Generates a valid CreateSaleCommand with randomized data.
    /// </summary>
    public static CreateSaleCommand GenerateValidCommand() => CommandFaker.Generate();

    /// <summary>
    /// Generates a CreateSaleCommand with items in the 10% discount tier (4-9 items).
    /// </summary>
    public static CreateSaleCommand GenerateCommandWithTenPercentDiscount()
    {
        var command = CommandFaker.Generate();
        command.Items = new List<CreateSaleItemCommand>
        {
            new() { ProductId = Guid.NewGuid(), ProductName = "Product A", Quantity = 5, UnitPrice = 100m }
        };
        return command;
    }

    /// <summary>
    /// Generates a CreateSaleCommand with items in the 20% discount tier (10-20 items).
    /// </summary>
    public static CreateSaleCommand GenerateCommandWithTwentyPercentDiscount()
    {
        var command = CommandFaker.Generate();
        command.Items = new List<CreateSaleItemCommand>
        {
            new() { ProductId = Guid.NewGuid(), ProductName = "Product A", Quantity = 10, UnitPrice = 100m }
        };
        return command;
    }

    /// <summary>
    /// Generates a CreateSaleCommand with items exceeding the maximum allowed quantity (> 20).
    /// </summary>
    public static CreateSaleCommand GenerateCommandWithExceedingQuantity()
    {
        var command = CommandFaker.Generate();
        command.Items = new List<CreateSaleItemCommand>
        {
            new() { ProductId = Guid.NewGuid(), ProductName = "Product A", Quantity = 21, UnitPrice = 100m }
        };
        return command;
    }
}
