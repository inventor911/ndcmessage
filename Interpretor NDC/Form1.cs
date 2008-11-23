using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Interpretor_NDC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            EnhancedConfigurationParametersLoad t = new EnhancedConfigurationParametersLoad("3∟∟∟1A∟∟0000201002030020600109002100021100212001130011400115001∟00010010100200503040040150502006030070050802009010960059801099005", 13);
            Console.WriteLine(t.ResponseFlag);
            //t.Configure("1 ∟123∟456∟02n");

        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialogMessageIn.ShowDialog();

            string pathFile = openFileDialogMessageIn.FileName;

            if (pathFile == "" || pathFile == null)
                return;

            FileStream inStream = File.OpenRead(pathFile);
            BinaryReader br = new BinaryReader(inStream);

            FileInfo f = new FileInfo("MessageIn(after split).txt");
            StreamWriter writer = f.CreateText(); 

            char ch = br.ReadChar();
            char ch2;
            char ch3;
            char ch4;


            while (br.BaseStream.Position < br.BaseStream.Length-1)
            {
                string message = "";

                if (ch == '3')                         // "3 ∟"
                {
                    ch2 = br.ReadChar();
                    ch3 = br.ReadChar();
                    if (ch2 == 32 && ch3 == 28)
                    {
                        message = Convert.ToString(ch) + Convert.ToString(ch2) + Convert.ToString(ch3);
                        ch = br.ReadChar();
                        while (true) // "] \r\n"
                        {
                            if (ch == ']')
                            {
                                ch2 = br.ReadChar();
                                ch3 = br.ReadChar();
                                ch4 = br.ReadChar();
                                if (ch2 == 32 && ch3 == '\r' && ch4 == '\n')
                                    break;
                                else
                                    br.BaseStream.Position -= 3;
                            }
                            message += Convert.ToString(ch);
                            ch = br.ReadChar();
                            
                        }
                        
                        //Console.WriteLine(message);
                        writer.Write(message);
                        writer.Write("\r\n");
                    }
                    else
                        br.BaseStream.Position -= 2;
                    //dataGridViewMessageIn.Row
                }

                ch = br.ReadChar();
            }

            inStream.Close();
            writer.Close();

        }
    }
}