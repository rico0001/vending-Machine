using System.Text.Json;
using System.Text.Json.Serialization;

namespace CS2S564VendingMachine.Core;

public static class ProductCatalog
{
    public static IReadOnlyList<Product> CreateDefault() => new List<Product>
    {
        new("cola", "Coca-Cola", 120, "coca-cola.jpg"),
        new("pepsi", "Pepsi", 115, "pepsi.jpg"),
        new("water", "Bottled Water", 90, "water.jpg"),
        new("juice", "Orange Juice", 150, "orange-juice.jpg"),
        new("chocolate", "Chocolate Bar", 85, "chocolate.jpg"),
        new("crisps", "Crisps", 75, "crisps.jpg")
    };

    public static IReadOnlyList<Product> LoadFromJsonOrDefault(string jsonPath)
    {
        try
        {
            if (!File.Exists(jsonPath))
            {
                return CreateDefault();
            }

            var json = File.ReadAllText(jsonPath);
            var records = JsonSerializer.Deserialize<List<ProductRecord>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            if (records is null || records.Count == 0)
            {
                return CreateDefault();
            }

            return records.Select(record => new Product(
                record.Id,
                record.Name,
                record.UnitPricePence,
                record.ImageFileName)).ToList();
        }
        catch (Exception)
        {
            return CreateDefault();
        }
    }

    private sealed class ProductRecord
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("unitPricePence")]
        public int UnitPricePence { get; set; }

        [JsonPropertyName("imageFileName")]
        public string ImageFileName { get; set; } = "";
    }
}
