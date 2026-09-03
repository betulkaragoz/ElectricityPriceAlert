using System.Text.Json;
using PriceEngine;
using PriceEngine.Models;

if (args.Length < 2)
{
    Console.WriteLine("Usage: PriceConsole <prices.json> <rules.json>");
    return;
}

string pricesPath = args[0];
string rulesPath = args[1];

if ( !File.Exists(pricesPath) || !File.Exists(rulesPath) )
{
    Console.WriteLine("Dosyalar bulunamadı. -Program.cs");
    return;
}

try
{
    var options = new JsonSerializerOptions{ PropertyNameCaseInsensitive = true,
        AllowOutOfOrderMetadataProperties = true 
    };

    var priceData = JsonSerializer.Deserialize<PriceData>(File.ReadAllText(pricesPath), options);
    var ruleSet = JsonSerializer.Deserialize<RuleSet>(File.ReadAllText(rulesPath), options);

    if (priceData?.Prices == null || ruleSet?.Rules == null)
    {
        Console.WriteLine("JSON verisi okunamadı. -Program.cs");
        return;
    }

    var evaluator = new Evaluator(ruleSet.Rules);
    evaluator.Process(priceData.Prices);
}
catch (Exception ex)
{
    Console.WriteLine($"Hata oluştu: {ex.Message} -Program.cs");

}