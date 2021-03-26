namespace SNESMiniLuaCompiler
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.app_exit = new System.Windows.Forms.Label();
            this.app_title = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.help = new System.Windows.Forms.Label();
            this.decode_button = new System.Windows.Forms.Button();
            this.recode_button = new System.Windows.Forms.Button();
            this.picLoader = new System.Windows.Forms.PictureBox();
            this.app_icon = new System.Windows.Forms.PictureBox();
            this.famicom50Button = new System.Windows.Forms.Button();
            this.sFamicomButton = new System.Windows.Forms.Button();
            this.famicomButton = new System.Windows.Forms.Button();
            this.snesPALButton = new System.Windows.Forms.Button();
            this.snesButton = new System.Windows.Forms.Button();
            this.nesButton = new System.Windows.Forms.Button();
            this.fadeOutTimer = new System.Windows.Forms.Timer(this.components);
            this.fadeInTimer = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLoader)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.app_icon)).BeginInit();
            this.SuspendLayout();
            // 
            // app_exit
            // 
            this.app_exit.AutoSize = true;
            this.app_exit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.app_exit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.app_exit.ForeColor = System.Drawing.Color.White;
            this.app_exit.Location = new System.Drawing.Point(569, 6);
            this.app_exit.Name = "app_exit";
            this.app_exit.Size = new System.Drawing.Size(15, 13);
            this.app_exit.TabIndex = 5;
            this.app_exit.Text = "X";
            this.app_exit.MouseClick += new System.Windows.Forms.MouseEventHandler(this.App_Exit_MouseClick);
            // 
            // app_title
            // 
            this.app_title.AutoSize = true;
            this.app_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.app_title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(95)))), ((int)(((byte)(95)))));
            this.app_title.Location = new System.Drawing.Point(31, 6);
            this.app_title.Name = "app_title";
            this.app_title.Size = new System.Drawing.Size(160, 13);
            this.app_title.TabIndex = 0;
            this.app_title.Text = "(S)NES Mini - Lua Compiler v1.0.1";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.help);
            this.panel1.Controls.Add(this.app_exit);
            this.panel1.Controls.Add(this.app_title);
            this.panel1.Location = new System.Drawing.Point(0, -1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(591, 25);
            this.panel1.TabIndex = 0;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Title_Header_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Title_Header_MouseMove);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Title_Header_MouseUp);
            // 
            // help
            // 
            this.help.AutoSize = true;
            this.help.Cursor = System.Windows.Forms.Cursors.Hand;
            this.help.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.help.ForeColor = System.Drawing.Color.White;
            this.help.Location = new System.Drawing.Point(549, 6);
            this.help.Name = "help";
            this.help.Size = new System.Drawing.Size(14, 13);
            this.help.TabIndex = 6;
            this.help.Text = "?";
            this.help.Click += new System.EventHandler(this.HelpButton_Click);
            // 
            // decode_button
            // 
            this.decode_button.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(41)))), ((int)(((byte)(45)))));
            this.decode_button.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(96)))), ((int)(((byte)(45)))));
            this.decode_button.FlatAppearance.BorderSize = 2;
            this.decode_button.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(49)))), ((int)(((byte)(54)))));
            this.decode_button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(49)))), ((int)(((byte)(54)))));
            this.decode_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.decode_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.decode_button.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(172)))), ((int)(((byte)(56)))));
            this.decode_button.Location = new System.Drawing.Point(79, 256);
            this.decode_button.Name = "decode_button";
            this.decode_button.Size = new System.Drawing.Size(170, 45);
            this.decode_button.TabIndex = 1;
            this.decode_button.Text = "Decode";
            this.decode_button.UseVisualStyleBackColor = false;
            this.decode_button.EnabledChanged += new System.EventHandler(this.BtnBackup_EnabledChanged);
            this.decode_button.Click += new System.EventHandler(this.DecryptButton_Click);
            this.decode_button.Paint += new System.Windows.Forms.PaintEventHandler(this.Button_Paint);
            // 
            // recode_button
            // 
            this.recode_button.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(41)))), ((int)(((byte)(45)))));
            this.recode_button.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(99)))), ((int)(((byte)(141)))));
            this.recode_button.FlatAppearance.BorderSize = 2;
            this.recode_button.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(49)))), ((int)(((byte)(54)))));
            this.recode_button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(49)))), ((int)(((byte)(54)))));
            this.recode_button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.recode_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.recode_button.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(140)))), ((int)(((byte)(203)))));
            this.recode_button.Location = new System.Drawing.Point(352, 256);
            this.recode_button.Name = "recode_button";
            this.recode_button.Size = new System.Drawing.Size(170, 45);
            this.recode_button.TabIndex = 2;
            this.recode_button.Text = "Encode";
            this.recode_button.UseVisualStyleBackColor = false;
            this.recode_button.EnabledChanged += new System.EventHandler(this.BtnRestore_EnabledChanged);
            this.recode_button.Click += new System.EventHandler(this.EncryptButton_Click);
            this.recode_button.Paint += new System.Windows.Forms.PaintEventHandler(this.Button_Paint);
            // 
            // picLoader
            // 
            this.picLoader.BackColor = System.Drawing.Color.Transparent;
            this.picLoader.Image = global::SNESMiniLuaCompiler.Properties.Resources.loading;
            this.picLoader.Location = new System.Drawing.Point(244, 140);
            this.picLoader.Name = "picLoader";
            this.picLoader.Size = new System.Drawing.Size(92, 92);
            this.picLoader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picLoader.TabIndex = 6;
            this.picLoader.TabStop = false;
            // 
            // app_icon
            // 
            this.app_icon.BackColor = System.Drawing.Color.Transparent;
            this.app_icon.Image = global::SNESMiniLuaCompiler.Properties.Resources.ico;
            this.app_icon.Location = new System.Drawing.Point(9, 4);
            this.app_icon.Name = "app_icon";
            this.app_icon.Size = new System.Drawing.Size(16, 16);
            this.app_icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.app_icon.TabIndex = 7;
            this.app_icon.TabStop = false;
            // 
            // famicom50Button
            // 
            this.famicom50Button.BackColor = System.Drawing.Color.Transparent;
            this.famicom50Button.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.famicom50Button.FlatAppearance.BorderSize = 0;
            this.famicom50Button.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(152)))), ((int)(((byte)(43)))));
            this.famicom50Button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(132)))), ((int)(((byte)(37)))));
            this.famicom50Button.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.famicom50Button.ForeColor = System.Drawing.Color.White;
            this.famicom50Button.Image = global::SNESMiniLuaCompiler.Properties.Resources.shonen;
            this.famicom50Button.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.famicom50Button.Location = new System.Drawing.Point(477, 142);
            this.famicom50Button.Name = "famicom50Button";
            this.famicom50Button.Size = new System.Drawing.Size(80, 88);
            this.famicom50Button.TabIndex = 14;
            this.famicom50Button.Text = "Famicom Shonen 50th";
            this.famicom50Button.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.famicom50Button.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.famicom50Button.UseVisualStyleBackColor = false;
            this.famicom50Button.Click += new System.EventHandler(this.SelectedSystem_Click);
            // 
            // sFamicomButton
            // 
            this.sFamicomButton.BackColor = System.Drawing.Color.Transparent;
            this.sFamicomButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.sFamicomButton.FlatAppearance.BorderSize = 0;
            this.sFamicomButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(152)))), ((int)(((byte)(43)))));
            this.sFamicomButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(132)))), ((int)(((byte)(37)))));
            this.sFamicomButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sFamicomButton.ForeColor = System.Drawing.Color.White;
            this.sFamicomButton.Image = global::SNESMiniLuaCompiler.Properties.Resources.sfamicom;
            this.sFamicomButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.sFamicomButton.Location = new System.Drawing.Point(381, 142);
            this.sFamicomButton.Name = "sFamicomButton";
            this.sFamicomButton.Size = new System.Drawing.Size(82, 88);
            this.sFamicomButton.TabIndex = 15;
            this.sFamicomButton.Text = "Super Famicom";
            this.sFamicomButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.sFamicomButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.sFamicomButton.UseVisualStyleBackColor = false;
            this.sFamicomButton.Click += new System.EventHandler(this.SelectedSystem_Click);
            // 
            // famicomButton
            // 
            this.famicomButton.BackColor = System.Drawing.Color.Transparent;
            this.famicomButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.famicomButton.FlatAppearance.BorderSize = 0;
            this.famicomButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(152)))), ((int)(((byte)(43)))));
            this.famicomButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(132)))), ((int)(((byte)(37)))));
            this.famicomButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.famicomButton.ForeColor = System.Drawing.Color.White;
            this.famicomButton.Image = global::SNESMiniLuaCompiler.Properties.Resources.famicom;
            this.famicomButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.famicomButton.Location = new System.Drawing.Point(293, 143);
            this.famicomButton.Name = "famicomButton";
            this.famicomButton.Size = new System.Drawing.Size(74, 88);
            this.famicomButton.TabIndex = 16;
            this.famicomButton.Text = "Famicom Mini";
            this.famicomButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.famicomButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.famicomButton.UseVisualStyleBackColor = false;
            this.famicomButton.Click += new System.EventHandler(this.SelectedSystem_Click);
            // 
            // snesPALButton
            // 
            this.snesPALButton.BackColor = System.Drawing.Color.Transparent;
            this.snesPALButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.snesPALButton.FlatAppearance.BorderSize = 0;
            this.snesPALButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(152)))), ((int)(((byte)(43)))));
            this.snesPALButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(132)))), ((int)(((byte)(37)))));
            this.snesPALButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.snesPALButton.ForeColor = System.Drawing.Color.White;
            this.snesPALButton.Image = global::SNESMiniLuaCompiler.Properties.Resources.snespal;
            this.snesPALButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.snesPALButton.Location = new System.Drawing.Point(209, 143);
            this.snesPALButton.Name = "snesPALButton";
            this.snesPALButton.Size = new System.Drawing.Size(70, 88);
            this.snesPALButton.TabIndex = 17;
            this.snesPALButton.Text = "SNES Mini (Europe)";
            this.snesPALButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.snesPALButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.snesPALButton.UseVisualStyleBackColor = false;
            this.snesPALButton.Click += new System.EventHandler(this.SelectedSystem_Click);
            // 
            // snesButton
            // 
            this.snesButton.BackColor = System.Drawing.Color.Transparent;
            this.snesButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.snesButton.FlatAppearance.BorderSize = 0;
            this.snesButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(152)))), ((int)(((byte)(43)))));
            this.snesButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(132)))), ((int)(((byte)(37)))));
            this.snesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.snesButton.ForeColor = System.Drawing.Color.White;
            this.snesButton.Image = global::SNESMiniLuaCompiler.Properties.Resources.snes;
            this.snesButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.snesButton.Location = new System.Drawing.Point(115, 143);
            this.snesButton.Name = "snesButton";
            this.snesButton.Size = new System.Drawing.Size(80, 88);
            this.snesButton.TabIndex = 18;
            this.snesButton.Text = "SNES Mini (USA)";
            this.snesButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.snesButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.snesButton.UseVisualStyleBackColor = false;
            this.snesButton.Click += new System.EventHandler(this.SelectedSystem_Click);
            // 
            // nesButton
            // 
            this.nesButton.BackColor = System.Drawing.Color.Transparent;
            this.nesButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.nesButton.FlatAppearance.BorderSize = 0;
            this.nesButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(152)))), ((int)(((byte)(43)))));
            this.nesButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(132)))), ((int)(((byte)(37)))));
            this.nesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.nesButton.ForeColor = System.Drawing.Color.White;
            this.nesButton.Image = global::SNESMiniLuaCompiler.Properties.Resources.nes;
            this.nesButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.nesButton.Location = new System.Drawing.Point(34, 143);
            this.nesButton.Name = "nesButton";
            this.nesButton.Size = new System.Drawing.Size(67, 88);
            this.nesButton.TabIndex = 19;
            this.nesButton.Text = "NES Mini";
            this.nesButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.nesButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.nesButton.UseVisualStyleBackColor = false;
            this.nesButton.Click += new System.EventHandler(this.SelectedSystem_Click);
            // 
            // fadeOutTimer
            // 
            this.fadeOutTimer.Tick += new System.EventHandler(this.FadeOutTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::SNESMiniLuaCompiler.Properties.Resources.app_bg;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(590, 336);
            this.Controls.Add(this.picLoader);
            this.Controls.Add(this.nesButton);
            this.Controls.Add(this.snesButton);
            this.Controls.Add(this.snesPALButton);
            this.Controls.Add(this.famicomButton);
            this.Controls.Add(this.sFamicomButton);
            this.Controls.Add(this.famicom50Button);
            this.Controls.Add(this.app_icon);
            this.Controls.Add(this.recode_button);
            this.Controls.Add(this.decode_button);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLoader)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.app_icon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label app_exit;
		private System.Windows.Forms.Label app_title;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button decode_button;
		private System.Windows.Forms.Button recode_button;
		private System.Windows.Forms.PictureBox picLoader;
        private System.Windows.Forms.PictureBox app_icon;
        private System.Windows.Forms.Button famicom50Button;
        private System.Windows.Forms.Button sFamicomButton;
        private System.Windows.Forms.Button famicomButton;
        private System.Windows.Forms.Button snesPALButton;
        private System.Windows.Forms.Button snesButton;
        private System.Windows.Forms.Button nesButton;
        private System.Windows.Forms.Label help;
        private System.Windows.Forms.Timer fadeOutTimer;
        private System.Windows.Forms.Timer fadeInTimer;
    }
}