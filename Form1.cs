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

        string originalTxt = ""; //Variable för att kunna jämföra om texten har ändrats
        string fileName = "Namnlös"; //Default namn på fil
        const string anteckningar = " - Anteckningar"; //Const sträng som visas i this.text med filnmanet
        string filePath = string.Empty; //Variabel för att se vart filen ska sparas
        bool textChanged = false; //För att se om texten har ändrats
        bool saveSuccess = false;//För att se om filen sparades

        private void avslutaToolStripMenuItem_Click(object sender, EventArgs e) //Avslutar programmet
        {
            Close();
        }


        private void öppnaToolStripMenuItem_Click(object sender, EventArgs e)//För att öppna en fil
        {
            if (textChanged) //Kollar om texten har ändrats för att fråga om man vill spara
            {
                string message = $"Vill du spara ändringar för {fileName}?";
                const string caption = "Anteckningar";
                var result = MessageBox.Show(message, caption,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    sparaToolStripMenuItem.PerformClick();
                    if (!saveSuccess)//Kollar om användaren avbröt sparning av fil
                    {
                        return;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            using (OpenFileDialog ofd = new OpenFileDialog())//Öppnar dialogruta för att öppnna fil
            {
                ofd.InitialDirectory = "c:\\Dokument";
                ofd.Filter = "txt files (*.txt)|*.txt";
                ofd.FilterIndex = 2;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var fileStream = ofd.OpenFile();
                    fileName = Path.GetFileName(ofd.FileName);//Hämtar filnamnet 
                    filePath = Path.GetFullPath(ofd.FileName);//Hämtar filvägen för att kunna använda senare

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        originalTxt = reader.ReadToEnd();//Sätter variabel till texten i filen för att kunna jämföra om den ändras senare
                        textBox1.Text = originalTxt;//Lägger till texten till textboxen 
                        textBox1.SelectionStart = textBox1.Text.Length;//Sätter markören i slutet av texten
                        textChanged = false;
                        setFormText();//Kallar på metod för att sätta namnet på filen i formtexten
                    }
                }
            }        
        }

        private void sparaSomToolStripMenuItem_Click(object sender, EventArgs e) //Spara som knapen
        {
            saveSuccess = false;
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.FileName = fileName;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK) //Öppnar dialogruta för att spara fil
            {
                filePath = Path.GetFullPath(saveFileDialog1.FileName);
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    using (StreamWriter writer = new StreamWriter(myStream))
                    {
                        writer.Write(textBox1.Text); //Skriver text i textbox till filen
                    }
                    fileName = Path.GetFileName(saveFileDialog1.FileName);//Hämtar filvägen
                    originalTxt = textBox1.Text; //Sätter texten i textbox
                    textChanged = false; 
                    setFormText(); //Sätter text i form
                    saveSuccess = true; //Sätter till true för att visa att det sparades
                    myStream.Close();
                }
            }
        }

        private void sparaToolStripMenuItem_Click(object sender, EventArgs e) //Kollar om en filväg finns när man klickar på "spara"
                                                                              //och sparar där, eller öppnar dialogrutan för att spara om det inte är satt
        {
            saveSuccess = false;
            if (filePath != string.Empty)//Om filePath är satt sparas texten till existerande filen
            {
                File.WriteAllText(filePath, textBox1.Text);
                originalTxt = textBox1.Text;
                textChanged = false;
                setFormText();
                saveSuccess = true;
            }
            else //Kallar på sparaSom 
            {
                sparaSomToolStripMenuItem.PerformClick();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) //Kollar om texten har ändrats samt kallar på metoder
        {
            if (textBox1.Text != originalTxt) //Sätter variable till true om
                                              //texten har ändrats från originaltexten
            {
                textChanged = true;
            }
            else if (textBox1.Text == originalTxt)//Sätter till false om texten är samma
            {
                textChanged = false;
            }

            nbrOfWords(); //Uppdaterar statusstrip

            setFormText();//Sätter form texten
        }

        void nbrOfWords() //Gör uträkningar för statusstrip
        {
            string[] words = textBox1.Text.Split(new char[] //Tar texten från textbox och tar bort whitespace och tomma strängar för att räkna ord
            { ' ', '\t', '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries);

            int wordCount = words.Length; //Räknar ord 
            int rowCount = textBox1.Lines.Length; //Räknar rader
            //int charSpaces = textBox1.Text.Replace("\r\n", "\n").Length; //Räknar tecken med radbrytningar och gör så att det räknas som ett tecken
            int charSpacesCount = textBox1.Text.Replace("\r\n", "").Length; //Räknar tecken med mellanslag utan radbrytningar
            int charCount = textBox1.Text.Replace("\r\n", "").Replace(" ", "").Length; //Räknar tecken utan mellanslag och radbrytningar

            statusMellanslag.Text = $"Tecken(mellanslag): {charSpacesCount.ToString()}";//Lägger in värden i statusstrip
            statusTecken.Text = $"Tecken: {charCount.ToString()}";//Lägger in värden i statusstrip
            statusOrd.Text = $"Ord: {wordCount.ToString()}";//Lägger in värden i statusstrip
            statusRader.Text = $"Rader: {rowCount.ToString()}";//Lägger in värden i statusstrip

        }

        void setFormText() //Sätter texten i form beroende på om texten har ändrats eller inte
        {
            if (textChanged)
            {
                this.Text = $"*{fileName}.txt" + anteckningar; //Lägger till "*" i början av filnamnet om text har ändrats
            }
            else
            {
                this.Text = fileName + anteckningar; //Tar bort "*" om texten inte har ändrats
            }
        }

        private void nyToolStripMenuItem_Click(object sender, EventArgs e)//Rensar textrutan och variabler
        {
            if (textChanged)//Frågar om man vill spara om texten har ändrats
            {
                string message = $"Vill du spara ändringar för {fileName}?";
                const string caption = "Anteckningar";
                var result = MessageBox.Show(message, caption,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    sparaToolStripMenuItem.PerformClick();
                    if (!saveSuccess)
                    {
                        return;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }
            textBox1.Text = string.Empty; //Sätter tillbaka variabler till default-värden
            originalTxt = "";
            fileName = "Namnlös";
            filePath = string.Empty;
            textChanged = false;
            saveSuccess = false;
            this.Text = fileName + anteckningar;
        }

        private void Anteckningar_FormClosing(object sender, FormClosingEventArgs e)//När programmet stängs ned
        {
            if (textChanged) //Kollar om texten har ändrats för att fråga om det ska sparas
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
                else if (result == DialogResult.Cancel) //Avbryter formClosing eventet om man klickar avbryt
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
