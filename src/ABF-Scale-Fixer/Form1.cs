using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ABF_Scale_Fixer
{
    public partial class Form1 : Form
    {
        private AbfScaleFixer fixer = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var version = typeof(Form1).Assembly.GetName().Version;
            Text = $"ABF Scale Fixer {version.Major}.{version.Minor}";
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string filePath in filePaths)
            {
                LoadABF(filePath);
                break;
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        string loadedAbf = null;
        private void LoadABF(string abfFilePath)
        {
            if (abfFilePath == null)
                return;

            loadedAbf = abfFilePath;
            try
            {
                fixer = new AbfScaleFixer(abfFilePath);
                tbScale.Text = fixer.fInstrumentScaleFactor.ToString();
                lblStatus.Text = $"Loaded ABF2 file: {System.IO.Path.GetFileName(abfFilePath)}";
                btnRead.Enabled = true;
                btnSave.Enabled = true;
                tbScale.Enabled = true;
            }
            catch
            {
                fixer = null;
                tbScale.Text = "?";
                lblStatus.Text = $"invalid ABF2 file: {System.IO.Path.GetFileName(abfFilePath)}";
                btnRead.Enabled = false;
                btnSave.Enabled = false;
                tbScale.Enabled = false;
            }
        }

        private void BtnRead_Click(object sender, EventArgs e)
        {
            LoadABF(loadedAbf);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            fixer.fInstrumentScaleFactor = double.Parse(tbScale.Text);

            string suggestedFileName = System.IO.Path.GetFileNameWithoutExtension(loadedAbf) + "-fixed.abf";
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.InitialDirectory = System.IO.Path.GetDirectoryName(loadedAbf);
            savefile.FileName = suggestedFileName;
            savefile.Filter = "ABF Files (*.abf)|*.abf|All files (*.*)|*.*";
            if (savefile.ShowDialog() == DialogResult.OK)
            {
                fixer.Save(savefile.FileName);
                lblStatus.Text = $"Saved: {System.IO.Path.GetFileName(savefile.FileName)}";
            }
        }

        private void TbScale_TextChanged(object sender, EventArgs e)
        {
            double newScaleFactor;
            btnSave.Enabled = double.TryParse(tbScale.Text, out newScaleFactor);
        }
    }
}
