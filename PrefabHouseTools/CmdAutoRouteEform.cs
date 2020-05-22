using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrefabHouseTools
{
    public partial class CmdAutoRouteEform : Form
    {
        public CmdAutoRouteEform()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }
        public void InputLevels(IEnumerable<string> strings)
        {
            foreach (string s in strings)
            {
                listCeilingLevel.Items.Add(s);
                listFloorLevel.Items.Add(s);
                listInteriorCeilingLevels.Items.Add(s);
            }
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            if ((listCeilingLevel.SelectedIndices.Count > 1) || 
                (listFloorLevel.SelectedIndices.Count > 1))
                MessageBox.Show("Only one level may be selected.");
            else if ((listCeilingLevel.SelectedIndices.Count < 1) ||
                (listFloorLevel.SelectedIndices.Count < 1))
                MessageBox.Show("No level is selected.");
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }      
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
