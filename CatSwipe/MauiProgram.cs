using CatSwipe.Services;
using CatSwipe.Views;

using Microsoft.Extensions.Logging;

namespace CatSwipe;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<ICatService, CatService>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<CollectionPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
