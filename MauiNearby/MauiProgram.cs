using MauiNearby.Services;
using Microsoft.Extensions.Logging;

namespace MauiNearby;

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

		builder.Services.AddSingleton<NearbySyncService>(NearbySyncService.Instance);

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

