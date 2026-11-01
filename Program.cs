using System;
using System.Globalization;
using System.Threading;
using Avalonia;
using ReactiveUI.Avalonia;
using Serilog;
using SNESMiniLuaCompiler.Helpers;

namespace SNESMiniLuaCompiler
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        static void Main(string[] args)
        {
            // Relative paths (decoded, lib/hashes, error.log) are resolved against the app folder,
            // not wherever the app was launched from
            Environment.CurrentDirectory = AppContext.BaseDirectory;

            Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           .WriteTo.File(
               "error.log",
               rollingInterval: RollingInterval.Day, // Optional: creates a new file each day
               formatProvider: CultureInfo.InvariantCulture,
               outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}"
           )
           .CreateLogger();

            ExceptionUtils.ConfigureLogger(Log.Logger);
            try
            {
                AppUtils.EnsureAllModuleDirectories();

                using var mutex = new Mutex(true, "SNESMiniLuaCompiler", out bool singleExecution);
                if (singleExecution)
                {
                    AppUtils.SaveGitVersionToConfig();
                    AppUtils.ExtractAllResources();
                    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                }
                else
                {
                    ExceptionUtils.LogException("The Application Is Already Running");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
