using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using XBMC.International;


namespace SimplePOViewerXBMC
{
    public partial class frmMain : Form
    {
        private DirectoryInfo xbmc = null;
        private Dictionary<string,LanguageInfo> languages = new Dictionary<string,LanguageInfo>();       


        #region Form entry points

        public frmMain(DirectoryInfo XBMC_ROOT)
        {
            InitializeComponent();

            xbmc = XBMC_ROOT;

        }
        internal frmMain()
        {
            ;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // specificy path to strings.po without using platform path specifier (and assume case-sensitive filenames)
                DirectoryInfo[] language = xbmc.GetDirectories("language");
                if (language.Count() == 1)
                {

                    DirectoryInfo[] lang = language[0].GetDirectories();

                    foreach (DirectoryInfo di in lang)
                    {
                        comboBox1.Items.Add(di.Name);
                    }

                    List<string> addons = GetPOAddons();

                    foreach (string addon in addons)
                    {
                        comboBox2.Items.Add(addon);
                    }

                    comboBox2.SelectedItem = comboBox2.Items[comboBox2.Items.IndexOf("skin.confluence")];
                }
                else
                {
                    MessageBox.Show("Unable to find the language folder. Did you set XBMC_ROOT or specify the folder on the commandline?");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            LoadWithAddon();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                if (comboBox1.Text != "English")
                {
                    ColumnHeader[] ch = new ColumnHeader[] { new ColumnHeader() };

                    LanguageInfo lng = LoadLanguage(comboBox1.Text, comboBox2.SelectedItem.ToString());

                    languages.Add(comboBox1.Text, lng);

                    ch[0].Text = string.Format("Translation({0})", lng.RevisionInfo["Language"].Replace(@"\n", ""));
                    ch[0].Width = listView1.Columns[1].Width;
                    listView1.Columns.AddRange(ch);
                    
                    PopulateListBox(listView1);


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            frmEvalID evalform = new frmEvalID(languages);
            evalform.ShowDialog();

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadWithAddon();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportToFile();
        }

        #endregion

        List<string> GetPOAddons()
        {
            List<string> addons = new List<string>();

            // specificy path to strings.po without using platform path specifier (and assume case-sensitive filenames)
            DirectoryInfo[] folders = xbmc.GetDirectories("addons").First().GetDirectories();

            foreach (DirectoryInfo di in folders)
            {
                DirectoryInfo en_folder = null;

                en_folder = new DirectoryInfo(System.IO.Path.Combine(di.FullName, "language", "English"));

                if (en_folder.Exists)
                {
                    FileInfo[] strings_po = en_folder.GetFiles("strings.po");
                    if (strings_po.Count() > 0)
                    {
                        addons.Add(di.Name);
                    }
                }
                else
                {
                    en_folder = new DirectoryInfo(System.IO.Path.Combine(di.FullName, "resources", "language", "English"));

                    if (en_folder.Exists)
                    {
                        FileInfo[] strings_po = en_folder.GetFiles("strings.po");
                        if (strings_po.Count() > 0)
                        {
                            addons.Add(di.Name);
                        }
                    }
                }
            }

            return addons;
        }

        string GetFileStringsPO(string addon, string Language)
        {
            string root_folder = string.Empty;

            if (addon == string.Empty)
            {
                root_folder = "language";
            }
            else
            {
                root_folder = System.IO.Path.Combine("addons", addon);
            }

            try
            {
                DirectoryInfo language = null;
                DirectoryInfo en_folder = null;

                // specificy path to strings.po without using platform path specifier (and assume case-sensitive filenames)
                if (addon == string.Empty)
                {
                    language = xbmc.GetDirectories(root_folder).First();
                }
                else
                {
                    language = new DirectoryInfo(System.IO.Path.Combine(xbmc.FullName, root_folder, "language"));
                    if (language.Exists == false) language = new DirectoryInfo(System.IO.Path.Combine(xbmc.FullName, root_folder, "resources", "language"));
                }

                en_folder = language.GetDirectories(Language).First();
                FileInfo[] strings_po = en_folder.GetFiles("strings.po");

                return strings_po[0].FullName;
            }
            catch
            {
                throw new FileNotFoundException(string.Format("Cannot find the language file [{2}] for {0} in XBMC_ROOT: {1}", Language, xbmc.FullName, root_folder));
            }
        }
        
        string GetFileStringsPO(string Language)
        {
            return GetFileStringsPO(string.Empty, Language);
        }

        private LanguageInfo LoadLanguage(string language, string addon_resource)
        {
            LanguageInfo lng = new LanguageInfo(GetFileStringsPO(language));
            lng.Load(GetFileStringsPO(addon_resource, language));

            if (language == "English")
            {
                foreach (TextResource t in lng.Text.Values)
                {
                    if (t.Text.Length > 0 && t.Text != t.Key)
                    {
                        MessageBox.Show("There should be no translations in the English message file?");
                        break;
                    }

                }
            }
            return lng;
        }
        private void LoadWithAddon()
        {
            try
            {
                listView1.Items.Clear();

                if (languages.Count == 0)
                {
                    languages.Add("English", LoadLanguage("English", comboBox2.SelectedItem.ToString()));
                }
                else
                {
                    foreach (string lng in languages.Keys.ToList())
                    {
                        languages[lng] = LoadLanguage(lng, comboBox2.SelectedItem.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            PopulateListBox(listView1);

            comboBox1.SelectedItem = comboBox1.Items[comboBox1.Items.IndexOf("English")];
        }
        private string ResourceText(Dictionary<int, TextResource> map, int key)
        {
            if (map.ContainsKey(key) == true)
                return map[key].Text;
            else
                return "";
        }


        private void PopulateListBox(ListView control)
        {
            bool filterpercent = checkBox1.Checked;
            LanguageInfo en = languages["English"];

            if (en != null)
            {
                control.Items.Clear();
                foreach (TextResource t in en.Text.Values)
                {
                    
                    if (filterpercent)
                    {
                        if (t.Key.Contains('%'))
                        {
                            ListViewItem lvi = control.Items.Add(t.NumId.ToString());
                            lvi.SubItems.Add(t.Comment);

                            lvi.SubItems.Add(t.Key);

                            for (int i = 1; i < languages.Count; i++)
                            {
                                lvi.SubItems.Add(ResourceText(languages.Values.ElementAt(i).Text, t.NumId));

                            }

                        }

                    }
                    else
                    {
                        ListViewItem lvi = control.Items.Add(t.NumId.ToString());
                        lvi.SubItems.Add(t.Comment);
                        lvi.SubItems.Add(t.Key);

                        for (int i = 1; i < languages.Count; i++)
                        {
                            lvi.SubItems.Add(ResourceText(languages.Values.ElementAt(i).Text, t.NumId));

                        }
 
                    }
                }
            }
        }


        private void ExportToFile()
        {
            try
            {
                ListView lv = listView1;
                StreamWriter sw = new StreamWriter(xbmc.FullName + @"\Export.Languages.Selection.txt");
                foreach (ListViewItem li in lv.Items)
                {
                    //sw.Write(li.Text);
                    foreach (ListViewItem.ListViewSubItem si in li.SubItems)
                    {
                        sw.Write("\t" + si.Text);
                    }
                    sw.WriteLine();
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

 

    }
}

