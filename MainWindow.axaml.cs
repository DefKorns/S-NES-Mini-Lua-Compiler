using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SNESMiniLuaCompiler.Utils;

namespace SNESMiniLuaCompiler
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void HelloButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Window
            {
                Content = new TextBlock { Text = "Hello World!" },
                Width = 200,
                Height = 100
            };
            dialog.ShowDialog(this);
        }
        //private void InitializeAppTitle()
        //{
        //    var assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        //    app_title.Text = $"{assembly.Name} {AppUtils.GetAppVersion()}";
        //}
    }
}
