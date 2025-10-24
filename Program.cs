using Avalonia;
using System;
using System.Threading;

namespace SNESMiniLuaCompiler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Utils.AppUtils.EnsureAllModuleDirectories();

                using (var mutex = new Mutex(true, "SNESMiniLuaCompiler", out bool singleExecution))
                {
                    if (singleExecution)
                    {
                        Utils.AppUtils.ExtractAllResources();
                        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                    }
                    else
                    {
                        MsgBox.Show("The Application Is Already Running", "Lua Compiler", MsgBox.ButtonType.OK, MsgBox.Ico.Warning);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                // Log or display the error message
                // You may want to use Avalonia dialogs here in the future
                Console.Error.WriteLine($"An application error occurred: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Builds the Avalonia application.
        /// </summary>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}