using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

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
            ConfigurationParametersLoad t = new ConfigurationParametersLoad("3∟∟∟13∟2002000000002002∟∟00010010100200503040040150502006030070050802009010960059801099005");
            Console.WriteLine(t.ResponseFlag);
            //t.Configure("1 ∟123∟456∟02n");

        }
    }
}