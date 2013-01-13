using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace SimplePOViewerXBMC
{
    public class Preferences
    {
        public List<string> LoadOnStartup = new List<string>();
        public string RootFolder = string.Empty;

        private string profile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                 ".xmbc.translations.SimplePOViewer.profile");


        public void Restore()
        {
            try
            {
                if (File.Exists(profile))
                {
                    XDocument cfg = XDocument.Load(profile);
                    RootFolder = cfg.Element("profile").Element("xbmcroot").Value;
                    LoadOnStartup = GetLanguages(cfg.Element("profile").Element("languages").Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error accessing profile: " + ex.Message);
            }
        }

        public void Save()
        {
            try
            {
                XDocument cfg = new XDocument(
                                               new XElement("profile",
                                               new XElement("xbmcroot"), new XElement("languages")));

                cfg.Element("profile").Element("xbmcroot").Value = RootFolder;
                cfg.Element("profile").Element("languages").Value = SetLanguages(LoadOnStartup);
                cfg.Save(profile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error accessing profile: " + ex.Message);
                throw ex;
            }
        }

        private List<string> GetLanguages(string text)
        {
            return text.Split(';').ToList();
        }

        private string SetLanguages(List<string> list)
        {
            string result = string.Empty;

            if (list.Count > 0)
            {
                foreach (string s in list)
                {
                    result += s + ";";
                }
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }
    }
}
