using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Client.Layout;

namespace Client.Layout
{
    public partial class VerifyOtpForm : Form
    {
        public VerifyOtpForm()
        {
            InitializeComponent();
        }

        private void btnVerifyOTP_Click(object sender, EventArgs e)
        {
            ResetPasswordForm frm = new ResetPasswordForm();

            frm.Show();

            this.Hide();
        }
    }
}
