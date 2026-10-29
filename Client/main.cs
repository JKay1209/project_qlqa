using client.network;
using System.Net.Security;
using System.Net.Sockets;

namespace project_qlqa
{
    public partial class main : Form
    {
        // Tạm thời ghi cứng; Task 14 (UDP Broadcast) sẽ tự tìm server thay cho phần này
        private const string ServerHost = "127.0.0.1";
        private const int ServerPort = 5000;

        private readonly ConnectionHandler _conn = new();
        private TcpClient? _tcp;
        private SslStream? _channel;
        private bool _busy;

        public main()
        {
            InitializeComponent();

            AcceptButton = btnLogin;          // nhấn Enter = bấm Đăng nhập
            txtUsername.MaxLength = 50;
            txtPassword.MaxLength = 128;
            lblStatus.Text = "";

            // Gán sự kiện bằng code. Nếu bạn đã double-click nút trong Designer thì BỎ các dòng này
            // (gán hai lần thì hàm chạy hai lần).
            btnLogin.Click += btnLogin_Click;
            btnRegister.Click += btnRegister_Click;
            btnForgotPassword.Click += btnForgotPassword_Click;
        }

        // ================= Đăng nhập =================
        private async void btnLogin_Click(object? sender, EventArgs e)
        {
            if (_busy) return;                                   // chặn bấm liên tiếp

            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text;
            if (user.Length == 0 || pass.Length == 0)
            {
                ShowStatus("Vui lòng nhập tài khoản và mật khẩu.");
                return;
            }

            SetBusy(true);
            ShowStatus("Đang kết nối...", ok: true);
            try
            {
                // 1) Kết nối TCP + bắt tay TLS + kiểm tra vân tay (mật khẩu CHƯA được gửi)
                var (tcp, channel, _) = await _conn.ConnectSecureAsync(
                    ServerHost, ServerPort, AskUserTrust, CancellationToken.None);
                _tcp = tcp;
                _channel = channel;

                // 2) TODO: gửi Login(user, pass) qua _channel và nhận LoginOk/LoginFail.
                //    Cần khung gói tin + phía Server, xem phần "Bước tiếp theo".
                ShowStatus("Kết nối an toàn thành công (chưa gửi đăng nhập).", ok: true);
            }
            catch (ServerIdentityChangedException ex)
            {
                MessageBox.Show(this,
                    "CẢNH BÁO BẢO MẬT: khóa của server khác với lần trước.\n" +
                    "Có thể server đã cài lại hoặc có người giả mạo/nghe lén.\n\nVân tay mới:\n" + ex.Fingerprint,
                    "Server không đáng tin", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                ShowStatus("Đã chặn: server không đáng tin.");
            }
            catch (ServerNotTrustedException) { ShowStatus("Bạn đã từ chối tin cậy server."); }
            catch (ServerUnavailableException) { ShowStatus("Không kết nối được tới server. Server đã bật chưa?"); }
            catch (TimeoutException) { ShowStatus("Hết thời gian chờ server."); }
            catch (HandshakeFailedException) { ShowStatus("Kết nối không an toàn hoặc bị ngắt giữa chừng."); }
            catch (OperationCanceledException) { ShowStatus("Đã hủy."); }
            finally
            {
                txtPassword.Clear();                             // không để mật khẩu nằm lại trong ô nhập
                SetBusy(false);
            }
        }

        // Hộp thoại xác nhận vân tay, chỉ gọi khi gặp server mới (chạy trên luồng giao diện)
        private bool AskUserTrust(string fingerprint)
        {
            return MessageBox.Show(this,
                "Đây là lần đầu kết nối tới server này.\n\nVân tay (SHA-256):\n" + fingerprint +
                "\n\nHãy so sánh với vân tay hiển thị trên máy Server. Nếu KHỚP, chọn Yes.",
                "Xác nhận server", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        // ================= Chức năng làm sau =================
        private void btnRegister_Click(object? sender, EventArgs e)
            => MessageBox.Show(this, "Chức năng Đăng ký sẽ làm ở Task 04.", "Chưa hỗ trợ");

        private void btnForgotPassword_Click(object? sender, EventArgs e)
            => MessageBox.Show(this, "Chức năng Quên mật khẩu (OTP) sẽ làm ở Task 07.", "Chưa hỗ trợ");

        // ================= Tiện ích giao diện =================
        private void SetBusy(bool busy)
        {
            _busy = busy;
            UseWaitCursor = busy;
            txtUsername.Enabled = txtPassword.Enabled = !busy;
            btnLogin.Enabled = btnRegister.Enabled = btnForgotPassword.Enabled = !busy;
        }

        private void ShowStatus(string message, bool ok = false)
        {
            lblStatus.ForeColor = ok ? Color.DarkGreen : Color.Firebrick;
            lblStatus.Text = message;
        }

        private void CloseConnection()
        {
            _channel?.Dispose();
            _tcp?.Dispose();
            _channel = null;
            _tcp = null;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            CloseConnection();
            base.OnFormClosed(e);
        }
    }
}