using System.Text.Json.Serialization;

namespace PI5.entities;

public class RetornoAPI{
    [JsonPropertyName("results")]
    public List<Result> Results {get;set;}

}
public class Result{
    //Pegando os atributos referente as açoes na API
    [JsonPropertyName("longName")]
    public string LongName{get;set;}

    [JsonPropertyName("symbol")]
    public string Symbol{get;set;}

    [JsonPropertyName("logourl")]
    public string Logourl{get;set;}

    [JsonPropertyName("historicalDataPrice")]
    public List<HistoricalDataPrice> HistoricalDataPrice {get;set;}
}
public class HistoricalDataPrice{

    [JsonPropertyName("date")]
    public long Data{get;set;}

    [JsonPropertyName("close")]
    public float Close{get;set;}

    [JsonPropertyName("open")]
    public float Open{get;set;}

    [JsonPropertyName("high")]
    public float High{get;set;}

    [JsonPropertyName("low")]
    public float Low{get;set;}
}

