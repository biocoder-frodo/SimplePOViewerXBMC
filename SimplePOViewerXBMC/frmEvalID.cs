using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XBMC.International;

namespace SimplePOViewerXBMC
{
    public partial class frmEvalID : Form
    {
        private Dictionary<string,LanguageInfo> lng = null;
        public frmEvalID()
        {
            InitializeComponent();
        }
        public frmEvalID(Dictionary<string,LanguageInfo> languages)
        {
            InitializeComponent();
            lng = languages;
        }
        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void Print(string text)
        {
            listBox1.Items.Add(text);
            Console.WriteLine(text);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                string idtext = textBox1.Text;

                if (idtext.Trim().Length > 0)
                {
                    try
                    {
                        Print("Localization strings :" + idtext);
                        string[] ids = textBox1.Text.Split(new char[] { ' ' });
                        foreach (LanguageInfo l in lng.Values)
                        {
                            string result = string.Empty;
                            foreach (string id in ids)
                            {
                                int i = 0;
                                if (int.TryParse(id, out i))
                                {
                                    if (l.Text.ContainsKey(i) == true)
                                        if (l.Text[i].Text.Length > 0)
                                        {
                                            result = result + " " + l.Text[i].Text;
                                        }
                                        else
                                        {
                                            if (l.Text[i].Key.Length > 0)
                                            {
                                                result = result + " " + l.Text[i].Key;
                                            }
                                            else
                                            {
                                                result = result + " " + lng["English"].Text[i].Key;
                                            }
                                        }
                                }
                            }
                            Print(l.RevisionInfo["Language"].Replace(@"\n", "") + ": " + result);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
    }
}
