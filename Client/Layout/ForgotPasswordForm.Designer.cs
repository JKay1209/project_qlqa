namespace Client.Layout
{
    partial class ForgotPasswordForm
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
            txtEmail = new TextBox();
            label2 = new Label();
            btnSendOTP = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 163);
            label1.Location = new Point(132, 81);
            label1.Name = "label1";
            label1.Size = new Size(171, 25);
            label1.TabIndex = 0;
            label1.Text = "QUÊN MẬT KHẨU";
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(132, 180);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(225, 23);
            txtEmail.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 163);
            label2.Location = new Point(132, 144);
            label2.Name = "label2";
            label2.Size = new Size(39, 17);
            label2.TabIndex = 2;
            label2.Text = "Email";
            // 
            // btnSendOTP
            // 
            btnSendOTP.Location = new Point(132, 246);
            btnSendOTP.Name = "btnSendOTP";
            btnSendOTP.Size = new Size(75, 23);
            btnSendOTP.TabIndex = 3;
            btnSendOTP.Text = "GỬI OTP";
            btnSendOTP.UseVisualStyleBackColor = true;
            btnSendOTP.Click += btnSendOtp_Click;
            // 
            // ForgotPasswordForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(506, 450);
            Controls.Add(btnSendOTP);
            Controls.Add(label2);
            Controls.Add(txtEmail);
            Controls.Add(label1);
            Name = "ForgotPasswordForm";
            Text = "ForgotPasswordForm";
            Load += ForgotPasswordForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtEmail;
        private Label label2;
        private Button btnSendOTP;
    }
}