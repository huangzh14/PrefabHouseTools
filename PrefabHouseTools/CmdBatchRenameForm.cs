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
using System.Reflection;

namespace PrefabHouseTools
{
    public partial class CmdBatchRenameForm : Form
    {
        string originalFolder;
        Dictionary<string, string> oldNamesExts;
        Dictionary<string, string> matchName;

        Dictionary<string, string> renameGrid;

        public CmdBatchRenameForm()
        {
            InitializeComponent();
            startRenameButton.Enabled = false;
            openFileDialog1.Filter = "csv files(*.csv)|*.csv";
            renameGrid = new Dictionary<string, string>();
        }

        private void openFolderButton_Click(object sender, EventArgs e)
        {
            oldNamesExts = new Dictionary<string, string>();
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                originalFolder = folderBrowserDialog1.SelectedPath;
                FileInfo[] files = 
                    new DirectoryInfo(originalFolder)
                    .GetFiles("*.*").ToArray();

                oldNamesExts.Clear();
                foreach(FileInfo fi in files)
                {
                    oldNamesExts.Add(fi.Name, fi.Extension);
                }
            }
            UpdateNewNames();
        }

        private void openMatchCsvButton_Click(object sender, EventArgs e)
        {
            matchName = new Dictionary<string, string>();
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader reader = 
                    new StreamReader(openFileDialog1.OpenFile()))
                {
                    while (!reader.EndOfStream)
                    {
                        string current = reader.ReadLine();
                        var values = current.Split(',');

                        matchName.Add(values[0], values[1]);
                        
                    }
                }
                UpdateNewNames();
            }
        }

        private void UpdateNewNames()
        {
            if (oldNamesExts == null||matchName == null||originalFolder == null) return;
           
            nameTextBox.Clear();
            renameGrid.Clear();
            string newName;
            foreach (KeyValuePair<string,string>
                originalName in oldNamesExts)
            {
                nameTextBox.AppendText(originalName.Key + "\n---->");
                try
                {
                    newName = matchName
                        .First(k => originalName.Key.Contains(k.Key)).Value;
                }
                catch
                {
                    newName = originalName.Key;
                }
                newName += originalName.Value;
                nameTextBox.AppendText(newName + "\n");

                renameGrid.Add
                    (originalFolder + "\\" + originalName.Key,
                    originalFolder + "\\" + newName);
            }
            startRenameButton.Enabled = true;
        }

        private void startRenameButton_Click(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string,string> namePair in renameGrid)
            {
                File.Move(namePair.Key, namePair.Value);
            }
        }
    }
}
