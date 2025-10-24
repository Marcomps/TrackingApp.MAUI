using Microsoft.Extensions.Logging;
using System.Globalization;

namespace TrackingApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		// Configurar cultura en español
		var culture = new CultureInfo("es-ES");
		CultureInfo.DefaultThreadCurrentCulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;
		
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
