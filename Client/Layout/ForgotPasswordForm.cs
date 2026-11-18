using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Client.Services;

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
        private void ForgotPasswordForm_Load(object sender,EventArgs e)
        {
            lblOTP.Visible = false;
            txtOTP.Visible = false;
            btnVerifyOTP.Visible = false;
        }

        private async void btnSendOtp_Click(
    object sender,
    EventArgs e)
        {
            string email =
                txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(
                    "Vui lòng nhập email");

                return;
            }

            try
            {
                await _auth.ForgotPasswordAsync(email);

                MessageBox.Show(
                    "OTP đã được gửi");

                lblOTP.Visible = true;
                txtOTP.Visible = true;
                btnVerifyOTP.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message);
            }
        }

        private async void btnVerifyOTP_Click(
    object sender,
    EventArgs e)
{
    try
    {
        string email =
            txtEmail.Text.Trim();

        string otp =
            txtOTP.Text.Trim();

        var result = await _auth.VerifyOtpAsync(email,otp);

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
        MessageBox.Show(
            ex.Message);
    }
}

    }
}
