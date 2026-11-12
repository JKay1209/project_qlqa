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
        public ForgotPasswordForm()
        {
            InitializeComponent();
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
                await _auth.ForgotPasswordAsync(
                    email);

                MessageBox.Show(
                    "OTP đã được gửi");

                VerifyOtpForm frm =
                    new VerifyOtpForm();

                frm.Show();

                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message);
            }
        }

        private void ForgotPasswordForm_Load(object sender, EventArgs e)
        {

        }
    }
}
