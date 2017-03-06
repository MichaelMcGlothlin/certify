using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Certify.Forms
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
   lblAppName.Text = Properties.Resources.LongAppName;
   lblAppVersion.Text = ProductVersion + " - " + Properties.Resources.ReleaseDate;
   lnkPublisherWebsite.Text = Properties.Resources.AppWebsiteURL;
   txtCredits.Text = Properties.Resources.Credits;
        }

        private void lnkPublisherWebsite_LinkClicked( Object sender, LinkLabelLinkClickedEventArgs e)
        {
            var sInfo = new ProcessStartInfo(Properties.Resources.AppWebsiteURL);
            Process.Start(sInfo);
        }
    }
}