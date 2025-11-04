using Avalonia;
using ReactiveUI.Avalonia;
using SNESMiniLuaCompiler.Helpers;
using Serilog;
using System;
using System.Threading;

namespace SNESMiniLuaCompiler
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           .WriteTo.File(
               "error.log",
               rollingInterval: RollingInterval.Day, // Optional: creates a new file each day
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

