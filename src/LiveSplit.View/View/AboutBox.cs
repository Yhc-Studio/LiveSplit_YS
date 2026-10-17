using LiveSplit.Localization;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace LiveSplit.View;

internal partial class AboutBox : Form
{
    private static string T(string source) => UiLocalizer.Translate(source, LanguageResolver.ResolveCurrentCultureLanguage());

    public AboutBox()
    {
        InitializeComponent();

        Text = string.Format(T("About {0}"), ForkInfo.ProductName);
        labelProductName.Text = ForkInfo.ProductName;
        lblVersion.Text = string.Format(T("Version {0}"), ForkInfo.Version);
        lblUpstreamVersion.Text = string.Format(T("Based on LiveSplit {0}"), ForkInfo.UpstreamBase);
        lblWebsite.Text = T("Official LiveSplit: livesplit.org");
        lblOriginalAuthors.Text = T("Original LiveSplit authors:");
        lblForkMaintainerCaption.Text = T("Fork maintained by:");
        labelDonation.Text = T("LiveSplit YS is a community-maintained fork. Support the original LiveSplit project:");
        lblMaintainer.Text = ForkInfo.Maintainer;
        lblMaintainerWebsite.Text = ForkInfo.MaintainerDisplayUrl;
        okButton.Text = T("&OK");
    }

    #region Assembly Attribute Accessors

    public string AssemblyTitle
    {
        get
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
                var titleAttribute = (AssemblyTitleAttribute)attributes[0];
                if (titleAttribute.Title != "")
                {
                    return titleAttribute.Title;
                }
            }

            return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
        }
    }

    public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

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

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // About links are optional UI conveniences. A browser launch failure
            // should never make the About dialog fail or crash LiveSplit.
        }
    }

    private void WebsiteLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenUrl("https://livesplit.org");
    }

    private void CryZeLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenUrl("https://twitter.com/CryZe107");
    }

    private void wooferzfgLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenUrl("https://twitter.com/wooferzfg");
    }

    private void MaintainerLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenUrl(ForkInfo.MaintainerUrl);
    }

    private void donateButton_Click(object sender, EventArgs e)
    {
        OpenUrl("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=R3Z2LGPKRNBNJ");
    }

    private void lblUpstreamVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenUrl(ForkInfo.UpstreamRevisionUrl);
    }
}
