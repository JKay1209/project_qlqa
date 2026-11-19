using Client.Services;
using project_qlqa;
using System;
using System.Windows.Forms;

namespace Client.Layout
{
    public partial class ResetPasswordForm : Form
    {
        private readonly string _resetToken;
        private readonly AuthService _auth = new AuthService();

        public ResetPasswordForm(string resetToken)
        {
            InitializeComponent();
            _resetToken = resetToken;
        }

        private async void btnResetPassword_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu mới.");
                return;
            }

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.");
                return;
            }

            try
            {
                await _auth.ResetPasswordAsync(
                    _resetToken,
                    txtNewPassword.Text);

                MessageBox.Show(
                    "Đổi mật khẩu thành công.");

                main loginForm = new main();

                loginForm.Show();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ResetPasswordform_Load(
            object sender,
            EventArgs e)
        {
        }
    }
}