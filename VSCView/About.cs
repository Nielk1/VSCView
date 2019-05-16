using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSCView
{
    partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;
            this.textBoxDescription.Text = AssemblyDescription;

            {
                if (!Directory.Exists("themes")) Directory.CreateDirectory("themes");
                string[] themeParents = Directory.GetDirectories("themes");

                foreach (string themeParent in themeParents)
                {
                    string ThemeName = Path.GetFileName(themeParent);
                    List<ThemeDescAuthors> authors = null;
                    try
                    {
                        string themeMetaFile = Path.Combine(themeParent, "theme.json");
                        if (File.Exists(themeMetaFile))
                        {
                            ThemeDesc desc = JsonConvert.DeserializeObject<ThemeDesc>(File.ReadAllText(themeMetaFile));
                            if (!string.IsNullOrWhiteSpace(desc.name))
                                ThemeName = desc.name;
                            authors = desc.authors;
                        }
                        authors?.ForEach(dr =>
                        {
                            if (string.IsNullOrWhiteSpace(dr.name))
                            {
                                if (string.IsNullOrWhiteSpace(dr.url))
                                {
                                    dr.name = null;
                                    dr.url = null;
                                }
                                else
                                {
                                    dr.name = dr.url;
                                }
                            }
                            else if (string.IsNullOrWhiteSpace(dr.url))
                            {
                                dr.url = null;
                            }
                        });
                        authors = authors?.Where(dr => dr.name != null).ToList();
                        if (authors?.Any(dr => dr.url != null) ?? false) // at least one url
                        {
                            LinkLabel label = new LinkLabel();
                            label.Text = ThemeName;
                            label.LinkColor = Color.Yellow;
                            label.ActiveLinkColor = Color.GreenYellow;
                            label.VisitedLinkColor = Color.DarkGreen;
                            foreach (var author in authors)
                            {
                                label.Text += " - ";
                                if (author.url != null)
                                {
                                    label.Links.Add(label.Text.Length, author.name.Length, author.url);
                                }
                                label.Text += author.name;
                            }
                            label.LinkClicked += Label_LinkClicked;
                            label.Padding = new Padding(0, 0, 0, 5);
                            label.AutoSize = true;
                            pnlThemes.Controls.Add(label);
                        }
                        else if ((authors?.Count ?? 0) > 0)
                        {
                            Label label = new Label();
                            label.Font = LinkLabel.DefaultFont;
                            label.Text = ThemeName + " - " + string.Join(" - ", authors.Select(dr => dr.name));
                            label.Padding = new Padding(0, 0, 0, 5);
                            label.AutoSize = true;
                            pnlThemes.Controls.Add(label);
                        }
                        else
                        {
                            Label label = new Label();
                            label.Font = LinkLabel.DefaultFont;
                            label.Text = ThemeName;
                            label.Padding = new Padding(0, 0, 0, 5);
                            label.AutoSize = true;
                            pnlThemes.Controls.Add(label);
                        }
                    }
                    catch { }
                }
            }
        }

        private void Label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            e.Link.Visited = true;
            System.Diagnostics.Process.Start((string)e.Link.LinkData);
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion
    }
}
