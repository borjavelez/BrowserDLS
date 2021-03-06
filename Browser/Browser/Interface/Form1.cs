﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Browser.Utilities;

namespace Browser
{
    public partial class Form1 : Form
    {
        Crawler cr;
        public Form1()
        {
            cr = new Crawler();
            InitializeComponent();
            splitContainer1.Top = 200;
            splitContainer1.Left = (this.ClientSize.Width - splitContainer1.Width) / 2;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            splitContainer1.Top = 40;

            //SEARCH TERM AND RETURN LIST<STRING> searchResult
            listBox1.DataSource = cr.selectValue(textBox1.Text);
            listBox1.Visible = true;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click((object)sender, (EventArgs)e);
            }
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int inserted = cr.indexFilesAndDirectories();
            MessageBox.Show(inserted + " row(s) affected");
        }

        public void showMessage(String message)
        {
            MessageBox.Show(message);
        }
    }
}
