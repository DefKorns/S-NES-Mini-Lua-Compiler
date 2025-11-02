using Avalonia.Controls;
using Avalonia.Controls.Notifications;

namespace SNESMiniLuaCompiler.Helpers
{
    public static class NotificationHelper
    {
        private static WindowNotificationManager? _manager;

        public static void Initialize(Window window)
        {
            _manager = new WindowNotificationManager(window)
            {
                Position = NotificationPosition.TopRight,
                MaxItems = 3
            };
        }

        public static void Success(string message, params string[]? classes)
        {
            _manager?.Show(new Notification(null, message), type: NotificationType.Success, classes: classes);
        }

        public static void Warning(string message, params string[]? classes)
        {
            _manager?.Show(new Notification(null, message), type: NotificationType.Warning, classes: classes);
        }
        public static void Error(string message, params string[]? classes)
        {
            _manager?.Show(new Notification(null, message), type: NotificationType.Error, classes: classes);
        }
        public static void Information(string message, params string[]? classes)
        {
            _manager?.Show(new Notification(null, message), type: NotificationType.Information, classes: classes);
        }
    }
}