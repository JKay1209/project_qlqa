using Client.Security;
using project_qlqa;
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
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
        private void btnLogout_Click(object sender,EventArgs e)
        {
            SessionManager.End();

            TokenManager.Clear();

            main loginForm = new main();

            loginForm.Show();

            this.Close();
        }
    }
}
