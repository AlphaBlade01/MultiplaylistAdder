using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MultiplaylistAdder.Logic;
using MultiplaylistAdder.ViewModels;
using MultiplaylistAdder.Views;
using System.Reflection;

namespace MultiplaylistAdder;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream("MultiplaylistAdder.appsettings.json");
        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        builder.Configuration.AddConfiguration(config);
        builder.Services
            .AddSingleton<Spotify>()
            .AddSingleton<MainPage>()
            .AddSingleton<MainViewModel>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
