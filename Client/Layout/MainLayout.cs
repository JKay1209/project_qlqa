using System.Windows.Forms;

namespace Client.Layout
{
    public class MainLayout : Form
    {
        public MainLayout()
        {
            Text = "Main Layout";
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainLayout));
            SuspendLayout();
            // 
            // MainLayout
            // 
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Zoom;
            ClientSize = new Size(892, 510);
            DoubleBuffered = true;
            Name = "MainLayout";
            ResumeLayout(false);


        }

        private void MainLayout_Load(object sender, EventArgs e)
        {

        }
    }
}