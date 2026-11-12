using project_qlqa;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Client.Services;
using Shared.DTOs.Auth;
using Shared.Enums;

namespace Client.Layout
{
    public partial class RegisterForm : Form
    {
        private readonly AuthService _auth = new AuthService();
        public RegisterForm()
        {
            InitializeComponent();
        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }
        private void lnkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            main loginForm = new main();

            loginForm.Show();

            this.Close();
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }
        private async void btnRegister_Click(object sender,EventArgs e)
        {
            string username =
                txtUsername.Text.Trim();

            string email =
                txtEmail.Text.Trim();

            string password =
                txtPassword.Text;

            string confirmPassword =
                txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(
                    "Vui lòng nhập tên đăng nhập");
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(
                    "Vui lòng nhập email");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show(
                    "Mật khẩu xác nhận không khớp");
                return;
            }

            try
            {
                await _auth.RegisterAsync(
                    new RegisterRequest(
                        username,
                        password,
                        username,
                        UserRole.Waiter));

                MessageBox.Show(
                    "Đăng ký thành công");

                main login =
                    new main();

                login.Show();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
