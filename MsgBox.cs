using SNESMiniLuaCompiler.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace SNESMiniLuaCompiler
{

    public partial class MsgBox : Form
    {
        private const int CS_DROPSHADOW = 0x00020000;
        private readonly Panel _plHeader = new Panel();
        private readonly Panel _plFooter = new Panel();
        private readonly Panel _plIcon = new Panel();
        private readonly PictureBox _picIcon = new PictureBox();
        private readonly FlowLayoutPanel _flpButtons = new FlowLayoutPanel();
        private readonly Label _lblTitle;
        private readonly Label _lblMessage;
        private readonly List<Button> _buttonCollection = new List<Button>();
        private DialogResult _buttonResult;
        private Timer _timer;

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "<Pending>")]
        public MsgBox()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainForm));
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.FromArgb(39, 41, 45);
            StartPosition = FormStartPosition.CenterScreen;
            Padding = new Padding(3);

            Icon = (Icon)resources.GetObject("$this.Icon", CultureInfo.CurrentUICulture);

            _lblTitle = new Label
            {
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 18),
                Dock = DockStyle.Top,
                Height = 50
            };

            _lblMessage = new Label
            {
                ForeColor = Color.White,
                Font = new Font("Microsoft Sans Serif", 10),
                Dock = DockStyle.Fill,
                AutoSize = false,
                MaximumSize = new Size(500, 0), // Set a max width for wrapping
            };

            _flpButtons.FlowDirection = FlowDirection.RightToLeft;
            _flpButtons.Dock = DockStyle.Fill;

            _plHeader.Dock = DockStyle.Fill;
            _plHeader.Padding = new Padding(20);
            _plHeader.Controls.Add(_lblMessage);
            _plHeader.Controls.Add(_lblTitle);

            _plFooter.Dock = DockStyle.Bottom;
            _plFooter.Padding = new Padding(10);
            _plFooter.BackColor = Color.Transparent;
            _plFooter.Height = 60;
            _plFooter.Controls.Add(_flpButtons);

            _picIcon.Width = 38;
            _picIcon.Height = 38;
            _picIcon.Location = new Point(15, 15);
            _picIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            _picIcon.BringToFront();


            _plIcon.Dock = DockStyle.Left;
            _plIcon.Padding = new Padding(10);
            _plIcon.Width = 70;
            _plIcon.Controls.Add(_picIcon);

            Controls.Add(_plHeader);
            Controls.Add(_plIcon);
            Controls.Add(_plFooter);

        }

        public MsgBox(Panel plHeader, Panel plFooter, Panel plIcon, PictureBox picIcon, FlowLayoutPanel flpButtons, Label lblTitle, Label lblMessage, List<Button> buttonCollection, IContainer components)
        {
            _plHeader = plHeader;
            _plFooter = plFooter;
            _plIcon = plIcon;
            _picIcon = picIcon;
            _flpButtons = flpButtons;
            _lblTitle = lblTitle;
            _lblMessage = lblMessage;
            _buttonCollection = buttonCollection;
            this.components = components;
        }

        public static void Show(string message)
        {
            using (var msgBox = new MsgBox())
            {
                msgBox._lblMessage.Text = message;
                msgBox.ShowDialog();
                NativeMethods.MessageBeepNative(0);
            }
        }

        public static void Show(string message, string title)
        {
            ExceptionUtils.ThrowArgNull(message);
            using (var msgBox = new MsgBox())
            {
                msgBox._lblMessage.Text = message;
                msgBox._lblTitle.Text = title;
                msgBox.StartPosition = FormStartPosition.CenterParent;
                msgBox.Size = MessageSize(message);
                msgBox.ShowDialog();
                NativeMethods.MessageBeepNative(0);
            }
        }

        public static DialogResult Show(string message, string title, ButtonType buttons)
        {
            ExceptionUtils.ThrowArgNull(message);
            using (var msgBox = new MsgBox())
            {
                msgBox._lblMessage.Text = message;
                msgBox._lblTitle.Text = title;
                msgBox._plIcon.Hide();

                msgBox.InitButtons(buttons);

                msgBox.Size = MessageSize(message);
                msgBox.ShowDialog();
                NativeMethods.MessageBeepNative(0);
                return msgBox._buttonResult;
            }
        }

        public static DialogResult Show(string message, string title, ButtonType buttons, Ico icon)
        {
            ExceptionUtils.ThrowArgNull(message);
            using (var msgBox = new MsgBox())
            {
                msgBox._lblMessage.Text = message;
                msgBox._lblTitle.Text = title;

                msgBox.InitButtons(buttons);
                msgBox.InitIcon(icon);

                msgBox.Size = MessageSize(message);
                msgBox.ShowDialog();
                NativeMethods.MessageBeepNative(0);
                return msgBox._buttonResult;
            }
        }

        private static DialogResult ShowInternal(IWin32Window owner, string message, string title, ButtonType buttons, Ico icon, AnimateStyle style)
        {
            ExceptionUtils.ThrowArgNull(message);
            using (var msgBox = new MsgBox())
            {
                msgBox._lblMessage.Text = message;
                msgBox._lblTitle.Text = title;
                msgBox.Height = 0;

                msgBox.InitButtons(buttons);
                msgBox.InitIcon(icon);

                msgBox._timer = new Timer();
                Size formSize = MessageSize(message);

                switch (style)
                {
                    case AnimateStyle.SlideDown:
                        msgBox.Size = new Size(formSize.Width, 0);
                        msgBox._timer.Interval = 1;
                        break;
                    case AnimateStyle.FadeIn:
                        msgBox.AutoSize = true;
                        msgBox.AutoSizeMode = AutoSizeMode.GrowOnly;
                        msgBox.Size = new Size(formSize.Width + 100, formSize.Height);
                        msgBox.Opacity = 0;
                        msgBox._timer.Interval = 20;
                        break;
                    case AnimateStyle.FadeInHelp:
                        msgBox.Size = new Size(500, 330);
                        msgBox.Opacity = 0;
                        msgBox._timer.Interval = 20;
                        break;
                    case AnimateStyle.ZoomIn:
                        msgBox.Size = new Size(formSize.Width + 100, formSize.Height + 100);
                        msgBox._timer.Interval = 1;
                        break;
                    default: throw new InvalidOperationException($"Unknown {nameof(style)}: {style}");
                }
                msgBox._timer.Tag = new AnimateMsgBox(formSize, style);
                msgBox._timer.Tick += msgBox.Timer_Tick;
                msgBox._timer.Start();

                if (owner != null)
                    msgBox.ShowDialog(owner);
                else
                    msgBox.ShowDialog();

                NativeMethods.MessageBeepNative(0);
                return msgBox._buttonResult;
            }
        }

        public static DialogResult Show(string message, string title, ButtonType buttons, Ico icon, AnimateStyle style)
        {
            return ShowInternal(null, message, title, buttons, icon, style);
        }

        public static DialogResult Show(IWin32Window owner, string message, string title, ButtonType buttons, Ico icon, AnimateStyle style)
        {
            return ShowInternal(owner, message, title, buttons, icon, style);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Timer timer = (Timer)sender;
            AnimateMsgBox animate = (AnimateMsgBox)timer.Tag;
            bool stopTimer = false;

            switch (animate.Style)
            {
                case AnimateStyle.SlideDown:
                    int height = Height;
                    stopTimer = !UpdateValue(ref height, animate.FormSize.Height, 17);
                    Height = height;
                    break;

                case AnimateStyle.FadeIn:
                case AnimateStyle.FadeInHelp:
                    double opacity = Opacity;
                    stopTimer = !UpdateValue(ref opacity, 1.0, 0.1);
                    Opacity = opacity;
                    break;

                case AnimateStyle.ZoomIn:
                    int width = Width;
                    int zoomHeight = Height;
                    bool widthChanged = UpdateValue(ref width, animate.FormSize.Width, -17);
                    bool heightChanged = UpdateValue(ref zoomHeight, animate.FormSize.Height, -17);
                    Width = width;
                    Height = zoomHeight;
                    stopTimer = !(widthChanged || heightChanged);
                    break;
                default: throw new InvalidOperationException($"Unknown {nameof(animate.Style)}: {animate.Style}");
            }

            Invalidate();

            if (stopTimer)
            {
                _timer.Stop();
                _timer.Dispose();
            }
        }

        private static bool UpdateValue(ref int current, int target, int delta)
        {
            if ((delta > 0 && current < target) || (delta < 0 && current > target))
            {
                current += delta;
                if ((delta > 0 && current > target) || (delta < 0 && current < target))
                    current = target;
                return true;
            }
            return false;
        }

        private static bool UpdateValue(ref double current, double target, double delta)
        {
            if (current < target)
            {
                current += delta;
                if (current > target)
                    current = target;
                return true;
            }
            return false;
        }

        private void InitButtons(ButtonType buttons)
        {
            _buttonCollection.Clear();
            _flpButtons.Controls.Clear();

            string[] labels;
            switch (buttons)
            {
                case ButtonType.AbortRetryIgnore: labels = new[] { "Abort", "Retry", "Ignore" }; break;
                case ButtonType.OK: labels = new[] { "OK" }; break;
                case ButtonType.OKCancel: labels = new[] { "OK", "Cancel" }; break;
                case ButtonType.RetryCancel: labels = new[] { "Retry", "Cancel" }; break;
                case ButtonType.YesNo: labels = new[] { "Yes", "No" }; break;
                case ButtonType.YesNoCancel: labels = new[] { "Yes", "No", "Cancel" }; break;
                default: throw new InvalidOperationException($"Unknown {nameof(buttons)}: {buttons}");
            }

            foreach (var btn in CreateStyledButtons(labels))
                _flpButtons.Controls.Add(btn);
        }

        private static DialogResult GetDialogResultForLabel(string label)
        {
            switch (label)
            {
                case "Abort": return DialogResult.Abort;
                case "Retry": return DialogResult.Retry;
                case "Ignore": return DialogResult.Ignore;
                case "OK": return DialogResult.OK;
                case "Cancel": return DialogResult.Cancel;
                case "Yes": return DialogResult.Yes;
                case "No": return DialogResult.No;
                default: throw new InvalidOperationException($"Unknown {nameof(label)}: '{label}'");
            }
        }

        private IEnumerable<Button> CreateStyledButtons(IEnumerable<string> labels)
        {
            foreach (var label in labels)
            {
                var btn = new Button
                {
                    Text = label,
                    ForeColor = Color.FromArgb(170, 170, 170),
                    DialogResult = GetDialogResultForLabel(label),
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0))),
                    Padding = new Padding(3),
                    FlatStyle = FlatStyle.Flat,
                    Height = 30,
                    UseVisualStyleBackColor = false
                };
                btn.FlatAppearance.BorderSize = 2;
                btn.FlatAppearance.BorderColor = Color.FromArgb(99, 99, 98);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(47, 49, 54);
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(47, 49, 54);
                btn.Click += ButtonClick;
                _buttonCollection.Add(btn);
                yield return btn;
            }
        }

        private void InitIcon(Ico icon)
        {
            if (_picIcon.Image != null)
            {
                _picIcon.Image.Dispose();
                _picIcon.Image = null;
            }

            Image img;
            switch (icon)
            {
                case Ico.Application: img = Properties.Resources.icn_application; break;
                case Ico.Exclamation: img = Properties.Resources.icn_ok; break;
                case Ico.Error: img = Properties.Resources.icn_stop; break;
                case Ico.Info: img = Properties.Resources.icn_info; break;
                case Ico.Question: img = Properties.Resources.icn_question; break;
                case Ico.Shield:
                    using (var bmp = SystemIcons.Shield.ToBitmap())
                    {
                        img = new Bitmap(bmp);
                    }
                    break;
                case Ico.Warning: img = Properties.Resources.icn_warn; break;
                default: throw new InvalidOperationException($"Unknown {nameof(icon)}: '{icon}'");
            }
            _picIcon.Image = img;
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            _buttonResult = btn.DialogResult;
            Dispose();
        }

        private static Size MessageSize(string message)
        {
            const int MinWidth = 350;
            const int BaseHeight = 230;
            const int MaxLineLength = 180;
            const int WideWidth = 700;
            const int LineSpacing = 10;

            using (var font = new Font("Microsoft Sans Serif", 10))
            {
                // Use TextRenderer for better WinForms text measurement
                Size textSize = TextRenderer.MeasureText(message, font, new Size(WideWidth, 0), TextFormatFlags.WordBreak);

                int width = Math.Max(MinWidth, textSize.Width);
                int height = BaseHeight;

                if (message.Length >= 150)
                {
                    int lines = (int)Math.Ceiling((double)message.Length / MaxLineLength);
                    width = WideWidth;
                    height = BaseHeight + (textSize.Height + LineSpacing) * lines;
                }
                else
                {
                    height = BaseHeight + textSize.Height;
                }

                return new Size(width, height);
            }

        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ExceptionUtils.ThrowArgNull(e);
            base.OnPaint(e);

            using (var pen = new Pen(Color.FromArgb(0, 0, 0)))
            {
                Rectangle rect = new Rectangle(new Point(0, 0), new Size(Width - 1, Height - 1));
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        public enum ButtonType
        {
            AbortRetryIgnore = 1,
            OK = 2,
            OKCancel = 3,
            RetryCancel = 4,
            YesNo = 5,
            YesNoCancel = 6
        }

        public enum Ico
        {
            Application = 1,
            Exclamation = 2,
            Error = 3,
            Warning = 4,
            Info = 5,
            Question = 6,
            Shield = 7,
            Search = 8
        }

        public enum AnimateStyle
        {
            SlideDown = 1,
            FadeIn = 2,
            ZoomIn = 3,
            FadeInHelp = 4
        }

    }

    class AnimateMsgBox
    {
        public Size FormSize;
        public MsgBox.AnimateStyle Style;

        public AnimateMsgBox(Size formSize, MsgBox.AnimateStyle style)
        {
            FormSize = formSize;
            Style = style;
        }
    }
    public static class NativeMethods
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool MessageBeep(uint type);

        public static void MessageBeepNative(uint type)
        {
            MessageBeep(type);
        }
    }
}

