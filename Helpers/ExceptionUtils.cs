using System;
using System.Diagnostics;
using System.IO;

namespace SNESMiniLuaCompiler.Helpers
{

    public class ExceptionUtils
    {
        /// <summary>
        /// Represents detailed information about an error or exception for logging and user feedback.
        /// </summary>
        public class ExceptionInfo
        {
            public Exception Exception { get; }
            public string? UserMessage { get; }
            public string? Context { get; }
            public DateTime Timestamp { get; }

            public ExceptionInfo(Exception exception, string? userMessage = null, string? context = null)
            {
                Exception = exception;
                UserMessage = userMessage;
                Context = context;
                Timestamp = DateTime.Now;
            }

            public override string ToString()
            {
                return $"[{Timestamp}] Context: {Context}\r\nUserMessage: {UserMessage}\r\nException: {Exception}\r\n";
            }
        }


        /// <summary>
        /// Throws an exception if the argument is null.
        /// </summary>
        public static void ThrowArgNull(object arg, string? paramName = null) => 
            _ = arg ?? throw new ArgumentNullException(paramName);

        /// <summary>
        /// Centralized error logger and user notifier.
        /// </summary>
        public static void HandleException(Exception ex, string? userMessage = null, string? logContext = null, bool showUser = true)
        {
            ThrowArgNull(ex, nameof(ex));

            var info = new ExceptionInfo(ex, userMessage, logContext);
            LogException(info);

            if (showUser)
            {
                string msg = userMessage ?? "An error occurred.";
                //MsgBox.Show($"{msg}\n\nDetails: {ex.Message}", "Error", MsgBox.ButtonType.OK, MsgBox.Ico.Error);
            }
        }

        /// <summary>
        /// Logs and optionally shows user-friendly messages for expected exceptions.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="userMessage">A user-friendly message (optional).</param>
        /// <param name="logContext">Context for logging (optional).</param>
        /// <param name="showUser">Whether to show a message box to the user (default: true).</param>
        public static void GlobalTryCatch(Action action, string? userMessage = null, string? logContext = null, bool showUser = true)
        {
            string DefaultLog = logContext ?? "ExceptionUtils.GlobalTryCatch";
            try
            {
                action.Invoke();
            }
            catch (IOException ex)
            {
                HandleException(ex, userMessage ?? "An I/O error occurred.", DefaultLog, showUser);
                LogException(new ExceptionInfo(ex, userMessage ?? "An I/O error occurred.", DefaultLog));
            }
            catch (UnauthorizedAccessException ex)
            {
                HandleException(ex, userMessage ?? "Access denied.", DefaultLog, showUser);
                LogException(new ExceptionInfo(ex, userMessage ?? "Access denied.", DefaultLog));
            }
            catch (ArgumentException ex)
            {
                HandleException(ex, userMessage ?? "Invalid argument.", DefaultLog, showUser);
                LogException(new ExceptionInfo(ex, userMessage ?? "Invalid argument.", DefaultLog));
            }
            catch (Exception ex)
            {
                HandleException(ex, userMessage ?? "An unexpected error occurred.", DefaultLog, showUser);
                LogException(new ExceptionInfo(ex, userMessage ?? "An unexpected error occurred.", DefaultLog));
                throw;
            }
        }

        /// <summary>
        /// Logs exception information to a file.
        /// </summary>
        public static void LogException(ExceptionInfo info)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            File.AppendAllText(logPath, info.ToString());
        }

        public static void LogException(string info)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            File.AppendAllText(logPath, info);
        }
    }
}