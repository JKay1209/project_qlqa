using Client.Layout;
using Client.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Client.Layout
{
    public partial class VerifyOtpForm : Form
    {
        private readonly AuthService _auth = new AuthService();

        private readonly string _email;

        public VerifyOtpForm(string email)
        {
            InitializeComponent();

            _email = email;
        }

        private async void btnVerifyOTP_Click(
            object sender,
            EventArgs e)
        {
            string otp = txtOtp.Text.Trim();

            if (string.IsNullOrWhiteSpace(otp))
            {
                MessageBox.Show("Vui lòng nhập mã OTP.");
                return;
            }

            if (otp.Length != 6 || !otp.All(char.IsAsciiDigit))
            {
                MessageBox.Show("Mã OTP phải gồm 6 chữ số.");
                return;
            }

            try
            {
                btnVerifyOTP.Enabled = false;

                // Gửi email và OTP lên Server
                var result = await _auth.VerifyOtpAsync(
                    _email,
                    otp);

                // Server trả Reset Token nếu OTP đúng
                string resetToken = result.ResetToken;

                var form = new ResetPasswordForm(resetToken);

                form.Show();

                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Xác thực OTP thất bại",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnVerifyOTP.Enabled = true;
            }
        }
    }
}
