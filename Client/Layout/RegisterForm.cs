using Client.LayoutLogics;
using Client.Security;
using Client.Services;
using project_qlqa;
using Shared.DTOs.Auth;
using Shared.Enums;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Client.Layout
{
    public partial class RegisterForm : Form
    {

        private readonly AuthService _auth = new AuthService();

        public sealed class RoleItem
        {
            public string Text { get; }
            public UserRole Value { get; }

            public RoleItem(string text, UserRole value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString() => Text;
        }

        private readonly AuthLogic _logic = new();

        public RegisterForm()
        {
            InitializeComponent();

            var roles = new List<RoleItem>();

            // Người chưa đăng nhập chỉ được đăng ký vai trò Waiter.
            bool isAdmin = SessionManager.IsLoggedIn
                           && SessionManager.Role == UserRole.Admin;

            if (isAdmin)
            {
                roles.Add(new RoleItem("Admin (Quản trị viên)", UserRole.Admin));
                roles.Add(new RoleItem("Quản lý", UserRole.Manager));
                roles.Add(new RoleItem("Thu ngân", UserRole.Cashier));
                roles.Add(new RoleItem("Bếp", UserRole.Kitchen));
                roles.Add(new RoleItem("Phục vụ", UserRole.Waiter));
            }
            else
            {
                roles.Add(new RoleItem("Phục vụ", UserRole.Waiter));
            }

            cmbRole.DisplayMember = nameof(RoleItem.Text);
            cmbRole.ValueMember = nameof(RoleItem.Value);
            cmbRole.DataSource = roles;

            cmbRole.SelectedIndex = 0;

            // Không cần hiển thị lựa chọn vai trò cho người đăng ký công khai.
            cmbRole.Visible = isAdmin;
        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }
        private async void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string display = txtDisplayName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string pass = txtPassword.Text;

            if (username == "" || display == "" || pass == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            if (pass != txtConfirmPassword.Text)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.");
                return;
            }

            if (cmbRole.SelectedItem is not RoleItem selected)
            {
                MessageBox.Show("Vui lòng chọn vai trò.");
                return;
            }

            btnRegister.Enabled = false;
            try
            {
                var (ok, error) = await _logic.RegisterAsync(
                    username, pass, display, selected.Value,
                    email == "" ? null : email);

                if (ok)
                {
                    MessageBox.Show("Tạo tài khoản thành công.");
                    Close();
                }
                else
                {
                    MessageBox.Show(error);
                }
            }
            finally
            {
                btnRegister.Enabled = true;
            }
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

    }
}
