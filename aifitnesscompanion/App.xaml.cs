namespace aifitnesscompanion;

public partial class App : Application
{
    public App()
    {

        InitializeComponent();
        MainPage = new AppShell();
    }

    public object ServiceProvider { get; internal set; }
}

