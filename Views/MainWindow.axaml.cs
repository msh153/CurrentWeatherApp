using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Input;
using WeatherAppAvalonia.Services;
using WeatherAppAvalonia.Models;
using System.Threading.Tasks;


namespace WeatherAppAvalonia.Views
{
    public partial class MainWindow : Window
    {
        private readonly WeatherService weatherService = new();
        private readonly WeatherLocalizationService localization = new();

        public MainWindow()
        {
            InitializeComponent();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(10) };
            timer.Tick += async (_, _) =>
            {
                var city = CityTextBox?.Text;
                if (!string.IsNullOrWhiteSpace(city))
                    await LoadWeather(city, force: false);
            };
            timer.Start();
        }

        private async void OnRefreshButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var city = CityTextBox.Text;
            await LoadWeather(city, force: true);
        }

        private void CityTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OnRefreshButtonClick(this, new Avalonia.Interactivity.RoutedEventArgs());
        }

        private async Task LoadWeather(string? city, bool force)
        {
            try
            {
                StatusBlock.Text = string.IsNullOrWhiteSpace(city) ? "Визначення міста по IP…" : "Завантаження…";

                var data = await weatherService.GetWeatherAsync(city, force);
                if (data == null || data?.CurrentCondition?.Count == 0)
                {
                    StatusBlock.Text = "Неправильний формат відповіді API";
                    return;
                }

                ApplyWeatherData(data!);
                StatusBlock.Text = "";
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
                StatusBlock.Text = "Помилка: " + ex.Message;
            }
        }

        private void ApplyWeatherData(WttrResponse data)
        {
            var current = data.CurrentCondition?.FirstOrDefault();
            var area = data.NearestArea?.FirstOrDefault();

            CityNameBlock.Text = area?.AreaName?.FirstOrDefault()?.Value ?? "Невідомо";
            TempBlock.Text = $"{current?.TempC} °C";

            var desc = current?.WeatherDesc?.FirstOrDefault()?.Value ?? "";
            DescBlock.Text = localization.Localize(desc);
            WeatherIcon.Source = localization.GetIcon(desc);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            localization.Dispose();
        }
    }
}
