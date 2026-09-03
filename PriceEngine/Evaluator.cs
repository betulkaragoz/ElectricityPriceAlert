using PriceEngine.Models;
using PriceEngine.Rules;
using System.Globalization;
using System.Timers;

namespace PriceEngine;

public class Evaluator
{
    private readonly List<RuleDefinition> _rules;

    public Evaluator(List<RuleDefinition> rules)
    {
        _rules = rules;
    }

    public void Process(List<PricePoint> prices)
    {
        var history = new List<PricePoint>(prices.Count);
        var lastFiredTimes = new Dictionary<RuleDefinition, DateTimeOffset>();

        for (int i = 0; i < prices.Count; i++)
        {
            var current = prices[i];
            var previous = i > 0 ? prices[i - 1] : null; // önceki fiyat ilk fiyat mı kontrol : i 0 dan büyük mü? büyükse -1 ile önceki fiyat, değilse null atadım

            history.Add(current); // şimdi okuduğumu geçmişe ekledim
            var context = new EngineContext(current, previous, history, lastFiredTimes);

            foreach (var rule in _rules)
            {
                if (rule.Evaluate(context))
                {
                    if (!string.IsNullOrEmpty(rule.Id) && !string.IsNullOrEmpty(rule.Message)) // iç kuralları yazdırmak istemiyoruz bu yüzden kontrol ettim id için
                    {
                        var timeStr = current.Timestamp.ToString("yyyy-MM-ddTHH:mm:sszzz");
                        var priceStr = current.Price.ToString("0.00", CultureInfo.InvariantCulture); // kültür farklılıkları ve tek çeşit fiyat basımı

                        Console.WriteLine($"[{timeStr}] {rule.Id}: {rule.Message} (price: {priceStr})");
                    }
                }
            }
        }
    }
}