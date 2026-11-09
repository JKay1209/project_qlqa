using Client.Layout;
using Client.LayoutLogics;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace project_qlqa
{
    public partial class main : Form
    {
        private readonly AuthLogic _authLogic = new();
        public main()
        {
            InitializeComponent();

            btnLogin.Click += btnLogin_Click;
            Resize += FormDangNhap_Resize;
        }

        private void main_Load(object sender, EventArgs e)
        {
           
            BackgroundImageLayout = ImageLayout.Stretch;
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;

          
            panelLogin.BackColor = Color.FromArgb(140, 255, 255, 255);

           
            btnLogin.BackColor = Color.FromArgb(41, 52, 164);
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;

           
            txtPass.UseSystemPasswordChar = true;

          
            txtUser.Font = new Font("Segoe UI", 11);
            txtPass.Font = new Font("Segoe UI", 11);

           
            RoundPanel(panelLogin, 30);
            RoundButton(btnLogin, 20);

           
            CenterPanel();
        }

        private void CenterPanel()
        {
            panelLogin.Left =
                (this.ClientSize.Width - panelLogin.Width) / 2;

            panelLogin.Top =
                (this.ClientSize.Height - panelLogin.Height) / 2;
        }

        private void RoundPanel(Panel panel, int radius)
        {
            panel.Region = CreateRoundedRegion(panel.Width, panel.Height, radius);
        }

        private void RoundButton(Button btn, int radius)
        {
            btn.Region = CreateRoundedRegion(btn.Width, btn.Height, radius);
        }

        private static Region CreateRoundedRegion(int width, int height, int radius)
        {
            radius = Math.Min(radius, Math.Min(width, height));

            GraphicsPath path = new GraphicsPath();

            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(width - radius, 0, radius, radius, 270, 90);
            path.AddArc(width - radius,
                        height - radius,
                        radius, radius, 0, 90);
            path.AddArc(0,
                        height - radius,
                        radius, radius, 90, 90);

            path.CloseFigure();

            return new Region(path);
        }

        private void FormDangNhap_Resize(object sender, EventArgs e)
        {
            CenterPanel();
        }

        private async void btnLogin_Click(object sender,EventArgs e)
        {
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text;

            if (string.IsNullOrWhiteSpace(user))
            {
                MessageBox.Show(
                    "Vui lòng nhập tài khoản");

                return;
            }

            if (string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show(
                    "Vui lòng nhập mật khẩu");

                return;
            }

            btnLogin.Enabled = false;

            try
            {
                var result =
                    await _authLogic.LoginAsync(
                        user,
                        pass);

                if (result.Ok)
                {
                    MessageBox.Show(
                        "Đăng nhập thành công");

                    MainForm frm = new MainForm();

                    frm.Show();

                    this.Hide();
                }
                else
                {
                    MessageBox.Show(
                        result.Error);
                }
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void lblSubTitle_Click(object sender, EventArgs e)
        {
        }

        private void lblTitle_Click(object sender, EventArgs e)
        {
        }
    }
}