using Microsoft.Maui.Controls;

namespace Weather;

public partial class MainSwipePage : ContentPage
{
    private readonly WeatherService _weatherService;
    private readonly CurrentWeatherView _currentWeatherView;
    private readonly FiveDayForecastView _fiveDayForecastView;
    private ContentView _currentView;
    private bool _isAnimating;

    public MainSwipePage()
    {
        InitializeComponent();
        _weatherService = new WeatherService();
        _currentWeatherView = new CurrentWeatherView(_weatherService);
        _fiveDayForecastView = new FiveDayForecastView(_weatherService);
        _currentView = _currentWeatherView;
        ContentContainer.Content = _currentView;
        UpdateIndicators();
    }

    private void OnSwiped(object? sender, SwipedEventArgs e)
    {
        if (_isAnimating) return;
        _isAnimating = true;

        try
        {
            switch (e.Direction)
            {
                case SwipeDirection.Left:
                    if (_currentView == _currentWeatherView)
                    {
                        AnimateTransition(_fiveDayForecastView, -1);
                        _currentView = _fiveDayForecastView;
                        UpdateIndicators();
                    }
                    break;

                case SwipeDirection.Right:
                    if (_currentView == _fiveDayForecastView)
                    {
                        AnimateTransition(_currentWeatherView, 1);
                        _currentView = _currentWeatherView;
                        UpdateIndicators();
                    }
                    break;
            }
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async void AnimateTransition(ContentView newView, int direction)
    {
        try
        {
            _isAnimating = true;
            var oldView = ContentContainer.Content;
            ContentContainer.Content = newView;
            ContentContainer.TranslationX = direction * this.Width;
            await ContentContainer.TranslateTo(0, 0, 250, Easing.CubicOut);

            if (newView is CurrentWeatherView currentView)
            {
                currentView.UpdateWeatherData();
            }
            else if (newView is FiveDayForecastView forecastView)
            {
                forecastView.UpdateWeatherData();
            }
        }
        catch (Exception)
        {
            // Игнорируем ошибки
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private void UpdateIndicators()
    {
        if (Indicator1 == null || Indicator2 == null) return;

        if (_currentView == _currentWeatherView)
        {
            Indicator1.Color = Color.FromArgb("#CBBDFF");
            Indicator2.Color = Color.FromArgb("#E6DDFF");
        }
        else
        {
            Indicator1.Color = Color.FromArgb("#E6DDFF");
            Indicator2.Color = Color.FromArgb("#CBBDFF");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _weatherService.ClearData();
    }
}