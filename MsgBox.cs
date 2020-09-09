using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace SNESMiniLuaCompiler
{

public partial class MsgBox : Form
    {
        private const int CS_DROPSHADOW = 0x00020000;
        private static MsgBox _msgBox;
        private readonly Panel _plHeader = new Panel();
        private readonly Panel _plFooter = new Panel();
        private readonly Panel _plIcon = new Panel();
        private readonly PictureBox _picIcon = new PictureBox();
        private readonly FlowLayoutPanel _flpButtons = new FlowLayoutPanel();
        private readonly Label _lblTitle;
        private readonly Label _lblMessage;
        private readonly List<Button> _buttonCollection = new List<Button>();
        private static DialogResult _buttonResult;
        private static Timer _timer;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "<Pending>")]
        public MsgBox()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.FromArgb(39, 41, 45);
            StartPosition = FormStartPosition.CenterScreen;
            Padding = new Padding(3);
            Width = 400;

            Icon = (Icon)resources.GetObject("$this.Icon");


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
                Dock = DockStyle.Fill
            };

            _flpButtons.FlowDirection = FlowDirection.RightToLeft;
            _flpButtons.Dock = DockStyle.Fill;

            _plHeader.Dock = DockStyle.Fill;
            _plHeader.Padding = new Padding(20);
            _plHeader.Controls.Add(_lblMessage);
            _plHeader.Controls.Add(_lblTitle);

            _plFooter.Dock = DockStyle.Bottom;
            _plFooter.Padding = new Padding(20);
            _plFooter.BackColor = Color.Transparent;
            _plFooter.Height = 80;
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
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            _msgBox.ShowDialog();
            NativeMethods.MessageBeepNative(0);
        }

        public static void Show(string message, string title)
        {
            AppUtils.ThrowArgNull(message);
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            _msgBox._lblTitle.Text = title;
            _msgBox.StartPosition = FormStartPosition.CenterParent;
            _msgBox.Size = MessageSize(message);
            _msgBox.ShowDialog();
            NativeMethods.MessageBeepNative(0);
        }

        public static DialogResult Show(string message, string title, ButtonType buttons)
        {
            AppUtils.ThrowArgNull(message);
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            _msgBox._lblTitle.Text = title;
            _msgBox._plIcon.Hide();

            InitButtons(buttons);

            _msgBox.Size = MessageSize(message);
            _msgBox.ShowDialog();
            NativeMethods.MessageBeepNative(0);
            return _buttonResult;
        }

        public static DialogResult Show(string message, string title, ButtonType buttons, Ico icon)
        {
            AppUtils.ThrowArgNull(message);
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            _msgBox._lblTitle.Text = title;

            InitButtons(buttons);
            InitIcon(icon);

            _msgBox.Size = MsgBox.MessageSize(message);
            _msgBox.ShowDialog();
            NativeMethods.MessageBeepNative(0);
            return _buttonResult;
        }

        public static DialogResult Show(string message, string title, ButtonType buttons, Ico icon, AnimateStyle style)
        {
            AppUtils.ThrowArgNull(message);
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            _msgBox._lblTitle.Text = title;
            _msgBox.Height = 0;

            InitButtons(buttons);
            InitIcon(icon);

            _timer = new Timer();
            Size formSize = MessageSize(message);

            switch (style)
            {
                case AnimateStyle.SlideDown:
                    _msgBox.Size = new Size(formSize.Width, 0);
                    _timer.Interval = 1;
                    _timer.Tag = new AnimateMsgBox(formSize, style);
                    break;

                case AnimateStyle.FadeIn:
                    _msgBox.Size = formSize;
                    _msgBox.Opacity = 0;
                    _timer.Interval = 20;
                    _timer.Tag = new AnimateMsgBox(formSize, style);
                    break;

                case AnimateStyle.ZoomIn:
                    _msgBox.Size = new Size(formSize.Width + 100, formSize.Height + 100);
                    _timer.Tag = new AnimateMsgBox(formSize, style);
                    _timer.Interval = 1;
                    break;
            }

            _timer.Tick += Timer_Tick;
            _timer.Start();

            _msgBox.ShowDialog();
            NativeMethods.MessageBeepNative(0);
            return _buttonResult;
        }

        static void Timer_Tick(object sender, EventArgs e)
        {
            Timer timer = (Timer)sender;
            AnimateMsgBox animate = (AnimateMsgBox)timer.Tag;

            switch (animate.Style)
            {
                case AnimateStyle.SlideDown:
                    if (_msgBox.Height < animate.FormSize.Height)
                    {
                        _msgBox.Height += 17;
                        _msgBox.Invalidate();
                    }
                    else
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }
                    break;

                case AnimateStyle.FadeIn:
                    if (_msgBox.Opacity < 1)
                    {
                        _msgBox.Opacity += 0.1;
                        _msgBox.Invalidate();
                    }
                    else
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }
                    break;

                case AnimateStyle.ZoomIn:
                    if (_msgBox.Width > animate.FormSize.Width)
                    {
                        _msgBox.Width -= 17;
                        _msgBox.Invalidate();
                    }
                    if (_msgBox.Height > animate.FormSize.Height)
                    {
                        _msgBox.Height -= 17;
                        _msgBox.Invalidate();
                    }
                    break;
            }
        }

        private static void InitButtons(ButtonType buttons)
        {
            switch (buttons)
            {
                case ButtonType.AbortRetryIgnore:
                    _msgBox.InitAbortRetryIgnoreButtons();
                    break;

                case ButtonType.OK:
                    _msgBox.InitOKButton();
                    break;

                case ButtonType.OKCancel:
                    _msgBox.InitOKCancelButtons();
                    break;

                case ButtonType.RetryCancel:
                    _msgBox.InitRetryCancelButtons();
                    break;

                case ButtonType.YesNo:
                    _msgBox.InitYesNoButtons();
                    break;

                case ButtonType.YesNoCancel:
                    _msgBox.InitYesNoCancelButtons();
                    break;
            }

            foreach (Button btn in _msgBox._buttonCollection)
            {
                btn.ForeColor = Color.FromArgb(170, 170, 170);
                btn.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
                btn.Padding = new Padding(3);
                btn.FlatStyle = FlatStyle.Flat;
                btn.Height = 30;
                btn.FlatAppearance.BorderSize = 2;
                btn.FlatAppearance.BorderColor = Color.FromArgb(99, 99, 98);
                //btn.Size = new Size(77, 28);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(49)))), ((int)(((byte)(54)))));
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(49)))), ((int)(((byte)(54)))));
                btn.UseVisualStyleBackColor = false;

                _msgBox._flpButtons.Controls.Add(btn);
            }
        }

        private static void InitIcon(Ico icon)
        {
            switch (icon)
            {
                case Ico.Application:
                    _msgBox._picIcon.Image = Properties.Resources.icn_application;
                    break;

                case Ico.Exclamation:
                    _msgBox._picIcon.Image = Properties.Resources.icn_ok;
                    break;

                case Ico.Error:
                    _msgBox._picIcon.Image = Properties.Resources.icn_stop;
                    break;

                case Ico.Info:
                    _msgBox._picIcon.Image = Properties.Resources.icn_info;
                    break;

                case Ico.Question:
                    _msgBox._picIcon.Image = Properties.Resources.icn_question;
                    break;

                case Ico.Shield:
                    _msgBox._picIcon.Image = SystemIcons.Shield.ToBitmap();
                    break;

                case Ico.Warning:
                    _msgBox._picIcon.Image = Properties.Resources.icn_warn;
                    break;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        private void InitAbortRetryIgnoreButtons()
        {
            Button btnAbort = new Button
            {
                Text = "Abort"
            };
            btnAbort.Click += ButtonClick;

            Button btnRetry = new Button
            {
                Text = "Retry"
            };
            btnRetry.Click += ButtonClick;

            Button btnIgnore = new Button
            {
                Text = "Ignore"
            };
            btnIgnore.Click += ButtonClick;

            _buttonCollection.Add(btnAbort);
            _buttonCollection.Add(btnRetry);
            _buttonCollection.Add(btnIgnore);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        private void InitOKButton()
        {
            Button btnOK = new Button
            {
                Text = "OK"
            };
            btnOK.Click += ButtonClick;

            _buttonCollection.Add(btnOK);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        private void InitOKCancelButtons()
        {
            Button btnOK = new Button
            {
                Text = "OK"
            };
            btnOK.Click += ButtonClick;

            Button btnCancel = new Button
            {
                Text = "Cancel"
            };
            btnCancel.Click += ButtonClick;


            _buttonCollection.Add(btnOK);
            _buttonCollection.Add(btnCancel);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        private void InitRetryCancelButtons()
        {
            Button btnRetry = new Button
            {
                Text = "OK"
            };
            btnRetry.Click += ButtonClick;

            Button btnCancel = new Button
            {
                Text = "Cancel"
            };
            btnCancel.Click += ButtonClick;


            _buttonCollection.Add(btnRetry);
            _buttonCollection.Add(btnCancel);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        private void InitYesNoButtons()
        {
            Button btnYes = new Button
            {
                Text = "Yes"
            };
            btnYes.Click += ButtonClick;

            Button btnNo = new Button
            {
                Text = "No"
            };
            btnNo.Click += ButtonClick;


            _buttonCollection.Add(btnYes);
            _buttonCollection.Add(btnNo);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        private void InitYesNoCancelButtons()
        {
            Button btnYes = new Button
            {
                Text = "Abort"
            };
            btnYes.Click += ButtonClick;

            Button btnNo = new Button
            {
                Text = "Retry"
            };
            btnNo.Click += ButtonClick;

            Button btnCancel = new Button
            {
                Text = "Cancel"
            };
            btnCancel.Click += ButtonClick;

            _buttonCollection.Add(btnYes);
            _buttonCollection.Add(btnNo);
            _buttonCollection.Add(btnCancel);
        }

        private static void ButtonClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            switch (btn.Text)
            {
                case "Abort":
                    _buttonResult = DialogResult.Abort;
                    break;

                case "Retry":
                    _buttonResult = DialogResult.Retry;
                    break;

                case "Ignore":
                    _buttonResult = DialogResult.Ignore;
                    break;

                case "OK":
                    _buttonResult = DialogResult.OK;
                    break;

                case "Cancel":
                    _buttonResult = DialogResult.Cancel;
                    break;

                case "Yes":
                    _buttonResult = DialogResult.Yes;
                    break;

                case "No":
                    _buttonResult = DialogResult.No;
                    break;
            }

            _msgBox.Dispose();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        private static Size MessageSize(string message)
        {
            Graphics g = _msgBox.CreateGraphics();
            int width = 350;
            int height = 230;

            SizeF size = g.MeasureString(message, new Font("Microsoft Sans Serif", 10));

            if (message.Length < 150)
            {
                if ((int)size.Width > 350)
                {
                    width = (int)size.Width;
                }
            }
            else
            {
                string[] groups = (from Match m in Regex.Matches(message, ".{1,180}") select m.Value).ToArray();
                int lines = groups.Length + 1;
                width = 700;
                height += (int)(size.Height + 10) * lines;
            }
            return new Size(width, height);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
        protected override void OnPaint(PaintEventArgs e)
        {
            AppUtils.ThrowArgNull(e);
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Rectangle rect = new Rectangle(new Point(0, 0), new Size(Width - 1, Height - 1));
            Pen pen = new Pen(Color.FromArgb(0, 0, 0));

            g.DrawRectangle(pen, rect);
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
            ZoomIn = 3
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

