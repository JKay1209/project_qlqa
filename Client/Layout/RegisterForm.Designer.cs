namespace Client.Layout
{
    partial class RegisterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegisterForm));
            lblTitle = new Label();
            btnRegister = new Button();
            txtUsername = new TextBox();
            txtEmail = new TextBox();
            txtPassword = new TextBox();
            txtConfirmPassword = new TextBox();
            lnkLogin = new LinkLabel();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Century", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.Gold;
            lblTitle.Location = new Point(264, 44);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(297, 28);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "ĐĂNG KÝ TÀI KHOẢN";
            lblTitle.Click += lblTitle_Click;
            // 
            // btnRegister
            // 
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnRegister.Location = new Point(340, 321);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(117, 32);
            btnRegister.TabIndex = 4;
            btnRegister.Text = "ĐĂNG KÝ NGAY";
            btnRegister.UseVisualStyleBackColor = true;
            btnRegister.Click += btnRegister_Click;
            // 
            // txtUsername
            // 
            txtUsername.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtUsername.Location = new Point(340, 142);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(117, 23);
            txtUsername.TabIndex = 0;
            txtUsername.Text = "Tên đăng nhập\r\n";
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(340, 192);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(117, 23);
            txtEmail.TabIndex = 1;
            txtEmail.Text = "Email\r\n";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(340, 238);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(117, 23);
            txtPassword.TabIndex = 2;
            txtPassword.Text = "Mật khẩu";
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.Location = new Point(340, 281);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Size = new Size(117, 23);
            txtConfirmPassword.TabIndex = 3;
            txtConfirmPassword.Text = "Xác nhận mật khẩu";
            // 
            // lnkLogin
            // 
            lnkLogin.AutoSize = true;
            lnkLogin.LinkColor = Color.Red;
            lnkLogin.Location = new Point(324, 368);
            lnkLogin.Name = "lnkLogin";
            lnkLogin.Size = new Size(155, 15);
            lnkLogin.TabIndex = 5;
            lnkLogin.TabStop = true;
            lnkLogin.Text = "Đã có tài khoản? Đăng nhập";
            lnkLogin.LinkClicked += lnkLogin_LinkClicked;
            // 
            // RegisterForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlDark;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Zoom;
            ClientSize = new Size(800, 450);
            Controls.Add(lnkLogin);
            Controls.Add(txtUsername);
            Controls.Add(txtEmail);
            Controls.Add(txtPassword);
            Controls.Add(txtConfirmPassword);
            Controls.Add(btnRegister);
            Controls.Add(lblTitle);
            DoubleBuffered = true;
            Name = "RegisterForm";
            Text = "RegisterForm";
            Load += RegisterForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblTitle;
        private Button btnRegister;
        private TextBox txtUsername;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private LinkLabel lnkLogin;
    }
}