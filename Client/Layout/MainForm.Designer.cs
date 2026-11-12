namespace Client.Layout
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            lblWelcome = new Label();
            btnTable = new Button();
            btnMenu = new Button();
            btnOrder = new Button();
            btnLogout = new Button();
            btnRegisterAccount = new Button();
            SuspendLayout();
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.BackColor = Color.Transparent;
            lblWelcome.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 163);
            lblWelcome.Location = new Point(369, 145);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(276, 32);
            lblWelcome.TabIndex = 0;
            lblWelcome.Text = "Xin chào, Quản trị viên";
            // 
            // btnTable
            // 
            btnTable.Location = new Point(329, 229);
            btnTable.Margin = new Padding(3, 4, 3, 4);
            btnTable.Name = "btnTable";
            btnTable.Size = new Size(121, 31);
            btnTable.TabIndex = 1;
            btnTable.Text = "Quản lý bàn";
            btnTable.UseVisualStyleBackColor = true;
            // 
            // btnMenu
            // 
            btnMenu.Location = new Point(329, 297);
            btnMenu.Margin = new Padding(3, 4, 3, 4);
            btnMenu.Name = "btnMenu";
            btnMenu.Size = new Size(86, 31);
            btnMenu.TabIndex = 2;
            btnMenu.Text = "Thực đơn";
            btnMenu.UseVisualStyleBackColor = true;
            // 
            // btnOrder
            // 
            btnOrder.Location = new Point(329, 360);
            btnOrder.Margin = new Padding(3, 4, 3, 4);
            btnOrder.Name = "btnOrder";
            btnOrder.Size = new Size(86, 31);
            btnOrder.TabIndex = 3;
            btnOrder.Text = "Đơn hàng";
            btnOrder.UseVisualStyleBackColor = true;
            // 
            // btnLogout
            // 
            btnLogout.Location = new Point(533, 360);
            btnLogout.Margin = new Padding(3, 4, 3, 4);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(86, 31);
            btnLogout.TabIndex = 4;
            btnLogout.Text = "Đăng xuất";
            btnLogout.UseVisualStyleBackColor = true;
            btnLogout.Click += btnLogout_Click;
            // 
            // btnRegisterAccount
            // 
            btnRegisterAccount.Location = new Point(508, 299);
            btnRegisterAccount.Name = "btnRegisterAccount";
            btnRegisterAccount.Size = new Size(137, 29);
            btnRegisterAccount.TabIndex = 5;
            btnRegisterAccount.Text = "Tạo tài khoản";
            btnRegisterAccount.UseVisualStyleBackColor = true;
            btnRegisterAccount.Click += btnRegisterAccount_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Zoom;
            ClientSize = new Size(974, 633);
            Controls.Add(btnRegisterAccount);
            Controls.Add(btnLogout);
            Controls.Add(btnOrder);
            Controls.Add(btnMenu);
            Controls.Add(btnTable);
            Controls.Add(lblWelcome);
            DoubleBuffered = true;
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainForm";
            Text = "MainForm";
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblWelcome;
        private Button btnTable;
        private Button btnMenu;
        private Button btnOrder;
        private Button btnLogout;
        private Button btnRegisterAccount;
    }
}