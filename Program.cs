using Avalonia;
using ReactiveUI.Avalonia;
using SNESMiniLuaCompiler.Helpers; // Add this to import AppUtils
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

            // Pass Serilog logger to ExceptionUtils
            ExceptionUtils.ConfigureLogger(Log.Logger);
            try
            {
                AppUtils.EnsureAllModuleDirectories();

                using var mutex = new Mutex(true, "SNESMiniLuaCompiler", out bool singleExecution);
                if (singleExecution)
                {
                    AppUtils.ExtractAllResources();
                    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                }
                else
                {
                    //MsgBox.Show("The Application Is Already Running", "Lua Compiler", MsgBox.ButtonType.OK, MsgBox.Ico.Warning);
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
            //catch (InvalidOperationException ex)
            //{
            //    // Log or display the error message
            //    // You may want to use Avalonia dialogs here in the future
            //    Console.Error.WriteLine($"An application error occurred: {ex.Message}");
            //    ExceptionUtils.LogException(new ExceptionUtils.ExceptionInfo(ex, "An application error occurred:", ex.Message));
            //}
            //catch (Exception ex)
            //{
            //    Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
            //    ExceptionUtils.LogException(new ExceptionUtils.ExceptionInfo(ex, "An unexpected error occurred:", ex.Message));
            //    throw;
            //}
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}

