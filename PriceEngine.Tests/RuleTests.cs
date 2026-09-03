using PriceEngine;
using PriceEngine.Models;
using PriceEngine.Rules;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace PriceEngine.Tests;

public class RuleTests
{
    private EngineContext CreateContext(decimal currentPrice, decimal? previousPrice = null)
    {
        var current = new PricePoint { Timestamp = DateTimeOffset.UtcNow, Price = currentPrice };
        var previous = previousPrice.HasValue ?  
        new PricePoint { Timestamp = DateTimeOffset.UtcNow.AddHours(-1), Price = previousPrice.Value }
        :null;

        var history = new List<PricePoint>();
        if (previous != null) history.Add(previous);
        history.Add(current);

        return new EngineContext(current, previous, history, new Dictionary<RuleDefinition, DateTimeOffset>());
    }
    [Fact]
    public void ThresholdRule_Gt_Mathes_Correctly()
    {
        var rule = new ThresholdRule { Operator = "gt", Value = 3000 };
        Assert.True(rule.Evaluate(CreateContext(3500)));
        Assert.False(rule.Evaluate(CreateContext(2500)));
    }
    [Fact]
    public void ChangeRule_Matches_On_Jump()
    {
        var rule = new ChangeRule { Percent = 20 };
        Assert.True(rule.Evaluate(CreateContext(2500, 2000)));
        Assert.False(rule.Evaluate(CreateContext(2100, 2000)));
        Assert.False(rule.Evaluate(CreateContext(2000, null)));
    }
}