using LibGit2Sharp;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

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
            bool hasModulesPath = Directory.Exists(AppUtils.modulesPath);
            if (!hasModulesPath)
                Directory.CreateDirectory(AppUtils.modulesPath);
            bool emptyModules = Directory.GetFileSystemEntries(AppUtils.modulesPath).Length == 0;

            using (Mutex mutex = new Mutex(true, "SNESMiniLuaCompiler", out bool singleExecution))
            {

                if (singleExecution)
                {
                    if (!hasModulesPath || emptyModules)
                    {
                        Repository.Clone("https://github.com/bobsayshilol/luajit-decomp.git", AppUtils.luaJitPath, new CloneOptions { BranchName = "deprecated" });
                        Repository.Clone("https://gitlab.com/znixian/luajit-decompiler.git", AppUtils.decompilerPath);
                    }

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    using (MainForm mainWindow = new MainForm())
                    {
                        Application.Run(mainWindow);
                    }

                }
                else
                {
                    MsgBox.Show("The Application Is Already Running", "Lua Compiler", MsgBox.ButtonType.OK, MsgBox.Ico.Warning);
                }
            }

        }
    }
}
