using System.Text.Json.Serialization;

namespace PriceEngine.Rules;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ThresholdRule), "threshold")]
[JsonDerivedType(typeof(ChangeRule), "change")]
[JsonDerivedType(typeof(RangeRule), "range")]
[JsonDerivedType(typeof(AndRule), "and")]
[JsonDerivedType(typeof(OrRule), "or")]
[JsonDerivedType(typeof(NotRule), "not")]
[JsonDerivedType(typeof(StreakRule), "streak")]
[JsonDerivedType(typeof(CooldownRule), "cooldown")]

public abstract class RuleDefinition
{
    [JsonPropertyName("id")]
    public string ? Id { get; set; }

    [JsonPropertyName("message")]
    public string ? Message { get; set; }

    public abstract bool Evaluate(EngineContext context);
}