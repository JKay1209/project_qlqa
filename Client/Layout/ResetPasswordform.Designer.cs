namespace Client.Layout
{
    partial class ResetPasswordForm
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
            label1 = new Label();
            label2 = new Label();
            txtNewPassword = new TextBox();
            txtConfirmPassword = new TextBox();
            label3 = new Label();
            btnResetPassword = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 163);
            label1.Location = new Point(145, 75);
            label1.Name = "label1";
            label1.Size = new Size(190, 25);
            label1.TabIndex = 0;
            label1.Text = "ĐẶT LẠI MẬT KHẨU";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 163);
            label2.Location = new Point(145, 121);
            label2.Name = "label2";
            label2.Size = new Size(100, 20);
            label2.TabIndex = 1;
            label2.Text = "Mật khẩu mới";
            // 
            // txtNewPassword
            // 
            txtNewPassword.Location = new Point(145, 155);
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.Size = new Size(190, 23);
            txtNewPassword.TabIndex = 2;
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.Location = new Point(145, 248);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Size = new Size(190, 23);
            txtConfirmPassword.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 163);
            label3.Location = new Point(149, 212);
            label3.Name = "label3";
            label3.Size = new Size(134, 20);
            label3.TabIndex = 4;
            label3.Text = "Xác nhận mật khẩu";
            // 
            // btnResetPassword
            // 
            btnResetPassword.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnResetPassword.Location = new Point(145, 304);
            btnResetPassword.Name = "btnResetPassword";
            btnResetPassword.Size = new Size(148, 30);
            btnResetPassword.TabIndex = 5;
            btnResetPassword.Text = "ĐỔI MẬT KHẨU";
            btnResetPassword.UseVisualStyleBackColor = true;
            btnResetPassword.Click += btnResetPassword_Click;
            // 
            // ResetPasswordform
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(505, 450);
            Controls.Add(btnResetPassword);
            Controls.Add(label3);
            Controls.Add(txtConfirmPassword);
            Controls.Add(txtNewPassword);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "ResetPasswordform";
            Text = "ResetPasswordform";
            Load += ResetPasswordform_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private Label label3;
        private Button btnResetPassword;
    }
}