using System;
using System.IO;
using Serilog;
using SNESMiniLuaCompiler.Views;

namespace SNESMiniLuaCompiler.Helpers
{
    public static class ExceptionUtils
    {
        private static ILogger? _logger;

        public static void ConfigureLogger(ILogger logger)
        {
            _logger = logger;
        }

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

        public static void ThrowArgNull(object? arg, string? paramName = null) =>
            _ = arg ?? throw new ArgumentNullException(paramName);

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

        public static T SafeFileOperation<T>(Func<T> operation, T defaultValue, string? filePath = null, string? context = null, bool showUser = false)
        {
            if (filePath != null && !File.Exists(filePath))
            {
                return defaultValue;
            }

            try
            {
                return operation();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                HandleException(ex, null, context ?? "SafeFileOperation", showUser);
                return defaultValue;
            }
        }

        public static void LogException(ExceptionInfo info)
        {
            _logger?.Error(info.Exception,
                "[{Timestamp}] Context: {Context} | UserMessage: {UserMessage}",
                info.Timestamp, info.Context, info.UserMessage);
        }

        public static void LogException(string info)
        {
            _logger?.Error("{Info}", info);
        }

        public static void GlobalTryCatch(Action action, string? userMessage = null, string? logContext = null, bool showUser = true)
        {
            try
            {
                action();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                HandleException(ex, userMessage ?? GetDefaultMessage(ex), logContext ?? "GlobalTryCatch", showUser);
            }
            catch (Exception ex)
            {
                HandleException(ex, userMessage ?? "An unexpected error occurred.", logContext ?? "GlobalTryCatch", showUser);
                throw;
            }
        }

        private static string GetDefaultMessage(Exception ex) => ex switch
        {
            IOException => "An I/O error occurred.",
            UnauthorizedAccessException => "Access denied.",
            ArgumentException => "Invalid argument.",
            _ => "An unexpected error occurred."
        };
    }
}
