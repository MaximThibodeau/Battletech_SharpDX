using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class Shooter : Form
    {
        public bool ResizeForm1 = true;
        public FirstPersonShooter Game;        

        public Shooter()
        {
            InitializeComponent();
            Game = new FirstPersonShooter(this);
        }

        private void utilitiesToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            utilities U = new utilities();
            U.ShowDialog();
        }
    }
}