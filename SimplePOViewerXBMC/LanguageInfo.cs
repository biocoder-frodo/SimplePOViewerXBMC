/*
 *      Copyright (C) 2012 Team XBMC
 *      http://xbmc.org
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with XBMC; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace XBMC.International
{
    public struct TextResource
    {
        public readonly int NumId;
        public readonly string Key;
        public readonly string Text;
        public readonly string Comment;
        public readonly bool HasPluralForms;
        public readonly Dictionary<int, string> PluralForms;
        public readonly string PluralForm;

        public TextResource(int numid, string key, string text, string comment, string plural = "", Dictionary<int, string> plural_forms = null)
        {
            NumId = numid;
            Key = key;
            Text = text;
            Comment = comment;

            HasPluralForms = plural != string.Empty || plural_forms != null;
            PluralForm = plural;
            if (plural_forms == null)
            {
                PluralForms = new Dictionary<int, string>();
            }
            else
            {
                PluralForms = plural_forms;
            }
        }
    }

    public class LanguageInfo
    {

        private string[] po_allowed =
        {
            "msgctxt \""
            , "msgid \""
            , "msgid_plural \""
            , "msgstr["
            , "msgstr \""
            , "\""
        };

        private bool[]   present = new bool[5];
        private string[] current_text = new string[5];

        // 'hardwired' to the keywords above
        private const int keyword_unknown = -1;
        private const int keyword_msg_context = 0;
        private const int keyword_msg_id = 1;
        private const int keyword_msg_plural = 2;
        private const int keyword_msg_plural_string = 3;
        private const int keyword_msg_text = 4;
        private const int po_line_continuation = 5;

        

        private Dictionary<string, string> revision_info = new Dictionary<string, string>();
        private Dictionary<int, TextResource> map = new Dictionary<int, TextResource>();

        public Dictionary<int, TextResource> Text { get { return map; } }
        public Dictionary<string, string> RevisionInfo { get { return revision_info; } }

        public LanguageInfo(string file)
        {
            LoadPO(file);
        }
        public void Load(string file)
        {
            LoadPO(file, true);
        }

        private void LoadPO(string file, bool ignore_revision = false)
        {
            int line = 0;
            int strings = 0;
            string Quote = "\"";
            Dictionary<string, int> line_continuations = new Dictionary<string, int>();
            Dictionary<int, string> plurals = new Dictionary<int, string>();
            string comment_running = string.Empty;

            int min = int.MaxValue;
            int max = int.MinValue;

            string[] folders = System.IO.Path.GetDirectoryName(file).Split(System.IO.Path.DirectorySeparatorChar);
            string language = folders[folders.Count() - 1];

            string header = "msgid \"\"\r\nmsgstr \"\"";
            StreamReader po = new FileInfo(file).OpenText();
            string po_line = po.ReadLine().Trim(); line += 1;

            for (int i = 0; i <= present.GetUpperBound(0); i++)
            {
                present[i] = false;
                current_text[i] = string.Empty;
            }

            Console.Write("Reading '" + file + "' ... ");

            while (po_line.StartsWith("#"))
            {
                po_line = po.ReadLine().Trim(); line += 1;
            }

            po_line += "\r\n" + po.ReadLine(); line += 1;

            if (po_line == header)
            {
                po_line = po.ReadLine(); line += 1;
                while (po_line.Length > 0)
                {
                    string name = po_line.Substring(0, po_line.IndexOf(":"));
                    string value = po_line.Substring(po_line.IndexOf(":") + 1).Trim();
                    if (name[0] == '\"') name = name.Substring(1);
                    if (value[value.Length - 1] == '\"') value = value.Substring(0, value.Length - 1);
                    if (ignore_revision == false) revision_info.Add(name, value);

                    po_line = po.ReadLine(); line += 1;
                }
                if (ignore_revision == false) revision_info.Add("Language-Name", language);


                bool need_read = false;
                int id = int.MinValue;
                int line_contd = keyword_unknown;
                int plural_case = -1;

                while (po.EndOfStream == false)
                {
                    if (need_read)
                    {
                        po_line = po.ReadLine().Trim(); line += 1;
                    }
                    else
                    {
                        need_read = true; // read on the next pass
                    }

                    if (po_line.Length == 0)
                    {
                        if (current_text[keyword_msg_context].Length > 0)
                        {
                            try
                            {
                                if (id > max) max = id;
                                if (id < min) min = id;
                                if (current_text[keyword_msg_plural] == string.Empty && plurals.Count == 0)
                                {
                                    map.Add(id, new TextResource(id, current_text[keyword_msg_id], current_text[keyword_msg_text], comment_running));
                                }
                                else
                                {
                                    map.Add(id, new TextResource(id, current_text[keyword_msg_id], current_text[keyword_msg_text], comment_running, current_text[keyword_msg_plural], plurals));
                                }
                                strings++;
                            }
                            catch (ArgumentException ex)
                            {
                                throw new FileLoadException(string.Format("Duplicate message id on line {0} in file {1}. {2}", line, file, ex));
                            }
                        }
                        comment_running = string.Empty;
                        id = int.MinValue;
                        line_contd = keyword_unknown;
                        for (int i = 0; i <= present.GetUpperBound(0); i++)
                        {
                            present[i] = false;
                            current_text[i] = string.Empty;
                        }
                        plurals = new Dictionary<int, string>();
                    }
                    else
                    {
                        if (po_line.StartsWith("#") == true)
                        {
                            if (po_line.StartsWith("#empty string") == false)
                            {
                                comment_running += po_line;
                            }
                        }
                        else
                        {
                            int token = keyword_unknown;
                            for (int i = 0; i <= po_allowed.GetUpperBound(0); i++)
                            {
                                if (po_line.StartsWith(po_allowed[i]))
                                {
                                    token = i;
                                    break;
                                }
                            }

                            switch (token)
                            {
                                case keyword_msg_context:
                                case keyword_msg_id:
                                case keyword_msg_plural:
                                case keyword_msg_text:
                                    {
                                        if (present[token] == false)
                                        {
                                            if (token != keyword_msg_context) // no line continuation for context
                                            {
                                                line_contd = token;
                                            }
                                            current_text[token] = po_line.Substring(po_allowed[token].Length);
                                            if (current_text[token].EndsWith(Quote))
                                            {
                                                current_text[token] = current_text[token].Substring(0, current_text[token].Length - 1);
                                            }
                                            else
                                            {
                                                Console.WriteLine("Warning: Line {0} does not end with a \".", line);
                                            }
                                            present[token] = true;

                                            if (token == keyword_msg_context)
                                            {
                                                string num_id = po_line.Substring((po_allowed[token]+"#").Length);
                                                if (num_id.EndsWith(Quote))
                                                {
                                                    num_id = num_id.Substring(0, num_id.Length - 1);
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Warning: Line {0} does not end with a \".", line);
                                                }
                                                if (int.TryParse(num_id, out id) == false)
                                                    throw new FileLoadException(string.Format("Expecting numeric message id on line {0} in file {1}.", line, file));
                                             }
                                        }
                                        else
                                        {
                                            throw new FileLoadException(string.Format("Unexpected PO token on line {0} in file {1}.", line, file));
                                        }

                                        break;
                                    }
                                case keyword_msg_plural_string:
                                    {
                                        plural_case = -1;
                                        line_contd = token;

                                        string plural = po_line.Substring((po_allowed[token].Length));
                                        if (plural.Contains(']'))
                                        {
                                            string p = plural.Substring(0,plural.IndexOf(']')).Trim();

                                            if (int.TryParse(p, out plural_case) == true)
                                            {
                                                plural = plural.Substring(plural.IndexOf(']') + 1).Trim();
                                                if (plural.StartsWith(Quote))
                                                {
                                                    

                                                    if (plurals.ContainsKey(plural_case) == false)
                                                    {
                                                        if (plural.EndsWith(Quote))
                                                        {
                                                            plural = plural.Substring(1, plural.Length - 2);
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("Warning: Line {0} does not end with a \".", line);
                                                        }
                                                        plurals.Add(plural_case, plural);
                                                    }
                                                    else
                                                    {
                                                        throw new FileLoadException(string.Format("Duplicate plural string resource [case {0}] on line {1} in file {2}.", plural_case, line, file));
                                                    }
                                                }
                                                else
                                                {
                                                    throw new FileLoadException(string.Format("Expecting plural string resource [case {0}] on line {1} in file {2}.", plural_case, line, file));
                                                }
                                            }
                                            else
                                            {
                                                throw new FileLoadException(string.Format("Expecting numeric plural case id on line {0} in file {1}.", line, file));
                                            }
                                        }
                                        else
                                        {
                                            throw new FileLoadException(string.Format("Expecting terminating ] on line {0} in file {1}.", line, file));
                                        }

                                        break;
                                    }
                                case po_line_continuation:
                                    {
                                        if (line_contd != keyword_unknown)
                                        {
                                            if (line_continuations.ContainsKey(current_text[keyword_msg_context]) == false)
                                            {
                                                line_continuations.Add(current_text[keyword_msg_context], 1);
                                            }

                                            string text = po_line.Substring(po_allowed[token].Length);
                                            if (text.EndsWith(Quote))
                                            {
                                                text = text.Substring(0, text.Length - 1);
                                            }
                                            else
                                            {
                                                Console.WriteLine("Warning: Line {0} does not end with a \".", line);
                                            }
                                            if (line_contd == keyword_msg_plural_string)
                                            {
                                                if (plurals.ContainsKey(plural_case))
                                                {
                                                    plurals[plural_case] += text;
                                                }
                                                else
                                                {
                                                    throw new FileLoadException(string.Format("Expecting to continue a string declaration from the previous line on line {0} in file {1}", line, file));
                                                }
                                            }
                                            else
                                            {
                                                current_text[line_contd] += text;
                                            }
                                        }
                                        else
                                        {
                                            throw new FileLoadException(string.Format("Expecting to continue a string declaration from the previous line on line {0} in file {1}", line, file));
                                        }
                                        break;
                                    }
                                case keyword_unknown:
                                    {
                                        throw new FileLoadException(string.Format("Expecting PO token on line {0} in file {1}.", line, file));
                                    }
                                default:
                                    {
                                        goto case keyword_unknown;
                                    }
                            }
                        }
                    }
                }

                if (current_text[keyword_msg_context].Length > 0)
                {
                    try
                    {
                        if (id > max) max = id;
                        if (id < min) min = id;
                        if (current_text[keyword_msg_plural] == string.Empty && plurals.Count == 0)
                        {
                            map.Add(id, new TextResource(id, current_text[keyword_msg_id], current_text[keyword_msg_text], comment_running));
                        }
                        else
                        {
                            map.Add(id, new TextResource(id, current_text[keyword_msg_id], current_text[keyword_msg_text], comment_running, current_text[keyword_msg_plural], plurals));
                        }
                        strings++;
                    }
                    catch (ArgumentException ex)
                    {
                        throw new FileLoadException(string.Format("Duplicate message id on line {0} in file {1}. {2}", line, file, ex));
                    }
                }

                Console.WriteLine(strings.ToString() + " string(s) [" + min.ToString() + " thru " + max.ToString() + "]");
                
                foreach (string contd in line_continuations.Keys)
                {
                    Console.WriteLine(string.Format("Warning: Line continuation for context {0} in file {1}", contd, file));
                }
            }
            else
            {
                throw new FileLoadException(string.Format("Expecting message header magic on line {0} in file {1}.", line, file));
            }
        }
    }

}
