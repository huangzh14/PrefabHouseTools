using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PrefabHouseTools
{
    public partial class CmdReadJsonForm : Form
    {
        Pen myPen = null;
        Graphics g = null;
        private int canvasW, canvasH;
        private CmdReadJson originCommand;
        public HouseObject CurrentHouse { get; private set; }
        int totalWorkLoad;
        int currentWorkLoad;

        public CmdReadJsonForm(CmdReadJson command)
        {
            InitializeComponent();
            StartModel.Enabled = false;

            
            ///Data prepare.
            originCommand = command;
            CurrentHouse = null;
        }


        private void CheckCondition()
        {
            if ((this.CurrentHouse != null) &&
                (this.LevelBox.SelectedItem != null))
                StartModel.Enabled = true;
        }

        public void SetInitialProgress(CmdReadJson currentCmd)
        {
            totalWorkLoad = currentCmd.TotalWorkLoad;
            currentWorkLoad = 0;
        }
        public void UpdateProgress(int progress)
        {
            currentWorkLoad += progress;
            int current = 100 * currentWorkLoad / totalWorkLoad;
            prograssLabel.Text = current.ToString();
            prograssLabel.Refresh();
            this.progressBar1.Value = current;
        }

        private void ReadCurrentSelection(Stream fileStream)
        {
            CurrentHouse = new HouseObject();
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
                    try
                    {
                        Stream fileStream = openJsonFile.OpenFile();
                        this.ReadCurrentSelection(fileStream);
                        fileStream.Close();
                    }
                    catch
                    {
                        MessageBox.Show("json文件格式错误，请检查输入文件。");
                        return;
                    }
                    
                    ///Write the data to the command as well.
                    originCommand.CurrentHouse = this.CurrentHouse;
                    this.CheckCondition();
                    PreviewCanvas.Refresh();
                }
            }
        }

        private void StartModel_Click(object sender, EventArgs e)
        {
            SetInitialProgress(originCommand);
            originCommand.DoCreateWalls();
            originCommand.DoCreateOpenings();

            DialogResult = DialogResult.OK;
        }

        private void LevelBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            originCommand.SetBaseLevel(LevelBox.SelectedItem as string);
            this.CheckCondition();
        }

        private void CmdReadJsonForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void CmdReadJsonForm_SizeChanged(object sender, EventArgs e)
        {
            PreviewCanvas.Refresh();
        }

        private void PreviewCanvas_Paint(object sender, PaintEventArgs e)
        {
            ///Graphic prepare
            myPen = new Pen(Color.Black);
            g = PreviewCanvas.CreateGraphics();
            g.Clear(Color.White);
            canvasW = PreviewCanvas.Width;
            canvasH = PreviewCanvas.Height;
            ///
            if (CurrentHouse == null) return;
            List<A_Wall> walls = CurrentHouse.Floors
                .SelectMany(f => f.Walls).ToList();
            List<A_Point> pts = walls.Select(w => w.P1).ToList();
            pts.AddRange(walls.Select(w => w.P2));

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
            scale = (float)(scale / 0.9);

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
