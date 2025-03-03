using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Anteckningar
{
    public partial class Anteckningar : Form
    {
        public Anteckningar()
        {
            InitializeComponent();
        }

        string originalTxt = "";
        string fileName = "Namnlös";
        const string anteckningar = " - Anteckningar";
        string filePath = string.Empty;
        bool textChanged = false;

        private void avslutaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void öppnaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textChanged)
            {
                string message = $"Vill du spara ändringar för {fileName}?";
                const string caption = "Anteckningar";
                var result = MessageBox.Show(message, caption,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    sparaToolStripMenuItem.PerformClick();
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = "c:\\Dokument";
                ofd.Filter = "txt files (*.txt)|*.txt";
                ofd.FilterIndex = 2;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var fileStream = ofd.OpenFile();
                    fileName = Path.GetFileName(ofd.FileName);
                    filePath = Path.GetFullPath(ofd.FileName);

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        originalTxt = reader.ReadToEnd();
                        textBox1.Text = originalTxt;
                        textBox1.SelectionStart = textBox1.Text.Length;
                        textChanged = false;
                        setFormText();
                    }
                }
            }        
        }

        private void sparaSomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = fileName;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filePath = Path.GetFullPath(saveFileDialog1.FileName);
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    using (StreamWriter writer = new StreamWriter(myStream))
                    {
                        writer.Write(textBox1.Text);
                    }
                    fileName = Path.GetFileName(saveFileDialog1.FileName);
                    originalTxt = textBox1.Text;
                    textChanged = false;
                    setFormText();
                    myStream.Close();
                }
            }
        }

        private void sparaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filePath != string.Empty)
            {
                File.WriteAllText(filePath, textBox1.Text);
                originalTxt = textBox1.Text;
                textChanged = false;
                setFormText();
            }
            else
            {
                sparaSomToolStripMenuItem.PerformClick();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text != originalTxt)
            {
                textChanged = true;
            }
            else if (textBox1.Text == originalTxt)
            {
                textChanged = false;
            }

            nbrOfWords();

            setFormText();
        }

        void nbrOfWords()
        {
            string[] words = textBox1.Text.Split(new char[] 
            { ' ', '\t', '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries);

            int wordCount = words.Length;
            int rowCount = textBox1.Lines.Length;
            //int charSpaces = textBox1.Text.Replace("\r\n", "\n").Length; //Med radbrytningar
            int charSpacesCount = textBox1.Text.Replace("\r\n", "").Length; //Utan radbrytningar
            int charCount = textBox1.Text.Replace("\r\n", "").Replace(" ", "").Length;

            statusMellanslag.Text = $"Tecken(mellanslag): {charSpacesCount.ToString()}";
            statusTecken.Text = $"Tecken: {charCount.ToString()}";
            statusOrd.Text = $"Ord: {wordCount.ToString()}";
            statusRader.Text = $"Rader: {rowCount.ToString()}";

        }

        void setFormText()
        {
            if (textChanged)
            {
                this.Text = $"*{fileName}.txt" + anteckningar;
            }
            else
            {
                this.Text = fileName + anteckningar;
            }
        }

        private void nyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Text = string.Empty;
            originalTxt = "";
            fileName = "Namnlös";
            filePath = string.Empty;
            textChanged = false;
            this.Text = fileName + anteckningar;
        }

        private void Anteckningar_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (textChanged)
            {
                string message = $"Vill du spara ändringar för {fileName}?";
                const string caption = "Anteckningar";
                var result = MessageBox.Show(message, caption,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    sparaToolStripMenuItem.PerformClick();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
