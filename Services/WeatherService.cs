using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherAppAvalonia.Models;
using WeatherAppAvalonia.Helpers;

namespace WeatherAppAvalonia.Services;
public class WeatherService
{
    private static readonly HttpClient httpClient = new();
    private readonly LruCache<string, (WttrResponse Data, DateTime UpdatedAt)> cache = new(30);

    public async Task<WttrResponse?> GetWeatherAsync(string? city, bool force = false)
    {
        string cityKey = string.IsNullOrWhiteSpace(city) ? "auto" : city.Trim().ToLowerInvariant();

        if (!force && cache.TryGet(cityKey, out var cachedEntry))
        {
            if ((DateTime.Now - cachedEntry.UpdatedAt).TotalMinutes < 10)
                return cachedEntry.Data;
        }

        string url = "https://wttr.in/" + (string.IsNullOrEmpty(city) ? "" : Uri.EscapeDataString(city)) + "?format=j1";

        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<WttrResponse>(json);

        if (data != null)
            cache.Set(cityKey, (data, DateTime.Now));

        return data;
    }
}