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
            lblTitle.Font = new Font("Century", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.Gold;
            lblTitle.Location = new Point(302, 59);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(309, 35);
            lblTitle.TabIndex = 8;
            lblTitle.Text = "TẠO TÀI KHOẢN";
            lblTitle.Click += lblTitle_Click;
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(350, 150);
            txtUsername.Margin = new Padding(3, 4, 3, 4);
            txtUsername.MaxLength = 50;
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "Tên đăng nhập";
            txtUsername.Size = new Size(217, 27);
            txtUsername.TabIndex = 0;
            // 
            // txtDisplayName
            // 
            txtDisplayName.Location = new Point(350, 198);
            txtDisplayName.Margin = new Padding(3, 4, 3, 4);
            txtDisplayName.MaxLength = 100;
            txtDisplayName.Name = "txtDisplayName";
            txtDisplayName.PlaceholderText = "Tên hiển thị";
            txtDisplayName.Size = new Size(217, 27);
            txtDisplayName.TabIndex = 1;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(350, 246);
            txtEmail.Margin = new Padding(3, 4, 3, 4);
            txtEmail.MaxLength = 100;
            txtEmail.Name = "txtEmail";
            txtEmail.PlaceholderText = "Email";
            txtEmail.Size = new Size(217, 27);
            txtEmail.TabIndex = 2;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(350, 294);
            txtPassword.Margin = new Padding(3, 4, 3, 4);
            txtPassword.MaxLength = 128;
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Mật khẩu";
            txtPassword.Size = new Size(217, 27);
            txtPassword.TabIndex = 3;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.Location = new Point(350, 342);
            txtConfirmPassword.Margin = new Padding(3, 4, 3, 4);
            txtConfirmPassword.MaxLength = 128;
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.PlaceholderText = "Xác nhận mật khẩu";
            txtConfirmPassword.Size = new Size(217, 27);
            txtConfirmPassword.TabIndex = 4;
            txtConfirmPassword.UseSystemPasswordChar = true;
            // 
            // cmbRole
            // 
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRole.FormattingEnabled = true;
            cmbRole.Location = new Point(350, 390);
            cmbRole.Margin = new Padding(3, 4, 3, 4);
            cmbRole.Name = "cmbRole";
            cmbRole.Size = new Size(217, 28);
            cmbRole.TabIndex = 5;
            // 
            // btnRegister
            // 
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnRegister.Location = new Point(391, 440);
            btnRegister.Margin = new Padding(3, 4, 3, 4);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(134, 43);
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
            lnkLogin.Location = new Point(370, 500);
            lnkLogin.Name = "lnkLogin";
            lnkLogin.Size = new Size(197, 20);
            lnkLogin.TabIndex = 7;
            lnkLogin.TabStop = true;
            lnkLogin.Text = "Đã có tài khoản? Đăng nhập";
            lnkLogin.LinkClicked += lnkLogin_LinkClicked;
            // 
            // RegisterForm
            // 
            AcceptButton = btnRegister;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlDark;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Zoom;
            ClientSize = new Size(914, 600);
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
            Margin = new Padding(3, 4, 3, 4);
            Name = "RegisterForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Tạo tài khoản";
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