using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InteriorAutoPanel
{
    public partial class InputForm : Form
    {
        public InputForm()
        {
            InitializeComponent();
        }

        private void Input_Load(object sender, EventArgs e)
        {

        }

        private void Start_Click(object sender, EventArgs e)
        {
            if ((UnitWidth.Value < 100) || (DistanceToWall.Value < 10))
            {
                MessageBox.Show("Distance to wall must be larger than 10mm.\n " +
                    "Standard Unit Width must be larger than 100mm.\n" +
                    "Please check again.");
            }
            else if ((UnitWidth.Value > 3000) || (DistanceToWall.Value > 1000))
            {
                MessageBox.Show("Distance to wall or " +
                    "standard Unit Width is too big.\n" +
                    "Please check again.");
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
