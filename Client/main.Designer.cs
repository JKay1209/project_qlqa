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
            pnlLogin = new Panel();
            btnForgotPassword = new Button();
            btnRegister = new Button();
            btnLogin = new Button();
            txtPassword = new TextBox();
            txtUsername = new TextBox();
            lblPassword = new Label();
            lblUsername = new Label();
            lblTitle = new Label();
            lblStatus = new Label();
            pnlLogin.SuspendLayout();
            SuspendLayout();
            // 
            // pnlLogin
            // 
            pnlLogin.Anchor = AnchorStyles.None;
            pnlLogin.Controls.Add(lblStatus);
            pnlLogin.Controls.Add(btnForgotPassword);
            pnlLogin.Controls.Add(btnRegister);
            pnlLogin.Controls.Add(btnLogin);
            pnlLogin.Controls.Add(txtPassword);
            pnlLogin.Controls.Add(txtUsername);
            pnlLogin.Controls.Add(lblPassword);
            pnlLogin.Controls.Add(lblUsername);
            pnlLogin.Controls.Add(lblTitle);
            pnlLogin.Location = new Point(250, 130);
            pnlLogin.Name = "pnlLogin";
            pnlLogin.Size = new Size(700, 576);
            pnlLogin.TabIndex = 8;
            // 
            // btnForgotPassword
            // 
            btnForgotPassword.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnForgotPassword.Location = new Point(230, 400);
            btnForgotPassword.Name = "btnForgotPassword";
            btnForgotPassword.Size = new Size(240, 48);
            btnForgotPassword.TabIndex = 15;
            btnForgotPassword.Text = "Quên mật khẩu";
            btnForgotPassword.UseVisualStyleBackColor = true;
            // 
            // btnRegister
            // 
            btnRegister.Font = new Font("Segoe UI", 13F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnRegister.Location = new Point(365, 320);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(255, 55);
            btnRegister.TabIndex = 14;
            btnRegister.Text = "Đăng ký";
            btnRegister.UseVisualStyleBackColor = true;
            // 
            // btnLogin
            // 
            btnLogin.Font = new Font("Segoe UI", 13F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnLogin.Location = new Point(80, 320);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(255, 55);
            btnLogin.TabIndex = 13;
            btnLogin.Text = "Đăng nhập";
            btnLogin.UseVisualStyleBackColor = true;
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtPassword.Location = new Point(230, 230);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(390, 39);
            txtPassword.TabIndex = 12;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // txtUsername
            // 
            txtUsername.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtUsername.Location = new Point(230, 150);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(390, 39);
            txtUsername.TabIndex = 11;
            // 
            // lblPassword
            // 
            lblPassword.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblPassword.Location = new Point(80, 230);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(140, 40);
            lblPassword.TabIndex = 10;
            lblPassword.Text = "Password";
            lblPassword.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUsername
            // 
            lblUsername.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblUsername.Location = new Point(80, 150);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(140, 40);
            lblUsername.TabIndex = 9;
            lblUsername.Text = "Username";
            lblUsername.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 28F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(0, 30);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(700, 80);
            lblTitle.TabIndex = 8;
            lblTitle.Text = "Quản Lý Quán Ăn";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI Semibold", 19.8000011F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(80, 452);
            lblStatus.Size = new Size(540, 44);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 46);
            lblStatus.TabIndex = 9;
            // 
            // main
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 760);
            Controls.Add(pnlLogin);
            MinimumSize = new Size(760, 600);
            Name = "main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Quản Lý Quán Ăn";
            pnlLogin.ResumeLayout(false);
            pnlLogin.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlLogin;
        private Button btnForgotPassword;
        private Button btnRegister;
        private Button btnLogin;
        private TextBox txtPassword;
        private TextBox txtUsername;
        private Label lblPassword;
        private Label lblUsername;
        private Label lblTitle;
        private Label lblStatus;
    }
}
