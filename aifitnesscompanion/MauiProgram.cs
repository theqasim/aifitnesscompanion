using aifitnesscompanion.Helpers;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {

        EnvFileLoader.LoadEnvVariables();

        var builder = MauiApp.CreateBuilder();
        _ = builder
            .UseMauiApp<aifitnesscompanion.App>()
            .UseMauiMaps() 
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaisonNeueExtraBold.otf", "MaisonNeueExtendedExtraBold");
                fonts.AddFont("BricolageGrotesque_36pt-SemiBold.ttf", "BricolageGrotesqueSemiBold");
                fonts.AddFont("DMSans-ExtraBold.ttf", "DMSans");

            });

        return builder.Build();
    }
}
