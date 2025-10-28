using Bogus;

namespace Product.UnitTest.Fixtures;


public class ProductTestFixture
{
    private readonly Faker _faker;

    public ProductTestFixture()
    {
        _faker = new Faker();
    }


    public string GenerateProductName() => _faker.Commerce.ProductName();


    public string GenerateDescription() => _faker.Commerce.ProductDescription();


    public decimal GenerateValidPrice(decimal min = 1, decimal max = 10000) =>
        _faker.Finance.Amount(min, max);

  
    public int GenerateValidStock(int min = 0, int max = 1000) =>
        _faker.Random.Int(min, max);

 
    public string GenerateSku() =>
        $"{_faker.Commerce.Categories(1)[0].ToUpper().Substring(0, 3)}-{_faker.Random.Int(1000, 9999)}";


    public Guid GenerateCategoryId() => Guid.NewGuid();

 
    public Core.Domain.Product CreateValidProduct(
        string? name = null,
        string? description = null,
        decimal? price = null,
        int? stock = null,
        string? sku = null,
        Guid? categoryId = null)
    {
        var result = Core.Domain.Product.Create(
            name ?? GenerateProductName(),
            description ?? GenerateDescription(),
            price ?? GenerateValidPrice(),
            stock ?? GenerateValidStock(),
            sku ?? GenerateSku(),
            categoryId ?? GenerateCategoryId()
        );

        return result.Value;
    }

    /// <summary>
    /// Genera più prodotti validi
    /// </summary>
    public List<Core.Domain.Product> CreateMultipleProducts(int count)
    {
        var products = new List<Core.Domain.Product>();
        var categoryId = GenerateCategoryId(); // Usa la stessa categoria per tutti

        for (int i = 0; i < count; i++)
        {
            products.Add(CreateValidProduct(categoryId: categoryId));
        }

        return products;
    }

    /// <summary>
    /// Genera dati invalidi per testare i casi di errore
    /// </summary>
    public class InvalidData
    {
        public static IEnumerable<object[]> InvalidNames()
        {
            yield return new object[] { "" };
            yield return new object[] { "   " };
            yield return new object[] { null! };
        }

        public static IEnumerable<object[]> InvalidSkus()
        {
            yield return new object[] { "" };
            yield return new object[] { "   " };
            yield return new object[] { null! };
        }

        public static IEnumerable<object[]> InvalidPrices()
        {
            yield return new object[] { 0m };
            yield return new object[] { -1m };
            yield return new object[] { -100m };
        }

        public static IEnumerable<object[]> InvalidStocks()
        {
            yield return new object[] { -1 };
            yield return new object[] { -100 };
        }
    }
}