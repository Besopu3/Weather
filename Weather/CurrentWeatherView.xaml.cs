using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Weather;

public partial class CurrentWeatherView : ContentView, INotifyPropertyChanged
{
    private readonly WeatherService _weatherService;
    private bool _isLoading;
    private bool _isViewActive = true;
    private ObservableCollection<HourlyWeather> _hourlyForecast = new();

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
    public bool HasData => _weatherService.CurrentWeather != null;

    public string CurrentDate => _weatherService.CurrentWeather?.Date.ToString("dd MMMM yyyy") ?? "";
    public string CurrentTemperature => _weatherService.CurrentWeather != null ? $"{_weatherService.CurrentWeather.Main.Temperature:0}°C" : "";
    public string CurrentDescription => _weatherService.CurrentWeather?.Weather?.FirstOrDefault()?.Description ?? "";
    public string CurrentHumidity => _weatherService.CurrentWeather?.Main.Humidity.ToString() ?? "";
    public string CurrentWind => _weatherService.CurrentWeather?.Wind?.Speed.ToString() ?? "";
    public string CurrentIconUrl => _weatherService.CurrentWeather?.Weather?.FirstOrDefault() != null ?
        $"https://openweathermap.org/img/wn/{_weatherService.CurrentWeather.Weather.First().Icon}@2x.png" : "";

    public ObservableCollection<HourlyWeather> HourlyForecast
    {
        get => _hourlyForecast;
        set
        {
            _hourlyForecast = value;
            NotifyPropertyChanged();
        }
    }

    public CurrentWeatherView(WeatherService weatherService)
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
        NotifyPropertyChanged(nameof(CurrentDate));
        NotifyPropertyChanged(nameof(CurrentTemperature));
        NotifyPropertyChanged(nameof(CurrentDescription));
        NotifyPropertyChanged(nameof(CurrentHumidity));
        NotifyPropertyChanged(nameof(CurrentWind));
        NotifyPropertyChanged(nameof(CurrentIconUrl));
        NotifyPropertyChanged(nameof(HasData));
        NotifyPropertyChanged(nameof(NoDataVisible));

        if (CityEntry.Text != _weatherService.CurrentCity)
        {
            CityEntry.Text = _weatherService.CurrentCity;
        }

        UpdateHourlyForecast();
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

    private void UpdateHourlyForecast()
    {
        var newForecast = new ObservableCollection<HourlyWeather>();

        if (_weatherService.FullForecast == null)
        {
            HourlyForecast = newForecast;
            return;
        }

        var now = DateTime.Now;
        var today = now.Date;

        var todayData = _weatherService.FullForecast
            .Where(x => x.Date.Date == today)
            .ToList();

        var targetHours = new[] { 6, 12, 18, 21 };

        foreach (var hour in targetHours)
        {
            var targetTime = today.AddHours(hour);
            var forecast = todayData
                .OrderBy(x => Math.Abs((x.Date - targetTime).TotalHours))
                .FirstOrDefault();

            if (forecast != null)
            {
                newForecast.Add(new HourlyWeather
                {
                    Time = targetTime.ToString("HH:mm"),
                    Temperature = $"{forecast.Main.Temperature:0}°C",
                    Description = CapitalizeFirstLetter(forecast.Weather.First().Description),
                    Humidity = forecast.Main.Humidity,
                    IconUrl = $"https://openweathermap.org/img/wn/{forecast.Weather.First().Icon}@2x.png"
                });
            }
        }

        HourlyForecast = newForecast;
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