using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Weather;

public partial class FiveDayForecastView : ContentView, INotifyPropertyChanged
{
    private readonly WeatherService _weatherService;
    private bool _isLoading;
    private bool _isViewActive = true;
    private ObservableCollection<WeatherDay> _weatherDays = new();

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged(nameof(NoDataVisible));
            NotifyPropertyChanged(nameof(HasData));
        }
    }

    public bool NoDataVisible => !IsLoading && !HasData;
    public bool HasData => _weatherService.FiveDayForecast?.Count > 0;

    public ObservableCollection<WeatherDay> WeatherDays
    {
        get => _weatherDays;
        set
        {
            _weatherDays = value;
            NotifyPropertyChanged();
        }
    }

    public FiveDayForecastView(WeatherService weatherService)
    {
        InitializeComponent();
        _weatherService = weatherService;

        CityEntry.Text = _weatherService.CurrentCity;
        BindingContext = this;
        _weatherService.OnDataUpdated += OnWeatherDataUpdated;
        LoadWeatherData(_weatherService.CurrentCity);
    }

    protected override void OnParentChanged()
    {
        base.OnParentChanged();
        _isViewActive = this.Parent != null;

        if (_isViewActive)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateWeatherData();
            });
        }
    }

    private void OnWeatherDataUpdated()
    {
        if (_isViewActive)
        {
            MainThread.BeginInvokeOnMainThread(UpdateWeatherData);
        }
    }

    public void UpdateWeatherData()
    {
        NotifyPropertyChanged(nameof(HasData));
        NotifyPropertyChanged(nameof(NoDataVisible));

        if (CityEntry.Text != _weatherService.CurrentCity)
        {
            CityEntry.Text = _weatherService.CurrentCity;
        }

        UpdateWeatherDays();
    }

    private async void LoadWeatherData(string city)
    {
        IsLoading = true;
        await _weatherService.LoadWeatherDataAsync(city);
        IsLoading = false;
    }

    private async void OnLoadWeatherClicked(object sender, EventArgs e)
    {
        try
        {
            IsLoading = true;
            await _weatherService.LoadWeatherDataAsync(CityEntry.Text);
        }
        catch (Exception)
        {
            // Игнорируем ошибки
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateWeatherDays()
    {
        var newWeatherDays = new ObservableCollection<WeatherDay>();

        if (_weatherService.FiveDayForecast == null)
        {
            WeatherDays = newWeatherDays;
            return;
        }

        var now = DateTime.Now;
        var today = now.Date;

        foreach (var weatherData in _weatherService.FiveDayForecast)
        {
            if (weatherData?.Weather?.FirstOrDefault() == null) continue;

            var weather = weatherData.Weather.First();

            string dateDisplay;
            if (weatherData.Date.Date == today)
            {
                dateDisplay = "Сегодня";
            }
            else if (weatherData.Date.Date == today.AddDays(1))
            {
                dateDisplay = "Завтра";
            }
            else
            {
                dateDisplay = weatherData.Date.ToString("dd MMM", new System.Globalization.CultureInfo("ru-RU"));
            }

            newWeatherDays.Add(new WeatherDay
            {
                Date = dateDisplay,
                Temperature = $"{weatherData.Main.Temperature:0}°C",
                Description = CapitalizeFirstLetter(weather.Description) ?? "Нет описания",
                Humidity = $"{weatherData.Main.Humidity}%",
                IconUrl = $"https://openweathermap.org/img/wn/{weather.Icon}@2x.png",
                TempValue = weatherData.Main.Temperature,
                HumidityValue = weatherData.Main.Humidity,
                WeatherIcon = weather.Icon
            });
        }

        WeatherDays = newWeatherDays;
    }

    private string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1);
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}