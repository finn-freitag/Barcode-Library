using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarcodeGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(comboBox1.Text == "Code128")
            {
                pictureBox1.Image = Barcode.Code128.Create(textBox1.Text, 75, 75, 2);
            }
            if(comboBox1.Text == "Code39")
            {
                pictureBox1.Image = Barcode.Code39.Create(textBox1.Text, 75, 75, 2, checkBox1.Checked);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Portable Network Graphic|*.png";
            if(sfd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
