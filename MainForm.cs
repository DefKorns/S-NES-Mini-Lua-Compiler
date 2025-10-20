using SNESMiniLuaCompiler.Utils;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SNESMiniLuaCompiler
{
    public partial class MainForm : Form
    {
        private bool _dragging;
        private Point _start_point = new Point(0, 0);
        private readonly Image _backgroundImage;
        private string _selectedSystem;
        private bool _activeButton { get; set; }

        delegate void SetButtonCallback(Button button);

        public MainForm()
        {
            InitializeComponent();
            InitializeAppTitle();
            _backgroundImage = Properties.Resources.app_bg;
            InitializeBackground();
            InitializeFirstRunCheck();
            SetStyle(ControlStyles.ResizeRedraw, true);
            InitializeButtonStates();
        }

        #region Initialization
        /// <summary>
        /// Sets the application title with the assembly name and version.
        /// </summary>
        private void InitializeAppTitle()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            app_title.Text = $"{assembly.Name} {AppUtils.GetAppVersion()}";
        }

        /// <summary>
        /// Sets the default background image and enables double buffering.
        /// </summary>
        private void InitializeBackground()
        {
            DoubleBuffered = true;
            picLoader.Visible = false;
        }

        /// <summary>
        /// Initializes the state of buttons based on directory existence and active button status.
        /// </summary>
        private void InitializeButtonStates()
        {
            if (_activeButton) return;

            if (!Directory.Exists(AppUtils.RecodedPath) || !Directory.Exists(AppUtils.DecodedPath))
            {
                ConfigureButtonState(recode_button, 0);
            }

            ConfigureButtonState(decode_button, 0);
        }

        /// <summary>
        /// Checks if this is the first run and verifies Python installation.
        /// </summary>
        private static void InitializeFirstRunCheck()
        {
            const string PythonRequirementMessage = "Make sure you have python 3.x installed!\n\nPlease download it from python.org";

            ExceptionUtils.GlobalTryCatch(() =>
            {
                // Check if Python 3.x is installed
                if (!ProcessUtils.PythonVersion())
                {
                    MsgBox.Show(
                        PythonRequirementMessage,
                        "Requirement",
                        MsgBox.ButtonType.OK,
                        MsgBox.Ico.Application,
                        style: MsgBox.AnimateStyle.FadeInHelp
                    );
                }

                // Remove the first-run marker file if it exists
                if (FileUtils.SafeFileExists(AppUtils.FirstRun))
                {
                    File.Delete(AppUtils.FirstRun);
                }
            },
            "An error occurred during the first run check.",
            "MainForm.InitializeFirstRunCheck");
        }
        #endregion

        #region UI Event Handlers

        private void MainForm_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            fadeInTimer.Interval = 20;
            fadeInTimer.Tick += new EventHandler(FadeIn);
            fadeInTimer.Start();
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

        void FadeIn(object sender, EventArgs e)
        {
            if (Opacity >= 1)
                fadeInTimer.Stop();
            else
                Opacity += 0.05;
        }

        /// <summary>
        /// The methods Title_Header_Mouse* allows dragging and move the app around
        /// </summary>
        private void Title_Header_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _start_point = new Point(e.X, e.Y);
        }

        private void Title_Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;

            Point p = PointToScreen(e.Location);
            Location = new Point(p.X - _start_point.X, p.Y - _start_point.Y);
        }

        private void Title_Header_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            MsgBox.Show("- Select the icon for your desired system\n\n- Press the \"Decode\" button to proceed\n\n- After conclusion, navigate to the \"decoded\" folder and edit the lua files as desired\n\n- With all your files saved press the \"Encode\" button\n\n- Copy all the edited files to you theme's location", "Instructions", MsgBox.ButtonType.OK, MsgBox.Ico.Application, MsgBox.AnimateStyle.FadeInHelp);
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

        #endregion

        #region Button State Management

        /// <summary>
        /// Configures the state and border size of a button.
        /// </summary>
        /// <param name="button">The button to configure.</param>
        /// <param name="borderSize">The border size to apply to the button.</param>
        private static void ConfigureButtonState(Button button, int borderSize)
        {
            //EnableControls(new Control[] { button }, false);
            //button.FlatAppearance.BorderSize = borderSize;
            AppUtils.SetButtonState(button, false, borderSize);
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
                    //button.FlatAppearance.BorderSize = 0;
                    AppUtils.SetButtonState(button, button.Enabled, 0);
                }
            }
        }

        private void SelectedSystem_Click(object sender, EventArgs e)
        {
            ResetButtonBackColor();

            if (!(sender is Button btn)) return;

            const int BorderSize = 2;
            var activeBorderColor = Color.FromArgb(215, 152, 43);
            var inactiveBorderSize = 0;

            // Set button styles
            SetButtonStyle(btn, activeBorderColor, BorderSize);
            SetButtonStyle(decode_button, Color.FromArgb(81, 172, 56), BorderSize);
            SetButtonStyle(recode_button, Color.FromArgb(18, 108, 182), BorderSize);

            EnableControls(new Control[] { decode_button, recode_button }, true);

            //if (!Directory.Exists(DecodedPath) || !Directory.Exists(RecodedPath))
            if (!AppUtils.AreDecodedAndRecodedDirsPresent())
            {
                SetButtonStyle(recode_button, activeBorderColor, inactiveBorderSize);
                EnableControls(new Control[] { recode_button }, false);
            }

            _activeButton = true;

            _selectedSystem = AppUtils.GetSystemPath(btn.Name);
        }

        private static void SetButtonStyle(Button button, Color borderColor, int borderSize)
        {
            AppUtils.SetButtonState(button, button.Enabled, borderSize, borderColor);
        }

        private void EnableAllButtons(bool enable)
        {
            EnableControls(new Control[] { recode_button, famicomButton, famicom50Button, sFamicomButton, nesButton, snesPALButton, snesButton, decode_button }, enable);
        }

        /// <summary>
        /// Acts as angular disable="false" atribute
        /// </summary>
        /// <param name="con">Control to be enabled</param>
        private void EnableControls(Control[] controls, bool enable)
        {
            foreach (var con in controls)
            {
                if (con.InvokeRequired)
                {
                    SetButtonCallback d = new SetButtonCallback(c => EnableControls(new[] { c }, enable));
                    Invoke(d, new object[] { con });
                }
                else
                {
                    con.Enabled = enable;
                    if (con is Button btn)
                    {
                        if (btn != recode_button && btn != decode_button)
                        {
                            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(186, 132, 37);
                        }
                    }
                }
            }
        }

        #endregion

        #region Decrypt/Encrypt Logic

        private async void DecryptButton_Click(object sender, EventArgs e)
        {
            await DecryptFilesAsync().ConfigureAwait(false);

        }

        private async void EncryptButton_Click(object sender, EventArgs e)
        {
            await EncryptFilesAsync().ConfigureAwait(false);
        }

        private async Task DecryptFilesAsync()
        {
            EnableAllButtons(false);
            if (string.IsNullOrEmpty(ProcessUtils.FindExePath("python.exe")))
            {
                MsgBox.Show("Python 3.x wasn't found on your system's PATH.\nPlease install it from python.org", "Error", MsgBox.ButtonType.OK, MsgBox.Ico.Warning);
                return;
            }

            AppUtils.LoadSpinner(true, picLoader, this);
            await Task.Run(() =>
            {
                FileUtils.DeletePath(AppUtils.DecodedPath);
                FileUtils.CopyAssets(_selectedSystem, AppUtils.DecodedPath);
                FileUtils.DeleteFile(FileUtils.DecodedHashFile);
                Decrypt("decoded");
            }).ConfigureAwait(false);
            EnableAllButtons(true);
            decode_button.FlatAppearance.BorderSize = 2;
            recode_button.FlatAppearance.BorderSize = 2;
            AppUtils.LoadSpinner(false, picLoader, this);
            FileUtils.CreatePath(AppUtils.RecodedPath);

            Action showDialog = ShowDecryptionFinishedDialog;
            if (InvokeRequired)
                Invoke(showDialog);
            else
                showDialog();
        }

        private void ShowDecryptionFinishedDialog()
        {
            DialogResult dialog = MsgBox.Show(this,
                "The decryption process is complete.\n\nWould you like to open the output directory now?",
                "Success",
                MsgBox.ButtonType.YesNo,
                MsgBox.Ico.Application,
                MsgBox.AnimateStyle.FadeIn
            );

            if (dialog == DialogResult.Yes)
            {
                ProcessUtils.OpenDirectory(AppUtils.DecodedPath);
            }
        }

        private async Task EncryptFilesAsync()
        {
            ResetButtonBackColor();
            FileUtils.DeletePath(AppUtils.RecodedPath);
            _activeButton = false;

            EnableAllButtons(false);

            AppUtils.LoadSpinner(true, picLoader, this);

            // Run Encrypt on a background thread to make the method truly async
            await Task.Run(() => Encrypt("decoded")).ConfigureAwait(false);

            AppUtils.LoadSpinner(false, picLoader, this);

            EnableAllButtons(true);
            EnableControls(new Control[] { decode_button }, false);
            recode_button.FlatAppearance.BorderSize = 2;
        }

        private void Decrypt(string sDir)
        {
            foreach (string d in Directory.GetDirectories(sDir))
            {
                ExceptionUtils.GlobalTryCatch(
                    () => Decrypt(d),
                    $"Error decrypting directory '{d}'.",
                    "MainForm.Decrypt"
                );
            }

            foreach (string file in Directory.GetFiles(sDir))
            {
                string decFile = file + ".dec";
                ExceptionUtils.GlobalTryCatch(
                    () =>
                    {
                        ProcessUtils.RunCmd(ProcessUtils.FindExePath("pythonw.exe"), AppUtils.DecompilerScript + " --file " + file + " --output " + decFile + " --catch_asserts");
                        File.Delete(file);
                        File.Move(decFile, file);
                        FileUtils.GenerateFileHash(file);
                    },
                    $"Error decrypting file '{file}'.",
                    "MainForm.Decrypt"
                );
            }
        }

        private void Encrypt(string sDir)
        {
            foreach (string d in Directory.GetDirectories(sDir))
            {
                ExceptionUtils.GlobalTryCatch(
                    () => Encrypt(d),
                    $"Error encrypting directory '{d}'.",
                    "MainForm.Encrypt"
                );
            }

            foreach (string decodedFile in Directory.GetFiles(sDir))
            {
                string decodedFullPath = Path.GetFullPath(decodedFile);
                string recodedFullPath = Path.GetFullPath(Regex.Replace(decodedFullPath, "decoded", "recoded"));
                string recodedFile = Path.GetFullPath(Regex.Replace(decodedFullPath, "decoded", "recoded"));

                ExceptionUtils.GlobalTryCatch(
                    () =>
                    {
                        FileUtils.CreatePath(Path.GetDirectoryName(recodedFullPath));

                        if (FileUtils.HasEditedFiles(FileUtils.GetSHA256HashFromFile(decodedFullPath)))
                        {
                            ProcessUtils.RunLuaJit(decodedFullPath, recodedFile);
                        }
                    },
                    $"Error encrypting file '{decodedFile}'.",
                    "MainForm.Encrypt"
                );
            }

            ExceptionUtils.GlobalTryCatch(
                () => FileUtils.DeleteEmptyDirectories(AppUtils.RecodedPath),
                $"Error deleting empty directories in '{AppUtils.RecodedPath}'.",
                "MainForm.Encrypt"
            );
        }

        #endregion
    }
}
