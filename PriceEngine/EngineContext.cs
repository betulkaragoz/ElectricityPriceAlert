using PriceEngine.Models;
using PriceEngine.Rules;

namespace PriceEngine;

public class EngineContext
{
    public PricePoint Current { get; }
    public PricePoint ? Previous { get; }
    public IReadOnlyList<PricePoint> History { get; }

    public Dictionary<RuleDefinition, DateTimeOffset> LastFiredTimes { get; }

    public EngineContext(
        PricePoint current,
        PricePoint? previous,
        IReadOnlyList<PricePoint> history,
        Dictionary<RuleDefinition, DateTimeOffset> lastFiredTimes)

    {
        Current = current;
        Previous = previous;
        History = history;
        LastFiredTimes = lastFiredTimes;

    }

}