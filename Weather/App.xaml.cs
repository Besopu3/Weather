namespace Weather;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Создаем и показываем MainSwipePage
        MainPage = new MainSwipePage();
    }
}