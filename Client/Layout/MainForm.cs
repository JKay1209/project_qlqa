using Client.Security;
using project_qlqa;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

namespace Client.Layout
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            lblWelcome.Text = "Xin chào, " + SessionManager.DisplayName;
            btnRegisterAccount.Visible = SessionManager.Role == UserRole.Admin;   // ẩn với người khác
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void btnRegisterAccount_Click(object sender, EventArgs e)
        {
            using var form = new RegisterForm();
            form.ShowDialog(this);  // dialog: Admin tạo xong quay lại MainForm
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            SessionManager.End();

            TokenManager.Clear();

            main loginForm = new main();

            loginForm.Show();

            this.Close();
        }
    }
}
