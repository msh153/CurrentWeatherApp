// MainWindow.axaml.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Platform;

namespace WeatherAppAvalonia
{
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

    public partial class MainWindow : Window
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private DateTime lastUpdate = DateTime.MinValue;
        private string? lastCity = string.Empty;
        
        public MainWindow()
        {
            InitializeComponent();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(10) };
            timer.Tick += async (_, _) =>
            {
                await FetchWeatherData(lastCity, force: false);
            };
            timer.Start();
        }

        private async void OnRefreshButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            lastCity = CityTextBox.Text;
            await FetchWeatherData(lastCity, force: true);
        }

        private void CityTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnRefreshButtonClick(this, new Avalonia.Interactivity.RoutedEventArgs());
            }
        }

        private async Task FetchWeatherData(string? city, bool force = false)
        {
            if (!force && lastUpdate != DateTime.MinValue && (DateTime.Now - lastUpdate).TotalMinutes < 10)
                return;

            StatusBlock.Text = string.Empty;

            string cityInput =  CityTextBox?.Text?.Trim();
            Console.WriteLine(cityInput);
            if (string.IsNullOrWhiteSpace(cityInput))
            {
                StatusBlock.Text = "Виконується автоматичне визначення за IP…";
            }
            else
            {
                StatusBlock.Text = "";
            }
            
            string url = "https://wttr.in/" + (string.IsNullOrEmpty(cityInput) ? "" : $"{Uri.EscapeDataString(cityInput)}") + "?format=j1";

            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); 
                var json = await response.Content.ReadAsStringAsync();

                var data = JsonSerializer.Deserialize<WttrResponse>(json);
                if (data == null || data.CurrentCondition == null || data.CurrentCondition.Count == 0)
                {
                    throw new JsonException("Неправильний формат відповіді API");
                }

                // Отримуємо назву міста з nearest_area
                string? cityName = data.NearestArea?[0]?.AreaName?[0]?.Value;
                if (string.IsNullOrEmpty(cityName))
                {
                    StatusBlock.Text = "Місто не знайдено у відповіді API.";
                    return;
                }

                if (!string.IsNullOrWhiteSpace(cityInput) &&
                    !string.Equals(cityInput, cityName, StringComparison.OrdinalIgnoreCase))
                {
                    StatusBlock.Text = $"Місто '{cityInput}' не знайдено. Показано '{cityName}'.";
                }

                CityNameBlock.Text = cityName;
                TempBlock.Text = $"{data.CurrentCondition[0].TempC} °C";

                string? descEn = data.CurrentCondition[0].WeatherDesc?[0]?.Value;
                DescBlock.Text = LocalizeDescription(descEn ?? "Відповідь від сервера відсутня");

                if (descEn != null && IconMap.TryGetValue(descEn, out string iconName))
                {
                    var uri = new Uri($"avares://WeatherAppAvalonia/Assets/WeatherIcons/{iconName}.png");

                    if (AssetLoader.Exists(uri))
                    {
                        var stream = AssetLoader.Open(uri);
                        WeatherIcon.Source = new Bitmap(stream);
                    }
                    else
                    {
                        WeatherIcon.Source = new Bitmap($"Assets/WeatherIcons/unknown.png");
                    }
                }
                else
                {
                    WeatherIcon.Source = new Bitmap($"Assets/WeatherIcons/unknown.png");
                }

                lastUpdate = DateTime.Now;
            }
            catch (HttpRequestException ex)
            {
                StatusBlock.Text = "Помилка HTTP: " + ex.Message;
            }
            catch (JsonException ex)
            {
                StatusBlock.Text = "Помилка обробки даних: " + ex.Message;
            }
            catch (Exception ex)
            {
                StatusBlock.Text = "Невідома помилка: " + ex.Message;
            }
        }

        private string LocalizeDescription(string desc)
        {
            if (string.IsNullOrEmpty(desc)) return desc;

            var dict = new Dictionary<string, string>()
            {
                ["Sunny"] = "Сонячно ☀️",
                ["Clear"] = "Ясно 🌞",
                ["Partly cloudy"] = "Мінлива хмарність 🌤️",
                ["Cloudy"] = "Хмарно ☁️",
                ["Overcast"] = "Похмуро 🌥️",
                ["Mist"] = "Мряка 🌫️",
                ["Patchy rain nearby"] = "Невеликі дощі поруч 🌦️",
                ["Light rain"] = "Невеликий дощ 🌧️",
                ["Heavy rain"] = "Сильний дощ ⛈️",
                ["Patchy rain possible"] = "Місцями дощ 🌧️",
                ["Patchy snow possible"] = "Місцями сніг ❄️",
                ["Patchy sleet possible"] = "Місцями дощ зі снігом 🌨️",
                ["Patchy freezing drizzle possible"] = "Місцями ожеледь 🌧️",
                ["Thundery outbreaks possible"] = "Місцями грози ⚡",
                ["Blowing snow"] = "Заметіль ❄️",
                ["Blizzard"] = "Снігова буря ❄️❄️",
                ["Fog"] = "Туман 🌁",
                ["Freezing fog"] = "Крижаний туман 🌫️",
                ["Patchy light drizzle"] = "Місцями мряка 🌦️",
                ["Light drizzle"] = "Мряка 🌧️",
                ["Freezing drizzle"] = "Ожеледь 🌧️",
                ["Heavy freezing drizzle"] = "Сильна ожеледь 🌧️",
                ["Patchy light rain"] = "Місцями невеликий дощ 🌦️",
                ["Moderate rain at times"] = "Часом помірний дощ 🌧️",
                ["Moderate rain"] = "Помірний дощ 🌧️",
                ["Heavy rain at times"] = "Часом сильний дощ ⛈️",
                ["Heavy rain"] = "Сильний дощ ⛈️",
                ["Light freezing rain"] = "Слабкий крижаний дощ 🌧️",
                ["Moderate or heavy freezing rain"] = "Крижаний дощ 🌧️",
                ["Light sleet"] = "Невеликий дощ зі снігом 🌨️",
                ["Moderate or heavy sleet"] = "Дощ зі снігом 🌨️",
                ["Patchy light snow"] = "Місцями невеликий сніг ❄️",
                ["Light snow"] = "Невеликий сніг ❄️",
                ["Patchy moderate snow"] = "Місцями помірний сніг ❄️",
                ["Moderate snow"] = "Помірний сніг ❄️",
                ["Patchy heavy snow"] = "Місцями сильний сніг ❄️",
                ["Heavy snow"] = "Сильний сніг ❄️❄️",
                ["Ice pellets"] = "Льодяний дощ 🌧️",
                ["Light rain shower"] = "Невелика злива 🌦️",
                ["Rain shower"] = "Невелика злива 🌦️",
                ["Moderate or heavy rain shower"] = "Злива ⛈️",
                ["Torrential rain shower"] = "Дуже сильна злива ⛈️",
                ["Light sleet showers"] = "Невеликий мокрий сніг 🌨️",
                ["Moderate or heavy sleet showers"] = "Мокрий сніг 🌨️",
                ["Light snow showers"] = "Невеликий сніг ❄️",
                ["Moderate or heavy snow showers"] = "Сніг ❄️",
                ["Patchy light rain with thunder"] = "Місцями дощ з грозою ⛈️",
                ["Moderate or heavy rain with thunder"] = "Дощ з грозою ⛈️",
                ["Patchy light snow with thunder"] = "Місцями сніг з грозою ⛈️",
                ["Moderate or heavy snow with thunder"] = "Сніг з грозою ⛈️",
            };
            
            return dict.ContainsKey(desc) ? dict[desc] : desc;
        }

        private readonly Dictionary<string, string> IconMap = new()
        {
            ["Sunny"] = "sunny",
            ["Clear"] = "clear",
            ["Partly cloudy"] = "partlycloudy",
            ["Cloudy"] = "cloudy",
            ["Overcast"] = "mostlycloudy",
            ["Mist"] = "fog",
            ["Fog"] = "fog",
            ["Freezing fog"] = "hazy",

            ["Patchy rain nearby"] = "chancerain",
            ["Light rain"] = "rain",
            ["Rain shower"] = "rain",
            ["Moderate rain"] = "rain",
            ["Heavy rain"] = "rain",
            ["Patchy rain possible"] = "chancerain",
            ["Moderate rain at times"] = "chancerain",
            ["Heavy rain at times"] = "chancerain",
            ["Light rain shower"] = "chancerain",
            ["Moderate or heavy rain shower"] = "rain",

            ["Light drizzle"] = "chancerain",
            ["Patchy light drizzle"] = "chancerain",
            ["Freezing drizzle"] = "sleet",
            ["Heavy freezing drizzle"] = "sleet",

            ["Patchy snow possible"] = "chancesnow",
            ["Light snow"] = "snow",
            ["Heavy snow"] = "snow",
            ["Patchy light snow"] = "chancesnow",
            ["Patchy moderate snow"] = "chancesnow",
            ["Patchy heavy snow"] = "chancesnow",
            ["Moderate snow"] = "snow",

            ["Patchy sleet possible"] = "chancesleet",
            ["Light sleet"] = "sleet",
            ["Moderate or heavy sleet"] = "sleet",
            ["Light sleet showers"] = "chancesleet",
            ["Moderate or heavy sleet showers"] = "sleet",

            ["Thundery outbreaks possible"] = "chancetstorms",
            ["Patchy light rain with thunder"] = "chancetstorms",
            ["Moderate or heavy rain with thunder"] = "tstorms",
            ["Patchy light snow with thunder"] = "chancetstorms",
            ["Moderate or heavy snow with thunder"] = "tstorms",
            ["Torrential rain shower"] = "tstorms",

            ["Blowing snow"] = "flurries",
            ["Blizzard"] = "flurries",
            ["Ice pellets"] = "sleet",
        };
    }
}
