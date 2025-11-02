using Serilog;
using SNESMiniLuaCompiler.Views;
using System;
using System.IO;

namespace SNESMiniLuaCompiler.Helpers
{
    public class ExceptionUtils
    {
        // Serilog logger instance (should be configured in Program.cs)
        private static ILogger? _logger;

        // Call this once during app startup, e.g. in Program.cs
        public static void ConfigureLogger(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Represents detailed information about an error or exception for logging and user feedback.
        /// </summary>
        public class ExceptionInfo(Exception exception, string? userMessage = null, string? context = null)
        {
            public Exception Exception { get; } = exception;
            public string? UserMessage { get; } = userMessage;
            public string? Context { get; } = context;
            public DateTime Timestamp { get; } = DateTime.Now;

            public override string ToString()
            {
                return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] Context: {Context}\r\nUserMessage: {UserMessage}\r\nException: {Exception}\r\n";
            }
        }


        /// <summary>
        /// Throws an exception if the argument is null.
        /// </summary>
        public static void ThrowArgNull(object? arg, string? paramName = null) =>
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
                MessageBox.ShowError($"{msg}\n\nDetails: {ex.Message}", "Error");
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
            string defaultLog = logContext ?? "ExceptionUtils.GlobalTryCatch";
            try
            {
                action();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                string message = ex switch
                {
                    IOException => userMessage ?? "An I/O error occurred.",
                    UnauthorizedAccessException => userMessage ?? "Access denied.",
                    ArgumentException => userMessage ?? "Invalid argument.",
                    _ => userMessage ?? "An unexpected error occurred."
                };
                HandleException(ex, message, defaultLog, showUser);
                //LogException(new ExceptionInfo(ex, message, defaultLog));
            }
            catch (Exception ex)
            {
                HandleException(ex, userMessage ?? "An unexpected error occurred.", defaultLog, showUser);
                //LogException(new ExceptionInfo(ex, userMessage ?? "An unexpected error occurred.", defaultLog));
                throw;
            }
        }

        /// <summary>
        /// Logs exception information to a file.
        /// </summary>
        public static void LogException(ExceptionInfo info)
        {
            _logger?.Error(info.Exception,
                "Context: {Context} | UserMessage: {UserMessage}",
                info.Timestamp, info.Context, info.UserMessage);
        }

        public static void LogException(string info)
        {
            _logger?.Error("{Info}", info);
        }
    }
}