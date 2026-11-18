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
        private readonly string _resetToken;

        public ResetPasswordForm(string resetToken)
        {
            InitializeComponent();

            _resetToken = resetToken;
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
