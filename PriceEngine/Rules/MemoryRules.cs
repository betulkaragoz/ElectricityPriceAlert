using System.Text.Json.Serialization;

namespace PriceEngine.Rules;

public class StreakRule : RuleDefinition
{
    [JsonPropertyName("direction")]
    public string Direction { get; set; } = string.Empty;

    [JsonPropertyName("hours")]
    public int Hours { get; set; }

    public override bool Evaluate(EngineContext context)
    {
        // 3 streak için 4 saat verisi lazım
        int requiredPoints = Hours + 1;

        // yeterli saat verimiz yoksa başlarda vs false dön (toplam 4 adet olması lazım en başta)
        if (context.History.Count < requiredPoints)
        {
            return false;
        }

        for (int i = 0; i < Hours; i++)
        {
            var later = context.History[context.History.Count - 1 - i]; // saat 6 yı temsil etsin
            var earlier = context.History[context.History.Count - 2 - i]; // saat 5 i temsil etsin

            if (Direction == "up" && later.Price <= earlier.Price) return false; // yön ileri doğruyken saat 6 daki fiyat saat 5 teki fiyattan küçük ya da eşitse streakimiz bozulsun erkenden

            if (Direction == "down" && later.Price >= earlier.Price) return false; // yön geri doğruyken de döngü bozulacak şekilde kontrol edelim

        }
        return true;
    }
}

public class CooldownRule : RuleDefinition
{
    [JsonPropertyName("hours")]
    public int Hours { get; set; }

    [JsonPropertyName("rule")]
    public RuleDefinition? Rule { get; set; } // iç kural 

    public override bool Evaluate(EngineContext context)
    {
        if (Rule == null) return false; // iç kural null ise false 
        
        if (!Rule.Evaluate(context)) return false; // iç kural false ise false dön

        if (context.LastFiredTimes.TryGetValue(this, out var lastFired)) // daha önce bu kural çalıştırılmış mı kontrol et, çalmışsa lastFired değişkenine atar
        {
            var hoursSince = (context.Current.Timestamp - lastFired).TotalHours; // geçen süreyi hesapla
            if (hoursSince < Hours) 
                return false;
                
        }
        context.LastFiredTimes[this] = context.Current.Timestamp;
        return true;
    }
}

