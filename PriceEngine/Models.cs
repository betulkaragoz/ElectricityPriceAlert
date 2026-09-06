using System.Data;
using System.Text.Json.Serialization;
using PriceEngine.Rules;
namespace PriceEngine.Models;

public class PriceData
{
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("prices")]
    public List<PricePoint> Prices { get; set; } = new();

}

public class PricePoint
{
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}

public class RuleSet 
{
    [JsonPropertyName("rules")]
    public List<RuleDefinition> Rules { get; set; } = new();
}
    


