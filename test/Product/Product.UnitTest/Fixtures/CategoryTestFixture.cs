using Bogus;
using Product.Core.Domain;

namespace Product.UnitTest.Fixtures;


public class CategoryTestFixture
{
    private readonly Faker _faker;

    public CategoryTestFixture()
    {
        _faker = new Faker();
    }

 
    public string GenerateCategoryName() => _faker.Commerce.Categories(1)[0];


    public string GenerateDescription() => _faker.Lorem.Sentence();


    public Category CreateValidCategory(
        string? name = null,
        string? description = null)
    {
        var result = Category.Create(
            name ?? GenerateCategoryName(),
            description ?? GenerateDescription()
        );

        return result.Value;
    }


    public List<Category> CreateMultipleCategories(int count)
    {
        var categories = new List<Category>();

        for (int i = 0; i < count; i++)
        {
            categories.Add(CreateValidCategory());
        }

        return categories;
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
    }
}