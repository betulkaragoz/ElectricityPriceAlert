using System.Text.Json.Serialization;

namespace PriceEngine.Rules;

public class AndRule : RuleDefinition 
{
    [JsonPropertyName("rules")]
    public List<RuleDefinition> Rules { get; set; } = new();

    public override bool Evaluate(EngineContext context)
    {
        if (Rules.Count == 0) return false;
        foreach (var rule in Rules) {
            if (!rule.Evaluate(context)) return false; // 1 kural bile eşleşmezse false döndürür döngü biter
        }
        return true;
    }
}

public class OrRule : RuleDefinition
{
    [JsonPropertyName("rules")]
    public List<RuleDefinition> Rules { get; set; } = new();
    public override bool Evaluate(EngineContext context)
    {
        if (Rules.Count == 0) return false;
        foreach (var rule in Rules)
        {
            if (rule.Evaluate(context)) return true; // 1 kurak bile eşleşirse true döndürür
        }
        return false;
    }
}

public class NotRule : RuleDefinition
{
    [JsonPropertyName("rule")]
    public RuleDefinition? Rule { get; set; } = null!;
    public override bool Evaluate(EngineContext context)
    {
        return !Rule!.Evaluate(context); // kuralın sonucunu tersine çevirir t=>f f=>t
    }
}