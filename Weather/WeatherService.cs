using System.Text.Json;

public class WeatherService
{
    private const string ApiKey = "0bb93fcb4c1e61244c9e63567223f3ad";
    private const string ApiUrl = "https://api.openweathermap.org/data/2.5/forecast?q={0}&units=metric&appid={1}";

    private WeatherData? _currentWeather;
    private List<WeatherData>? _fiveDayForecast;
    private List<WeatherData>? _fullForecast;
    private string _currentCity = "Moscow";

    public WeatherData? CurrentWeather => _currentWeather;
    public List<WeatherData>? FiveDayForecast => _fiveDayForecast;
    public List<WeatherData>? FullForecast => _fullForecast;
    public string CurrentCity => _currentCity;

    public event Action? OnDataUpdated;

    public async Task<bool> LoadWeatherDataAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            city = _currentCity;
        else
            _currentCity = city;

        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(string.Format(ApiUrl, city, ApiKey));

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<WeatherApiResponse>(json);

            if (data?.Items == null || data.Items.Count == 0)
            {
                return false;
            }

            _fullForecast = data.Items;

            var now = DateTime.Now;

            _currentWeather = data.Items
                .Where(x => x.Date >= now.AddHours(-3))
                .OrderBy(x => Math.Abs((x.Date - now).TotalHours))
                .FirstOrDefault();

            var today = now.Date;
            var dailyGroups = data.Items
                .Where(x => x.Date.Date >= today)
                .GroupBy(x => x.Date.Date)
                .OrderBy(g => g.Key)
                .Take(5)
                .ToList();

            _fiveDayForecast = new List<WeatherData>();
            foreach (var dayGroup in dailyGroups)
            {
                WeatherData? selectedData;

                var targetTime = dayGroup.Key.AddHours(12);
                selectedData = dayGroup.OrderBy(x => Math.Abs((x.Date - targetTime).TotalHours)).FirstOrDefault();

                if (selectedData != null)
                {
                    _fiveDayForecast.Add(selectedData);
                }
            }

            OnDataUpdated?.Invoke();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void ClearData()
    {
        _currentWeather = null;
        _fiveDayForecast = null;
        _fullForecast = null;
        OnDataUpdated?.Invoke();
    }
}