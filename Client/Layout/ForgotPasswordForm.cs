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

        private string _resetToken = "";
        public ForgotPasswordForm()
        {
            InitializeComponent();
        }
        private void ForgotPasswordForm_Load(object sender, EventArgs e)
        {
            lblOTP.Visible = false;
            txtOTP.Visible = false;
            btnVerifyOTP.Visible = false;
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
                await _auth.ForgotPasswordAsync(email);

                MessageBox.Show(
                    "Nếu email thuộc một tài khoản hợp lệ, mã OTP sẽ được gửi đến email đó.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                lblOTP.Visible = true;
                txtOTP.Visible = true;
                btnVerifyOTP.Visible = true;
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

        private async void btnVerifyOTP_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("B1");
                string email = txtEmail.Text.Trim();
                string otp = txtOTP.Text.Trim();
                MessageBox.Show("B2");
                var result = await _auth.VerifyOtpAsync(email, otp);
                MessageBox.Show("B3");
                _resetToken =
                    result.ResetToken;

                MessageBox.Show(
                    "OTP hợp lệ");

                ResetPasswordForm frm =
                    new ResetPasswordForm(
                        _resetToken);

                frm.Show();

                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
