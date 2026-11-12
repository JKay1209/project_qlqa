using Client.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Client.Layout
{
    public partial class ResetPasswordForm : Form
    {
        public ResetPasswordForm()
        {
            InitializeComponent();
        }

        private readonly AuthService _auth = new AuthService();

        private readonly string _resetToken;

        public ResetPasswordForm(string resetToken)
        {
            InitializeComponent();

            _resetToken = resetToken;

            // Che mật khẩu khi nhập
            txtNewPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.UseSystemPasswordChar = true;
        }

        private async void btnResetPassword_Click(object sender, EventArgs e)
        {
            string newPassword = txtNewPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(newPassword)
                || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ mật khẩu.");
                return;
            }

            if (newPassword.Length < 8 || newPassword.Length > 128)
            {
                MessageBox.Show("Mật khẩu phải từ 8 đến 128 ký tự.");
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.");
                return;
            }

            try
            {
                btnResetPassword.Enabled = false;

                await _auth.ResetPasswordAsync(
                    _resetToken,
                    newPassword);

                MessageBox.Show(
                    "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Không thể đặt lại mật khẩu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnResetPassword.Enabled = true;
            }
        }

        private void ResetPasswordform_Load(object sender, EventArgs e)
        {
        }
    }
}
