using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace WeatherAppAvalonia.Services
{
    public class WeatherLocalizationService
    {
        private readonly Dictionary<string, string> _desc = new()
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

        private readonly Dictionary<string, string> _icons = new()
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

        public string Localize(string? desc) =>
            string.IsNullOrEmpty(desc) ? "Невідомо" : _desc.TryGetValue(desc, out var result) ? result : desc;

        public Bitmap GetIcon(string? desc)
        {
            if (desc != null && _icons.TryGetValue(desc, out var iconName))
            {
                var uri = new Uri($"avares://WeatherAppAvalonia/Assets/WeatherIcons/{iconName}.png");
                if (AssetLoader.Exists(uri))
                    return new Bitmap(AssetLoader.Open(uri));
            }

            return new Bitmap("Assets/WeatherIcons/unknown.png");
        }
    }
}
