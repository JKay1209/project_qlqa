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

        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Đổi mật khẩu thành công");
        }

        private void ResetPasswordform_Load(object sender, EventArgs e)
        {

        }
    }
}
