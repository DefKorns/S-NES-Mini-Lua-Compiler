//using LibGit2Sharp;
using System;
//using System.IO;
using System.Threading;
using System.Windows.Forms;
using static SNESMiniLuaCompiler.Utils.AppUtils;

namespace SNESMiniLuaCompiler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                EnsureAllModuleDirectories();

                using (var mutex = new Mutex(true, "SNESMiniLuaCompiler", out bool singleExecution))
                {
                    if (singleExecution)
                    {
                        ExtractAllResources();

                        // Initialize and run the application
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        using (var mainWindow = new MainForm())
                        {
                            Application.Run(mainWindow);
                        }
                    }
                    else
                    {
                        ShowAlreadyRunningMessage();
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                // Log or display the error message
                MessageBox.Show($"An application error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // Log or display unexpected errors
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // Rethrow to ensure the application doesn't continue in an unstable state
            }
        }

        /// <summary>
        /// Displays a message indicating the application is already running.
        /// </summary>
        private static void ShowAlreadyRunningMessage()
        {
            MsgBox.Show("The Application Is Already Running", "Lua Compiler", MsgBox.ButtonType.OK, MsgBox.Ico.Warning);
        }
    }
}