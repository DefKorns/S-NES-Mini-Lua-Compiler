using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;


namespace SNESMiniLuaCompiler
{
    public static class AppUtils
    {
        private const ProcessWindowStyle hidden = ProcessWindowStyle.Hidden;
        private static Cursor Cursor;

        public static readonly string appPath = Path.GetDirectoryName(Application.ExecutablePath);
        public static readonly string modulesPath = Path.Combine(appPath, "modules");
        public static readonly string decodedPath = Path.Combine(appPath, "decoded");
        public static readonly string recodedPath = Path.Combine(appPath, "recoded");
        public static readonly string originalPath = Path.Combine("lib", "original");
        public static readonly string luaJitPath = Path.Combine("modules", "luajit");
        public static readonly string decompilerPath = Path.Combine("modules", "decompiler");
        public static readonly string decompilerScript = Path.Combine(decompilerPath, "main.py");
        public static readonly string decodedHashFile = Path.Combine("lib", "hashes");
        public static readonly string firstRun = Path.Combine("lib", "firstRun");
        public static readonly string batFile = "encode.bat";

        public static void CopyAssets(string sourceFolder, string destFolder)
        {
            CreatePath(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest, true);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyAssets(folder, dest);
            }
        }

        public static string FindExePath(string exe)
        {
            exe = Environment.ExpandEnvironmentVariables(exe);
            if (!File.Exists(exe))
            {
                if (string.IsNullOrEmpty(Path.GetDirectoryName(exe)))
                {
                    foreach (string arg in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
                    {
                        string path = arg.Trim();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                            return Path.GetFullPath(path);
                    }
                }
            }
            return "";
        }

        public static void RunCmd(string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = cmd,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                }
            }
        }

        public static void CreateLuaJitLauncher()
        {
            if (!File.Exists(batFile))
            {
                StreamWriter w = new StreamWriter(batFile);
                w.WriteLine("@setlocal enableextensions enabledelayedexpansion");
                w.WriteLine("@echo off");
                w.WriteLine("set LuaJit=modules\\luajit");
                w.WriteLine("set LUA_DIR=%LuaJit%");
                w.WriteLine("set LUA_CPATH=?.dll;%LUA_DIR%\\?.dll");
                w.WriteLine("set LUA_PATH=?.lua;%LUA_DIR%\\jit\\?.lua;%LUA_DIR%\\?.lua");
                w.WriteLine("%LuaJit%\\luajit.exe -b %1 %2");
                w.Close();
            }
        }

        public static string GetSHA256HashFromFile(string fileName)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty).ToLower(CultureInfo.CurrentCulture);
                }
            }
        }

        public static void GenerateFileHash(string fileName)
        {
            string fileHash = GetSHA256HashFromFile(fileName);
                       
            using (StreamWriter sw = File.AppendText(decodedHashFile))
            {
                sw.WriteLine(fileHash);
            }
        }

        public static bool HasEditedFiles(string hash)
        {
            foreach (var line in File.ReadAllLines(decodedHashFile))
            {
                if (line.Contains(hash))
                {
                    return false;
                }

            }
            return true;
        }

        public static void RunLuaJit(string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WorkingDirectory = appPath,
                CreateNoWindow = false,
                FileName = "encode.bat",
                WindowStyle = hidden,
                Arguments = args
            };

            using (Process batProcess = Process.Start(startInfo))
            {
                batProcess.WaitForExit();
            }

        }

        public static void LoadSpinner(bool displayLoader, PictureBox loaderImage, Form form)
        {
            ThrowArgNull(loaderImage);
            ThrowArgNull(form);

            if (displayLoader)
            {
                form.Invoke((MethodInvoker)delegate
                {
                    loaderImage.Visible = true;
                    loaderImage.BringToFront();
                    Cursor = Cursors.WaitCursor;
                });
            }
            else
            {
                form.Invoke((MethodInvoker)delegate
                {
                    loaderImage.Visible = false;
                    Cursor = Cursors.Default;
                });
            }
        }

        public static void CreatePath(string path)
        {
            bool folderExists = Directory.Exists(path);
            if (!folderExists)
                Directory.CreateDirectory(path);
        }

        public static void DeletePath(string path)
        {
            bool folderExists = Directory.Exists(path);
            if (folderExists)
            {
                Directory.Delete(path, true);
            }
        }
        
        public static void DeleteEmptyDirectories(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                DeleteEmptyDirectories(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }

        public static void DeleteFile(string file)
        {
            bool fileExists = File.Exists(file);
            if (fileExists)
            {
                File.Delete(file);
            }
        }

        public static void ThrowArgNull(object arg)
        {
            if (arg == null) throw new ArgumentNullException(nameof(arg), "There is no expression with which to test the object's value.");
        }

    }


}
