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
        : null;

        var history = new List<PricePoint>();
        if (previous != null) history.Add(previous);
        history.Add(current);

        return new EngineContext(current, previous, history, new Dictionary<RuleDefinition, DateTimeOffset>());
    }

    [Fact]
    public void ThresholdRule_Gt_Mathes_Correctly()
    // Fiyatın belirlenen eşik değerin üstüne çıktığında ('gt' operatörü) kuralın doğru şekilde tetiklenip tetiklenmediğini test eder.
    // 3500 için alarm vermesini (True), 2500 için vermemesini (False) bekler.
    {
        var rule = new ThresholdRule { Operator = "gt", Value = 3000 };
        Assert.True(rule.Evaluate(CreateContext(3500)));
        Assert.False(rule.Evaluate(CreateContext(2500)));
    }

    [Fact]
    public void ChangeRule_Matches_On_Jump() 
    // Saatteki yüzde değişim (sıçrama) mantığını test eder. 
    // Önceki saat 2000 iken sonraki saat 2500 olduğunda %20'lik artışı yakalayıp True dönmesini;
    // küçük değişimlerde veya ilk saat olduğu için önceki verinin null geldiği senaryolarda yanlış alarm üretmeyip False dönmesini güvence altına alır.
    {
        var rule = new ChangeRule { Percent = 20 };
        Assert.True(rule.Evaluate(CreateContext(2500, 2000)));
        Assert.False(rule.Evaluate(CreateContext(2100, 2000)));
        Assert.False(rule.Evaluate(CreateContext(2000, null)));
    }

    [Fact]
    public void ThresholdRule_Lt_Matches_Correctly()
    // Fiyatın belirlenen eşik değerin altına indiğinde ('lt' operatörü) kuralın doğru çalışıp çalışmadığını test eder.
    // 2500 için alt sınır kuralına uyduğu için True, 3500 için False dönmelidir.
    {
        var rule = new ThresholdRule { Operator = "lt", Value = 3000 };
        Assert.True(rule.Evaluate(CreateContext(2500)));
        Assert.False(rule.Evaluate(CreateContext(3500)));
    }

    [Fact]
    public void ThresholdRule_Exact_Boundary_Behavior()
    // Sınır değer (boundary) durumunu test eder. 
    // Fiyat tam eşik değerine (3000) eşit olduğunda, 'gt' (büyüktür) kuralının alarm üretmeyip False dönmesi gerektiğini doğrular.
    {
        var rule = new ThresholdRule { Operator = "gt", Value = 3000 };
        Assert.False(rule.Evaluate(CreateContext(3000)));
    }

    [Fact]
    public void ChangeRule_Matches_On_Drop()
    // Yüzde değişim kuralının negatif yönlü (düşüş) hareketleri doğru yakaladığını test eder.
    // Fiyatın 2000'den 1500'e düşmesi %25'lik bir değişim yarattığı için kuralın eşleşip True dönmesini bekler.
    {
        var rule = new ChangeRule { Percent = 20 };
        Assert.True(rule.Evaluate(CreateContext(1500, 2000)));
    }

    [Fact]
    public void RangeRule_Evaluates_Inside_And_Outside_Correctly()
    // Aralık (Range) kuralının belirlenen [min, max] bandının içini ve dışını doğru ayırt edip etmediğini test eder.
    // Bandın dışındaki fiyatlar (3500 ve 500) alarm üretirken (True), bandın içindeki fiyat (2000) alarm üretmez (False).
    {
        var rule = new RangeRule { Min = 1000, Max = 3000 };
        
        // Bandın dışı (Alarm üretmeli)
        Assert.True(rule.Evaluate(CreateContext(3500)));
        Assert.True(rule.Evaluate(CreateContext(500)));
        
        // Bandın içi (Alarm üretmemeli)
        Assert.False(rule.Evaluate(CreateContext(2000)));
    }

    [Fact]
    public void AndRule_Requires_All_Conditions_To_Match()
    // Bileşik kurallardan biri olan 'And' (ve) mantığını test eder. İçindeki tüm alt kuralların aynı anda sağlanmasını şart koşar.
    // Hem fiyatın 2500'den büyük olması hem de %10'dan fazla değişim göstermesi durumunda True döner; şartlardan biri eksikse False döner.
    {
        var andRule = new AndRule
        {
            Rules = new List<RuleDefinition>
            {
                new ThresholdRule { Operator = "gt", Value = 2500 },
                new ChangeRule { Percent = 10 }
            }
        };

        // Hem fiyat > 2500 hem de %10'dan fazla değişim olmalı
        var contextMatching = CreateContext(3000, 2500); // Fiyat 2500->3000 (%20 değişim ve >2500)
        Assert.True(andRule.Evaluate(contextMatching));

        var contextNotMatching = CreateContext(2600, 2550); // Fiyat > 2500 ama değişim çok küçük
        Assert.False(andRule.Evaluate(contextNotMatching));
    }
}