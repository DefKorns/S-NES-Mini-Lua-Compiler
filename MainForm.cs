using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SNESMiniLuaCompiler
{
    public partial class MainForm : Form
    {
        private bool _dragging;
        private Point _start_point = new Point(0, 0);
        private readonly Image backgroundImage;
        private string SelectecSystem { get; set; }
        private bool ActiveButton { get; set; }
        delegate void SetButtonCallback(Button button);

        public MainForm()
        {

            InitializeComponent();
            backgroundImage = Properties.Resources.app_bg;
            DoubleBuffered = true;
            picLoader.Visible = false;

            if (File.Exists(AppUtils.firstRun))
            {
                MsgBox.Show("Make sure you have python 3.x installed!\n\nPlease download it from python.org", "Requirement", MsgBox.ButtonType.OK, MsgBox.Ico.Application, MsgBox.AnimateStyle.FadeIn);
                File.Delete(AppUtils.firstRun);
            }

            SetStyle(ControlStyles.ResizeRedraw, true);

            if (!ActiveButton)
            {
                if (!Directory.Exists(AppUtils.recodedPath) || !Directory.Exists(AppUtils.decodedPath))
                {
                    DisableControls(recode_button);
                    recode_button.FlatAppearance.BorderSize = 0;
                }
                DisableControls(decode_button);
                decode_button.FlatAppearance.BorderSize = 0;
            }
        }

        /// <summary>
        /// The methods Title_Header_Mouse* allows dragging and move the app around
        /// </summary>
        private void Title_Header_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _start_point = new Point(e.X, e.Y);
        }
        private void Title_Header_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }
        private void Title_Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - _start_point.X, p.Y - _start_point.Y);
            }
        }
        /// <summary>
        /// Close application by clicking on the 'X'
        /// </summary>
        private void App_Exit_MouseClick(object sender, MouseEventArgs e)
        {
            DialogResult dialog = MsgBox.Show("Are you sure you want to exit?", "Exit application", MsgBox.ButtonType.YesNo, MsgBox.Ico.Warning, MsgBox.AnimateStyle.FadeIn);

            if (dialog == DialogResult.Yes)
            {
                fadeOutTimer.Start();
            }

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            fadeInTimer.Interval = 20;
            fadeInTimer.Tick += new EventHandler(FadeIn);
            fadeInTimer.Start();
        }

        void FadeIn(object sender, EventArgs e)
        {
            if (Opacity >= 1)
                fadeInTimer.Stop();
            else
                Opacity += 0.05;
        }

        private void FadeOutTimer_Tick(object sender, EventArgs e)
        {
            fadeOutTimer.Interval = 20;
            if (Opacity > 0.0)
            {
                Opacity -= 0.05;
            }
            else
            {
                fadeOutTimer.Stop();
                Application.Exit();
            }
        }
        /// <summary>
        /// Acts as angular disable atribute
        /// </summary>
        /// <param name="con">Control to be disabled</param>
        private void DisableControls(Control con)
        {
            if (con.InvokeRequired)
            {
                SetButtonCallback d = new SetButtonCallback(DisableControls);
                Invoke(d, new object[] { con });
            }
            else
            {
                con.Enabled = false;
            }
        }
        /// <summary>
        /// Acts as angular disable="false" atribute
        /// </summary>
        /// <param name="con">Control to be enabled</param>
        private void EnableControls(Control con)
        {
            if (con.InvokeRequired)
            {
                SetButtonCallback d = new SetButtonCallback(EnableControls);
                Invoke(d, new object[] { con });
            }
            else
            {
                con.Enabled = true;
            }

        }
        private void BtnRestore_EnabledChanged(object sender, EventArgs e)
        {
            recode_button.ForeColor = recode_button.Enabled ? Color.FromArgb(68, 140, 203) : Color.FromArgb(81, 113, 127);
        }
        private void BtnBackup_EnabledChanged(object sender, EventArgs e)
        {
            decode_button.ForeColor = decode_button.Enabled ? Color.FromArgb(81, 172, 56) : Color.FromArgb(93, 129, 83);
        }
        private void Button_Paint(object sender, PaintEventArgs e)
        {
            var btn = (Button)sender;
            var drawBrush = new SolidBrush(btn.ForeColor);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            e.Graphics.DrawString(btn.Text, btn.Font, drawBrush, e.ClipRectangle, sf);
            drawBrush.Dispose();
            sf.Dispose();
        }
        /// <summary>
        /// Restore default backgroundcolor to buttons
        /// </summary>
        private void ResetButtonBackColor()
        {
            foreach (object item in Controls)
            {
                if (item is Button button)
                {
                    button.FlatAppearance.BorderSize = 0;
                }
            }
        }
        private void HelpButton_Click(object sender, EventArgs e)
        {
            MsgBox.Show("- Select the icon for your desired system\n\n- Press the \"Decode\" button to proceed\n\n- After conclusion, navigate to the \"decoded\" folder and edit the lua files as desired\n\n- With all your files saved press the \"Encode\" button\n\n- Copy all the edited files to you theme's location", "Instructions", MsgBox.ButtonType.OK, MsgBox.Ico.Application, MsgBox.AnimateStyle.FadeInHelp);
        }
        private void SelectedSystem_Click(object sender, EventArgs e)
        {
            ResetButtonBackColor();
            Button btn = (sender as Button);
            btn.FlatAppearance.BorderColor = Color.FromArgb(215, 152, 43);//Color.FromArgb(110, 86, 48);
            btn.FlatAppearance.BorderSize = 2;
            decode_button.FlatAppearance.BorderSize = 2;
            recode_button.FlatAppearance.BorderSize = 2;
            EnableControls(decode_button);
            EnableControls(recode_button);
            if (!Directory.Exists(AppUtils.decodedPath) || !Directory.Exists(AppUtils.recodedPath))
            {
                recode_button.FlatAppearance.BorderSize = 0;
                DisableControls(recode_button);
            }

            ActiveButton = true;

            switch (btn.Name)
            {
                case "famicomButton":
                    SelectecSystem = Path.Combine(AppUtils.originalPath, "hvc");
                    break;
                case "famicom50Button":
                    SelectecSystem = Path.Combine(AppUtils.originalPath, "hvcj");
                    break;
                case "sFamicomButton":
                    SelectecSystem = Path.Combine(AppUtils.originalPath, "shvc");
                    break;
                case "nesButton":
                    SelectecSystem = Path.Combine(AppUtils.originalPath, "nes");
                    break;
                case "snesPALButton":
                    SelectecSystem = Path.Combine(AppUtils.originalPath, "snes-eur");
                    break;
                default:
                    SelectecSystem = Path.Combine(AppUtils.originalPath, "snes-usa");
                    break;

            }
        }
        private void DecryptButton_Click(object sender, EventArgs e)
        {
            ThreadLauncher(DecryptFiles);

        }
        private void EncryptButton_Click(object sender, EventArgs e)
        {
            ThreadLauncher(EncryptFiles);
        }
        private void Decrypt(string sDir)
        {
            foreach (string d in Directory.GetDirectories(sDir))
            {
                Decrypt(d);
            }
            foreach (string file in Directory.GetFiles(sDir))
            {
                string decFile = file + ".dec";
                AppUtils.RunCmd(AppUtils.FindExePath("pythonw.exe"), AppUtils.decompilerScript + " --file " + file + " --output " + decFile + " --catch_asserts");
                File.Delete(file);
                File.Move(decFile, file);
                AppUtils.GenerateFileHash(file);

            }

        }
        private void Encrypt(string sDir)
        {
            foreach (string d in Directory.GetDirectories(sDir))
            {
                Encrypt(d);
            }

            foreach (string decodedFile in Directory.GetFiles(sDir))
            {

                string decodedFullPath = Path.GetDirectoryName(decodedFile);
                string recodedFullPath = Regex.Replace(decodedFullPath, "decoded", "recoded");
                string recodedFile = Regex.Replace(decodedFile, "decoded", "recoded");

                AppUtils.CreatePath(recodedFullPath);
                AppUtils.CreateLuaJitLauncher();

                if (AppUtils.HasEditedFiles(AppUtils.GetSHA256HashFromFile(decodedFile)))
                {
                    AppUtils.RunLuaJit(decodedFile + " " + recodedFile);
                }
                AppUtils.DeleteFile(AppUtils.batFile);
            }
        }

        private void DisableAllButtons()
        {
            DisableControls(recode_button);
            DisableControls(decode_button);
            DisableControls(famicomButton);
            DisableControls(famicom50Button);
            DisableControls(sFamicomButton);
            DisableControls(nesButton);
            DisableControls(snesPALButton);
            DisableControls(snesButton);

        }
        private void EnableAllButtons()
        {
            EnableControls(recode_button);
            EnableControls(famicomButton);
            EnableControls(famicom50Button);
            EnableControls(sFamicomButton);
            EnableControls(nesButton);
            EnableControls(snesPALButton);
            EnableControls(snesButton);
            EnableControls(decode_button);
        }
        private void DecryptFiles()
        {
            DisableAllButtons();

            if (!string.IsNullOrEmpty(AppUtils.FindExePath("python.exe")))
            {
                AppUtils.LoadSpinner(true, picLoader, this);
                AppUtils.DeletePath(AppUtils.decodedPath);
                AppUtils.CopyAssets(SelectecSystem, AppUtils.decodedPath);
                AppUtils.DeleteFile(AppUtils.decodedHashFile);

                Decrypt("decoded");

                EnableAllButtons();
                decode_button.FlatAppearance.BorderSize = 2;
                recode_button.FlatAppearance.BorderSize = 2;
                AppUtils.LoadSpinner(false, picLoader, this);
                AppUtils.CreatePath(AppUtils.recodedPath);
                DialogResult dialog = MsgBox.Show("The decrytion has finished.\n\nDo you want to navigate to the output directory?", "Success", MsgBox.ButtonType.YesNo, MsgBox.Ico.Application, MsgBox.AnimateStyle.FadeIn);

                if (dialog == DialogResult.Yes)
                {
                    Process.Start(@AppUtils.decodedPath);
                }
            }
            else
            {
                MsgBox.Show("Python 3.x wasn't found on your system's PATH.\nPlease install it from python.org", "Error", MsgBox.ButtonType.OK, MsgBox.Ico.Warning);
            }

        }
        private void EncryptFiles()
        {
            ResetButtonBackColor();
            AppUtils.DeletePath(AppUtils.recodedPath);
            ActiveButton = false;

            DisableAllButtons();

            AppUtils.LoadSpinner(true, picLoader, this);

            Encrypt("decoded");

            AppUtils.LoadSpinner(false, picLoader, this);

            EnableAllButtons();
            DisableControls(decode_button);
            recode_button.FlatAppearance.BorderSize = 2;
        }
        private static void ThreadLauncher(ThreadStart data)
        {
            try
            {
                Thread threadInput = new Thread(data);
                threadInput.Start();

            }
            catch (ThreadStateException ex)
            {
                MsgBox.Show(ex.Message, "Error", MsgBox.ButtonType.OK, MsgBox.Ico.Warning);
            }
        }
    }
}
