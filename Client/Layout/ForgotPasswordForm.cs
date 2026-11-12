using Client.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;

namespace Client.Layout
{
    public partial class ForgotPasswordForm : Form
    {
        private readonly AuthService _auth = new AuthService();
        public ForgotPasswordForm()
        {
            InitializeComponent();
        }

        private async void btnSendOtp_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();

            // Kiểm tra email
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Vui lòng nhập email.", "Thông báo", MessageBoxButtons.OK);
                return;
            }

            if (!MailAddress.TryCreate(email, out _))
            {
                MessageBox.Show("Email không đúng định dạng.", "Thông báo", MessageBoxButtons.OK);
                return;
            }

            try
            {
                btnSendOTP.Enabled = false;

                // Gọi Server gửi OTP
                await _auth.ForgotPasswordAsync(email);

                MessageBox.Show(
                    "Nếu email thuộc một tài khoản hợp lệ, mã OTP sẽ được gửi đến email đó.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Chuyển sang màn hình nhập OTP
                var form = new VerifyOtpForm(email);

                form.Show();

                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Không thể gửi yêu cầu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnSendOTP.Enabled = true;
            }
        }

        private void ForgotPasswordForm_Load(object sender, EventArgs e)
        {
        }
    }
}
