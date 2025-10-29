using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using SNESMiniLuaCompiler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SNESMiniLuaCompiler.Views
{
    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public class Builder
        {
            private string _title = "Message";
            private string _text = "";
            private Geometry? _iconGeometry;
            private IBrush? _iconForeground;
            private readonly List<(string Caption, MessageBoxResult Result, bool IsDefault, string? StyleClass)> _buttons = [];
            private ControlTheme? _buttonTheme;

            public Builder Title(string title) { _title = title; return this; }
            public Builder Text(string text) { _text = text; return this; }
            public Builder Icon(Geometry? geometry, IBrush? foreground = null) { _iconGeometry = geometry; _iconForeground = foreground; return this; }
            public Builder Button(string caption, MessageBoxResult result, bool isDefault = false, string? styleClass = null)
            { _buttons.Add((caption, result, isDefault, styleClass)); return this; }
            public Builder ButtonTheme(ControlTheme? theme) { _buttonTheme = theme; return this; }

            private void Validate()
            {
                if (_buttons.Count == 0)
                    throw new InvalidOperationException("At least one button must be added to the MessageBox.");
                if (_buttons.Count(b => b.IsDefault) > 1)
                    throw new InvalidOperationException("Only one button can be marked as default.");
                if (_buttons.Select(b => b.Caption).Distinct().Count() != _buttons.Count)
                    throw new InvalidOperationException("Button captions must be unique.");
            }

            public Task<MessageBoxResult> ShowAsync(Window? parent = null)
            {
                Validate();

                var msgbox = new MessageBox { Title = _title };
                msgbox.FindControl<TextBlock>("Text")!.Text = _text;

                var pathIcon = msgbox.FindControl<PathIcon>("NotificationIcon");
                if (pathIcon != null && _iconGeometry != null)
                {
                    pathIcon.Data = _iconGeometry;
                    pathIcon.IsVisible = true;
                    if (_iconForeground != null)
                        pathIcon.Foreground = _iconForeground;
                }

                var buttonPanel = msgbox.FindControl<StackPanel>("Buttons")!;
                var res = _buttons.FirstOrDefault(b => b.IsDefault).Result;

                void SetResult(MessageBoxResult r) => res = r;

                var styleOptions = new ButtonStyleOptions
                {
                    SolidTheme = Application.Current?.FindResource("SolidButton") as ControlTheme,
                    OutlineTheme = Application.Current?.FindResource("OutlineButton") as ControlTheme,
                };

                foreach (var (caption, result, isDefault, styleClass) in _buttons)
                {
                    AddButton(buttonPanel, msgbox, caption, result, SetResult, isDefault, styleClass, styleOptions);
                }

                var tcs = new TaskCompletionSource<MessageBoxResult>();
                msgbox.Closed += delegate { tcs.TrySetResult(res); };

                if (parent == null && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is Window mainWindow)
                    parent = mainWindow;

                if (parent != null)
                    msgbox.ShowDialog(parent);
                else
                    msgbox.Show();

                return tcs.Task;
            }
        }

        // --- Static Helper for Quick Use ---
        public static Task<MessageBoxResult> Show(string text, string title, MessageBoxButtons buttons)
        {
            var builder = new Builder().Title(title).Text(text);
            var solidButtonTheme = Application.Current?.FindResource("SolidButton") as ControlTheme;

            switch (buttons)
            {
                case MessageBoxButtons.Ok:
                    builder.Button("Ok", MessageBoxResult.Ok, true, "Primary");
                    break;
                case MessageBoxButtons.OkCancel:
                    builder.Button("Ok", MessageBoxResult.Ok, true, "Primary")
                           .Button("Cancel", MessageBoxResult.Cancel, false, "Tertiary");
                    break;
                case MessageBoxButtons.YesNo:
                    builder.Button("Yes", MessageBoxResult.Yes, true, "Primary")
                           .Button("No", MessageBoxResult.No, true, "Tertiary");
                    break;
                case MessageBoxButtons.YesNoCancel:
                    builder.Button("Yes", MessageBoxResult.Yes, false, "Primary")
                           .Button("No", MessageBoxResult.No, true, "Tertiary")
                           .Button("Cancel", MessageBoxResult.Cancel, false, "Tertiary");
                    break;
            }

            builder.ButtonTheme(solidButtonTheme);
            return builder.ShowAsync();
        }

        public static Task<MessageBoxResult> ShowError(string text, string title = "Error")
        {
            var builder = new Builder()
                .Title(title)
                .Text(text)
                .Icon(Application.Current?.FindResource("SemiIconAlertCircle") as Geometry,
                      Application.Current?.FindResource("NotificationCardErrorIconForeground") as IBrush)
                .Button("Ok", MessageBoxResult.Ok, true, "Danger")
                .ButtonTheme(Application.Current?.FindResource("SolidButton") as ControlTheme);

            return builder.ShowAsync();
        }

        public static Task<MessageBoxResult> ShowWarning(string text, string title = "Warning")
        {
            var builder = new Builder()
                .Title(title)
                .Text(text)
                .Icon(Application.Current?.FindResource("SemiIconAlertTriangle") as Geometry,
                      Application.Current?.FindResource("NotificationCardWarningIconForeground") as IBrush)
                .Button("Yes", MessageBoxResult.Yes, false, "Warning")
                .Button("No", MessageBoxResult.No, true, "Warning")
                .ButtonTheme(Application.Current?.FindResource("SolidButton") as ControlTheme)
                .ButtonTheme(Application.Current?.FindResource("OutlineButton") as ControlTheme);

            return builder.ShowAsync();
        }

        public static Task<MessageBoxResult> ShowSuccess(string text, string title = "Success")
        {
            var builder = new Builder()
                .Title(title)
                .Text(text)
                .Icon(Application.Current?.FindResource("SemiIconTickCircle") as Geometry,
                      Application.Current?.FindResource("NotificationCardSuccessIconForeground") as IBrush)
                .Button("Ok", MessageBoxResult.Ok, false, "Success")
                .ButtonTheme(Application.Current?.FindResource("SolidButton") as ControlTheme);

            return builder.ShowAsync();
        }

        public static Task<MessageBoxResult> ShowInformation(string text, string title = "Information")
        {
            var builder = new Builder()
                .Title(title)
                .Text(text)
                .Icon(Application.Current?.FindResource("SemiIconInfoCircle") as Geometry,
                      Application.Current?.FindResource("NotificationCardInformationIconForeground") as IBrush)
                .Button("Ok", MessageBoxResult.Ok, false, "Primary")
                .ButtonTheme(Application.Current?.FindResource("SolidButton") as ControlTheme);

            return builder.ShowAsync();
        }

        // --- Button Helper ---
        public class ButtonStyleOptions
        {
            public ControlTheme? SolidTheme { get; set; }
            public ControlTheme? OutlineTheme { get; set; }
        }

        private static void AddButton(
            StackPanel buttonPanel,
            MessageBox msgbox,
            string caption,
            MessageBoxResult resultValue,
            Action<MessageBoxResult> setResult,
            bool isDefault = false,
            string? styleClass = null,
            ButtonStyleOptions? styleOptions = null)
        {
            var btn = new Button { Content = caption };

            if (styleOptions != null)
            {
                if (!string.IsNullOrWhiteSpace(styleClass))
                    btn.Classes.Add(styleClass);

                // Apply custom themes
                if (isDefault && styleOptions.OutlineTheme != null)
                    btn.Theme = styleOptions.OutlineTheme;
                else if (styleOptions.SolidTheme != null)
                    btn.Theme = styleOptions.SolidTheme;
            }

            btn.Click += (_, __) =>
            {
                setResult(resultValue);
                msgbox.Close();
            };
            buttonPanel.Children.Add(btn);
            if (isDefault)
                setResult(resultValue);
        }
    }
}
