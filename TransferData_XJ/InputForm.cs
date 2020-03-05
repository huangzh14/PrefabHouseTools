using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TransferData_XJ
{
    public partial class InputForm : Form
    {
        public InputForm()
        {
            InitializeComponent();
            StartModel.Enabled = false;
            this.DialogResult = DialogResult.No;
        }

        public HouseObjects CurrentHouse { get; set; }
        private void UpdateCurrentSelection(Stream fileStream)
        {
            CurrentHouse = new HouseObjects();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                JObject jsonObj = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                IList<JToken> floors = jsonObj["floors"].Children().ToList();
                foreach (JToken floor in floors)
                {
                    CurrentHouse.Floors.Add(floor.ToObject<A_Floor>());
                }
            }
        }
        private void ChooseFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openJsonFile = new OpenFileDialog())
            {
                openJsonFile.InitialDirectory = "C:\\";
                openJsonFile.Filter = "json files (*.json)|*.json";
                openJsonFile.RestoreDirectory = true;

                if (openJsonFile.ShowDialog() == DialogResult.OK)
                {
                    Stream fileStream = openJsonFile.OpenFile();
                    this.UpdateCurrentSelection(fileStream);
                    fileStream.Close();
                    StartModel.Enabled = true;
                }
            }
        }

        private void StartModel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void WallTypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
