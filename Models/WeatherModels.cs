using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WeatherAppAvalonia.Models;

public class WttrResponse
{
    [JsonPropertyName("current_condition")]
    public List<CurrentCondition>? CurrentCondition { get; set; }

    [JsonPropertyName("nearest_area")]
    public List<NearestArea>? NearestArea { get; set; }
}

public class CurrentCondition
{
    [JsonPropertyName("temp_C")]
    public string? TempC { get; set; }

    [JsonPropertyName("weatherDesc")]
    public List<NameValue>? WeatherDesc { get; set; }
}

public class NearestArea
{
    [JsonPropertyName("areaName")]
    public List<NameValue>? AreaName { get; set; }
}

public class NameValue
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
