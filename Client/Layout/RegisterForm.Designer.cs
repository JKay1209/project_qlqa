namespace Client.Layout
{
    partial class RegisterForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegisterForm));
            lblTitle = new Label();
            txtUsername = new TextBox();
            txtDisplayName = new TextBox();
            txtEmail = new TextBox();
            txtPassword = new TextBox();
            txtConfirmPassword = new TextBox();
            cmbRole = new ComboBox();
            btnRegister = new Button();
            lnkLogin = new LinkLabel();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Segoe UI Black", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.Crimson;
            lblTitle.Location = new Point(293, 33);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(211, 32);
            lblTitle.TabIndex = 8;
            lblTitle.Text = "TẠO TÀI KHOẢN";
            lblTitle.Click += lblTitle_Click;
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(306, 76);
            txtUsername.MaxLength = 50;
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "Tên đăng nhập";
            txtUsername.Size = new Size(190, 23);
            txtUsername.TabIndex = 0;
            // 
            // txtDisplayName
            // 
            txtDisplayName.Location = new Point(306, 115);
            txtDisplayName.MaxLength = 100;
            txtDisplayName.Name = "txtDisplayName";
            txtDisplayName.PlaceholderText = "Tên hiển thị";
            txtDisplayName.Size = new Size(190, 23);
            txtDisplayName.TabIndex = 1;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(306, 154);
            txtEmail.MaxLength = 100;
            txtEmail.Name = "txtEmail";
            txtEmail.PlaceholderText = "Email";
            txtEmail.Size = new Size(190, 23);
            txtEmail.TabIndex = 2;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(306, 193);
            txtPassword.MaxLength = 128;
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Mật khẩu";
            txtPassword.Size = new Size(190, 23);
            txtPassword.TabIndex = 3;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.Location = new Point(306, 233);
            txtConfirmPassword.MaxLength = 128;
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.PlaceholderText = "Xác nhận mật khẩu";
            txtConfirmPassword.Size = new Size(190, 23);
            txtConfirmPassword.TabIndex = 4;
            txtConfirmPassword.UseSystemPasswordChar = true;
            // 
            // cmbRole
            // 
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRole.FormattingEnabled = true;
            cmbRole.Location = new Point(306, 273);
            cmbRole.Name = "cmbRole";
            cmbRole.Size = new Size(190, 23);
            cmbRole.TabIndex = 5;
            // 
            // btnRegister
            // 
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnRegister.Location = new Point(339, 302);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(117, 32);
            btnRegister.TabIndex = 6;
            btnRegister.Text = "TẠO";
            btnRegister.UseVisualStyleBackColor = true;
            btnRegister.Click += btnRegister_Click;
            // 
            // lnkLogin
            // 
            lnkLogin.AutoSize = true;
            lnkLogin.BackColor = Color.Transparent;
            lnkLogin.LinkColor = Color.Red;
            lnkLogin.Location = new Point(323, 343);
            lnkLogin.Name = "lnkLogin";
            lnkLogin.Size = new Size(155, 15);
            lnkLogin.TabIndex = 7;
            lnkLogin.TabStop = true;
            lnkLogin.Text = "Đã có tài khoản? Đăng nhập";
            lnkLogin.LinkClicked += lnkLogin_LinkClicked;
            // 
            // RegisterForm
            // 
            AcceptButton = btnRegister;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlDark;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Zoom;
            ClientSize = new Size(804, 461);
            Controls.Add(lblTitle);
            Controls.Add(txtUsername);
            Controls.Add(txtDisplayName);
            Controls.Add(txtEmail);
            Controls.Add(txtPassword);
            Controls.Add(txtConfirmPassword);
            Controls.Add(cmbRole);
            Controls.Add(btnRegister);
            Controls.Add(lnkLogin);
            DoubleBuffered = true;
            Name = "RegisterForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Tạo tài khoản";
            Load += RegisterForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private TextBox txtUsername;
        private TextBox txtDisplayName;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private ComboBox cmbRole;
        private Button btnRegister;
        private LinkLabel lnkLogin;
    }
}