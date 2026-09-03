using System.Text.Json.Serialization;

namespace PriceEngine.Rules;

public class ThresholdRule : RuleDefinition
{
    [JsonPropertyName("operator")]
    public string Operator { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    public override bool Evaluate(EngineContext context)
    {
        return Operator switch
        {
            "gt" => context.Current.Price > Value,
            "lt" => context.Current.Price < Value,
            _ => false
        };
    }
}

public class ChangeRule : RuleDefinition
{
    [JsonPropertyName("percent")]
    public decimal Percent { get; set; }
    public override bool Evaluate(EngineContext context)
    {
        if (context.Previous == null || context.Previous.Price == 0)
        {
            return false;
        }

        var diff = Math.Abs(context.Current.Price - context.Previous.Price);
        var changePercent = (diff / context.Previous.Price) * 100m;
        
        return changePercent >= Percent;
    }
}

public class RangeRule : RuleDefinition
{
    [JsonPropertyName("min")]
    public decimal Min { get; set; }

    [JsonPropertyName("max")]
    public decimal Max { get; set; }
    
    public override bool Evaluate(EngineContext context)
    {
        return context.Current.Price < Min && context.Current.Price > Max;
    }
}