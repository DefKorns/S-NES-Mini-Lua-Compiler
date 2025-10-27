using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SNESMiniLuaCompiler.Helpers;
using SNESMiniLuaCompiler.Models;
using System;
using System.Threading.Tasks;

namespace SNESMiniLuaCompiler.Views
{
    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static Task<MessageBoxResult> Show(string text, string title, MessageBoxButtons buttons)
        {
            // Always use MainWindow if available
            Window? parent = null;
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is Window mainWindow)
            {
                parent = mainWindow;
            }
            var msgbox = new MessageBox()
            {
                Title = title
            };
            msgbox.FindControl<TextBlock>("Text")!.Text = text;
            var buttonPanel = msgbox.FindControl<StackPanel>("Buttons")!;

            var res = MessageBoxResult.Ok;

            void AddButton(string caption, MessageBoxResult r, bool def = false)
            {
                var btn = new Button { Content = caption };
                btn.Click += (_, __) =>
                {
                    res = r;
                    msgbox.Close();
                };
                buttonPanel.Children.Add(btn);
                if (def)
                    res = r;
            }

            if (buttons == MessageBoxButtons.Ok || buttons == MessageBoxButtons.OkCancel)
                AddButton("Ok", MessageBoxResult.Ok, true);
            if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel)
            {
                AddButton("Yes", MessageBoxResult.Yes);
                AddButton("No", MessageBoxResult.No, true);
            }

            if (buttons == MessageBoxButtons.OkCancel || buttons == MessageBoxButtons.YesNoCancel)
                AddButton("Cancel", MessageBoxResult.Cancel, true);


            var tcs = new TaskCompletionSource<MessageBoxResult>();
            msgbox.Closed += delegate { tcs.TrySetResult(res); };

            if (parent != null)
                msgbox.ShowDialog(parent);
            else msgbox.Show();
            return tcs.Task;
        }

        public static Task<MessageBoxResult> ShowError(string text, string title = "Error")
        {
            Window? parent = null;
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is Window mainWindow)
            {
                parent = mainWindow;
            }
            var msgbox = new MessageBox()
            {
                Title = title,
                //Icon = new WindowIcon("avares://SNESMiniLuaCompiler/Assets/avalonia-logo.ico")
                ////Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red)
            };
            msgbox.FindControl<TextBlock>("Text")!.Text = text;
            msgbox.FindControl<Image>("Icon")!.Source = ImageHelper.LoadFromResource(new Uri("avares://SNESMiniLuaCompiler/Assets/icn_warn.png"));
            var buttonPanel = msgbox.FindControl<StackPanel>("Buttons")!;

            var res = MessageBoxResult.Ok;

            void AddButton(string caption, MessageBoxResult r, bool def = false)
            {
                var btn = new Button { Content = caption };
                btn.Click += (_, __) =>
                {
                    res = r;
                    msgbox.Close();
                };
                buttonPanel.Children.Add(btn);
                if (def)
                    res = r;
            }

            AddButton("Ok", MessageBoxResult.Ok, true);

            var tcs = new TaskCompletionSource<MessageBoxResult>();
            msgbox.Closed += delegate { tcs.TrySetResult(res); };
            if (parent != null)
                msgbox.ShowDialog(parent);
            else msgbox.Show();
            return tcs.Task;
        }

    }
}
