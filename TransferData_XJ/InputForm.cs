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
        Pen myPen = null;
        Graphics g = null;
        private int canvasW, canvasH;
        
        public InputForm()
        {
            InitializeComponent();
            StartModel.Enabled = false;
            this.DialogResult = DialogResult.No;

            myPen = new Pen(Color.Black);
            g = PreviewCanvas.CreateGraphics();
            canvasW = PreviewCanvas.Width;
            canvasH = PreviewCanvas.Height;
            CurrentHouse = null;
        }

        public HouseObjects CurrentHouse { get; set; }

        private void CheckCondition()
        {
            if ((this.CurrentHouse != null) &&
                (this.LevelBox.SelectedItem != null) &&
                (this.WallTypeBox.SelectedItem != null))
                StartModel.Enabled = true;
        }

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
                    this.CheckCondition();
                }
            }
            PreviewCanvas.Refresh();
        }

        private void StartModel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void WallTypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.CheckCondition();
        }

        private void LevelBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.CheckCondition();
        }

        private void PreviewCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (CurrentHouse == null) return;
            List<A_Wall> walls = CurrentHouse.Floors
                .SelectMany(f => f.Walls).ToList();
            List<A_Point> pts = walls.Select(w => w.P1).ToList();
            pts.AddRange( walls.Select(w => w.P2));

            ///Get the range of the house.
            float minX = pts.OrderBy(p => p.X).First().X;
            float minY = pts.OrderBy(p => p.Y).First().Y;
            float maxX = pts.OrderByDescending(p => p.X)
                             .First().X;
            float maxY = pts.OrderByDescending(p => p.Y)
                             .First().Y;

            ///Calculate the transform and scale factor.
            Point toBaseT = Point.Round(new PointF(minX, minY));
            float scaleX = (maxX - minX) / canvasW;
            float scaleY = (maxY - minY) / canvasH;
            float scale = Math.Max(scaleX, scaleY);
            scale = (float) (scale / 0.9);

            foreach (A_Wall wall in walls)
            {
                float x1 = ((wall.P1.X - minX) / scale) + (canvasW / 20);
                float y1 = ((wall.P1.Y - minY) / scale) + (canvasH / 20);
                float x2 = ((wall.P2.X - minX) / scale) + (canvasW / 20);
                float y2 = ((wall.P2.Y - minY) / scale) + (canvasH / 20);
                PointF p1 = new PointF(x1, y1);
                PointF p2 = new PointF(x2, y2);
                g.DrawLine(myPen, p1, p2);
            }
        }
    }
}
