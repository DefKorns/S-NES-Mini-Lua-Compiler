using Avalonia;
using ReactiveUI.Avalonia;
using SNESMiniLuaCompiler.Helpers; // Add this to import AppUtils
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
        //public static void Main(string[] args) => BuildAvaloniaApp()
        //    .StartWithClassicDesktopLifetime(args);
        static void Main(string[] args)
        {
            try
            {
                AppUtils.EnsureAllModuleDirectories();
                //AppUtils.EnsureDirectoryExists(AppUtils.RecodedPath);
                //FileUtils.CreatePath(AppUtils.RecodedPath);

                using var mutex = new Mutex(true, "SNESMiniLuaCompiler", out bool singleExecution);
                if (singleExecution)
                {
                    AppUtils.ExtractAllResources();
                    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                }
                else
                {
                    //MsgBox.Show("The Application Is Already Running", "Lua Compiler", MsgBox.ButtonType.OK, MsgBox.Ico.Warning);
                    ExceptionUtils.LogException("The Application Is Already Running\", \"Lua Compiler\",");
                }
            }
            catch (InvalidOperationException ex)
            {
                // Log or display the error message
                // You may want to use Avalonia dialogs here in the future
                Console.Error.WriteLine($"An application error occurred: {ex.Message}");
                ExceptionUtils.LogException(new ExceptionUtils.ExceptionInfo(ex, "An application error occurred:", ex.Message));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
                ExceptionUtils.LogException(new ExceptionUtils.ExceptionInfo(ex, "An unexpected error occurred:", ex.Message));
                throw;
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

