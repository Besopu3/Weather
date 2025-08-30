using System.Text.Json.Serialization;

public class WeatherApiResponse
{
    [JsonPropertyName("list")]
    public List<WeatherData> Items { get; set; } = new List<WeatherData>();
}

public class WeatherData
{
    [JsonPropertyName("dt_txt")]
    public string DateString { get; set; } = string.Empty;

    public DateTime Date
    {
        get
        {
            if (DateTime.TryParseExact(DateString, "yyyy-MM-dd HH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            return DateTime.MinValue;
        }
    }

    [JsonPropertyName("main")]
    public MainData Main { get; set; } = new MainData();

    [JsonPropertyName("weather")]
    public List<WeatherDescription> Weather { get; set; } = new List<WeatherDescription>();

    [JsonPropertyName("wind")]
    public WindData Wind { get; set; } = new WindData();
}

public class MainData
{
    [JsonPropertyName("temp")]
    public double Temperature { get; set; }

    [JsonPropertyName("temp_min")]
    public double MinTemperature { get; set; }

    [JsonPropertyName("temp_max")]
    public double MaxTemperature { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}

public class WeatherDescription
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
}

public class WindData
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }

    [JsonPropertyName("deg")]
    public int Direction { get; set; }
}

public class HourlyWeather
{
    public string Time { get; set; } = string.Empty;
    public string Temperature { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Humidity { get; set; }
    public string IconUrl { get; set; } = string.Empty;
}

public class WeatherDay
{
    public string Date { get; set; } = string.Empty;
    public string Temperature { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Humidity { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public double TempValue { get; set; }
    public int HumidityValue { get; set; }
    public string WeatherIcon { get; set; } = string.Empty;
}