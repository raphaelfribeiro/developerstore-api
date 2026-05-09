using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Provides methods for generating test data for Sale and SaleItem entities using the Bogus library.
/// </summary>
public static class SaleTestData
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Generates a valid Sale entity with a guaranteed non-empty Id.
    /// </summary>
    public static Sale GenerateValidSale(int quantity = 5, decimal unitPrice = 100m)
    {
        var sale = new Sale
        {
            Id = Guid.NewGuid(), // explicit Id required outside EF context
            SaleNumber = $"SALE-{Faker.Random.Number(1000, 9999)}",
            SaleDate = Faker.Date.Recent(),
            CustomerId = Guid.NewGuid(),
            CustomerName = Faker.Person.FullName,
            BranchId = Guid.NewGuid(),
            BranchName = Faker.Company.CompanyName()
        };

        var item = GenerateValidSaleItem(quantity, unitPrice);
        sale.AddItem(item);
        return sale;
    }

    /// <summary>
    /// Generates a valid SaleItem with explicit Id.
    /// </summary>
    public static SaleItem GenerateValidSaleItem(int quantity = 5, decimal unitPrice = 100m)
    {
        return new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = Faker.Commerce.ProductName(),
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    public static SaleItem GenerateItemBelowDiscountThreshold() =>
        GenerateValidSaleItem(quantity: Faker.Random.Int(1, 3));

    public static SaleItem GenerateItemInTenPercentTier() =>
        GenerateValidSaleItem(quantity: Faker.Random.Int(4, 9));

    public static SaleItem GenerateItemInTwentyPercentTier() =>
        GenerateValidSaleItem(quantity: Faker.Random.Int(10, 20));

    public static SaleItem GenerateItemExceedingMaxQuantity() =>
        GenerateValidSaleItem(quantity: Faker.Random.Int(21, 50));
}
