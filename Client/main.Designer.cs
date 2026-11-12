namespace project_qlqa
{
    partial class main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(main));
            panelLogin = new Panel();
            btnOpenRegister = new LinkLabel();
            linkForgot = new LinkLabel();
            btnLogin = new Button();
            chkRemember = new CheckBox();
            txtPass = new TextBox();
            lblPass = new Label();
            txtUser = new TextBox();
            lblUser = new Label();
            lblSubTitle = new Label();
            lblTitle = new Label();
            picLogo = new PictureBox();
            panelLogin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            SuspendLayout();
            // 
            // panelLogin
            // 
            panelLogin.BackColor = Color.FromArgb(150, 255, 255, 255);
            panelLogin.Controls.Add(btnOpenRegister);
            panelLogin.Controls.Add(linkForgot);
            panelLogin.Controls.Add(btnLogin);
            panelLogin.Controls.Add(chkRemember);
            panelLogin.Controls.Add(txtPass);
            panelLogin.Controls.Add(lblPass);
            panelLogin.Controls.Add(txtUser);
            panelLogin.Controls.Add(lblUser);
            panelLogin.Controls.Add(lblSubTitle);
            panelLogin.Controls.Add(lblTitle);
            panelLogin.Controls.Add(picLogo);
            panelLogin.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 163);
            panelLogin.Location = new Point(365, 107);
            panelLogin.Name = "panelLogin";
            panelLogin.Size = new Size(314, 410);
            panelLogin.TabIndex = 0;
            panelLogin.Paint += panel1_Paint;
            // 
            // btnOpenRegister
            // 
            btnOpenRegister.AutoSize = true;
            btnOpenRegister.Font = new Font("Segoe UI", 12.75F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnOpenRegister.Location = new Point(80, 372);
            btnOpenRegister.Name = "btnOpenRegister";
            btnOpenRegister.RightToLeft = RightToLeft.No;
            btnOpenRegister.Size = new Size(157, 23);
            btnOpenRegister.TabIndex = 11;
            btnOpenRegister.TabStop = true;
            btnOpenRegister.Text = "Đăng ký tài khoản";
            btnOpenRegister.Click += btnOpenRegister_Click;
            // 
            // linkForgot
            // 
            linkForgot.AutoSize = true;
            linkForgot.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 163);
            linkForgot.Location = new Point(97, 338);
            linkForgot.Name = "linkForgot";
            linkForgot.Size = new Size(124, 21);
            linkForgot.TabIndex = 10;
            linkForgot.TabStop = true;
            linkForgot.Text = "Quên mật khẩu?";
            linkForgot.LinkClicked += linkForgot_LinkClicked;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = SystemColors.HotTrack;
            btnLogin.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnLogin.ForeColor = SystemColors.ButtonHighlight;
            btnLogin.Location = new Point(23, 286);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(270, 40);
            btnLogin.TabIndex = 9;
            btnLogin.Text = "Đăng nhập";
            btnLogin.UseVisualStyleBackColor = false;
            // 
            // chkRemember
            // 
            chkRemember.AutoSize = true;
            chkRemember.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 163);
            chkRemember.ForeColor = SystemColors.GrayText;
            chkRemember.Location = new Point(23, 250);
            chkRemember.Name = "chkRemember";
            chkRemember.Size = new Size(85, 17);
            chkRemember.TabIndex = 8;
            chkRemember.Text = "Ghi nhớ tôi";
            chkRemember.UseVisualStyleBackColor = true;
            // 
            // txtPass
            // 
            txtPass.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 163);
            txtPass.Location = new Point(23, 217);
            txtPass.Name = "txtPass";
            txtPass.Size = new Size(270, 27);
            txtPass.TabIndex = 7;
            txtPass.UseSystemPasswordChar = true;
            // 
            // lblPass
            // 
            lblPass.AutoSize = true;
            lblPass.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 163);
            lblPass.Location = new Point(23, 199);
            lblPass.Name = "lblPass";
            lblPass.Size = new Size(57, 15);
            lblPass.TabIndex = 6;
            lblPass.Text = "Mật khẩu";
            // 
            // txtUser
            // 
            txtUser.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 163);
            txtUser.Location = new Point(23, 155);
            txtUser.Name = "txtUser";
            txtUser.Size = new Size(270, 27);
            txtUser.TabIndex = 4;
            txtUser.TextChanged += textBox1_TextChanged;
            // 
            // lblUser
            // 
            lblUser.AutoSize = true;
            lblUser.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 163);
            lblUser.Location = new Point(23, 137);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(58, 15);
            lblUser.TabIndex = 3;
            lblUser.Text = "Tài khoản";
            // 
            // lblSubTitle
            // 
            lblSubTitle.AutoSize = true;
            lblSubTitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 163);
            lblSubTitle.ForeColor = SystemColors.GrayText;
            lblSubTitle.Location = new Point(80, 111);
            lblSubTitle.Name = "lblSubTitle";
            lblSubTitle.Size = new Size(137, 15);
            lblSubTitle.TabIndex = 2;
            lblSubTitle.Text = "Chào mừng bạn quay lại";
            lblSubTitle.Click += lblSubTitle_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 163);
            lblTitle.Location = new Point(23, 90);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(266, 21);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Đăng nhập vào tài khoản của bạn";
            lblTitle.Click += lblTitle_Click;
            // 
            // picLogo
            // 
            picLogo.BackColor = Color.Transparent;
            picLogo.Image = (Image)resources.GetObject("picLogo.Image");
            picLogo.InitialImage = null;
            picLogo.Location = new Point(80, 3);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(141, 97);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 0;
            picLogo.TabStop = false;
            // 
            // main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1050, 570);
            Controls.Add(panelLogin);
            Margin = new Padding(3, 2, 3, 2);
            MinimumSize = new Size(667, 460);
            Name = "main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản Lý Quán Ăn";
            Load += main_Load;
            panelLogin.ResumeLayout(false);
            panelLogin.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelLogin;
        private PictureBox picLogo;
        private Label lblTitle;
        private Label lblSubTitle;
        private TextBox txtUser;
        private Label lblUser;
        private TextBox txtPass;
        private Label lblPass;
        private Button btnLogin;
        private CheckBox chkRemember;
        private LinkLabel linkForgot;
        private LinkLabel btnOpenRegister;
    }
}
