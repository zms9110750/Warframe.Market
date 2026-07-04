using Serilog;
using Serilog.Events;
using System.IO;

namespace WarframeMarketApp;

public static class LogSetup
{
	public static void Configure()
	{
		var logPath = System.IO.Path.Combine(
			AppContext.BaseDirectory, "app.log");

		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Information()
			.WriteTo.File(logPath,
				rollingInterval: RollingInterval.Day,
				outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
			.CreateLogger();
	}
}
