using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

namespace SNESMiniLuaCompiler.Helpers
{
    public static class MessageBoxHelper
    {
        public static async Task<string?> ShowConfirmationAsync(
            string title,
            string message,
            IEnumerable<ButtonDefinition>? buttons = null)
        {
            var defaultButtons = buttons ?? new List<ButtonDefinition>
            {
                new ButtonDefinition { Name = "Yes" },
                new ButtonDefinition { Name = "No" }
            };
            return await ShowCustomAsync(title, message, defaultButtons, Icon.Question);
        }

        public static async Task<string?> ShowSuccessAsync(string title, string message)
        {
            return await ShowCustomAsync(title, message, null, Icon.Success);
        }

        public static async Task<string?> ShowWarningAsync(string title, string message)
        {
            return await ShowCustomAsync(title, message, null, Icon.Warning);
        }

        public static async Task<string?> ShowErrorAsync(string title, string message)
        {
            return await ShowCustomAsync(title, message, null, Icon.Error);
        }

        public static async Task<string?> ShowNotificationAsync(string title, string message)
        {
            return await ShowCustomAsync(title, message, null, Icon.Info);
        }

        public static async Task<string?> ShowCustomAsync(
            string title,
            string message,
            IEnumerable<ButtonDefinition>? buttons = null,
            Icon icon = Icon.None,
            int maxWidth = 500,
            int maxHeight = 800)
        {
            var box = MessageBoxManager.GetMessageBoxCustom(
                new MessageBoxCustomParams
                {
                    ButtonDefinitions = buttons ?? new List<ButtonDefinition>
                    {
                        new ButtonDefinition { Name = "OK" }
                    },
                    ContentTitle = title,
                    ContentMessage = message,
                    Icon = icon,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CanResize = false,
                    MaxWidth = maxWidth,
                    MaxHeight = maxHeight,
                    SystemDecorations = SystemDecorations.None,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ShowInCenter = true,
                    Topmost = false,
                });

            return await box.ShowAsync();
        }

        public static async Task<ButtonResult> ShowStandardAsync(
            string title,
            string message,
            ButtonEnum buttons = ButtonEnum.Ok,
            Icon icon = Icon.None)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(title, message, buttons, icon);
            return await box.ShowAsync();
        }
    }
}