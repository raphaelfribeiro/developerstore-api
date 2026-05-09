using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class CartTestData
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Generates a valid Cart entity with a guaranteed non-empty Id.
    /// </summary>
    public static Cart GenerateValidCart()
    {
        var cart = new Cart
        {
            Id = Guid.NewGuid(), // explicit Id required outside EF context
            UserId = Guid.NewGuid(),
            Date = Faker.Date.Recent()
        };

        cart.AddItem(CartItem.Create(Guid.NewGuid(), Faker.Random.Int(1, 10)));
        return cart;
    }

    public static CartItem GenerateValidCartItem(int quantity = 3) =>
        CartItem.Create(Guid.NewGuid(), quantity);
}

public static class ProductTestData
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// Generates a valid Product entity with a guaranteed non-empty Id.
    /// </summary>
    public static Product GenerateValidProduct()
    {
        return new Product
        {
            Id = Guid.NewGuid(), // explicit Id required outside EF context
            Title = Faker.Commerce.ProductName(),
            Price = Faker.Random.Decimal(1m, 1000m),
            Description = Faker.Commerce.ProductDescription(),
            Category = Faker.Commerce.Department(),
            Image = Faker.Internet.Url(),
            Rating = new ProductRating
            {
                Rate = Faker.Random.Decimal(0m, 5m),
                Count = Faker.Random.Int(0, 1000)
            }
        };
    }
}
