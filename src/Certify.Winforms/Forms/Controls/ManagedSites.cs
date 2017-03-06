using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Certify.Management;
using Certify.Models;

namespace Certify.Forms.Controls
{
    public partial class ManagedSites : UserControl
    {
        private SiteManager siteManager;
        private ManagedSite selectedSite;

        public ManagedSites()
        {
            InitializeComponent();

            siteManager = new SiteManager();
        }

  private MainForm GetParentMainForm () => (MainForm) Parent.FindForm ();

  private void RefreshManagedSitesList()
        {
            siteManager.LoadSettings();
            var sites = siteManager.GetManagedSites();

   listView1.ShowGroups = true;

            if ( listView1.Items != null)
            {
    listView1.Items.Clear();
            }

            foreach (var s in sites)
            {
    var siteNode = new ListViewItem ( s.SiteName ) {
     Tag = s.SiteId,
     ImageIndex = 0
    };
    listView1.Items.Add(siteNode);
                if (s.IncludeInAutoRenew)
                {
                    siteNode.Group = listView1.Groups[0];
                }
                else
                {
                    siteNode.Group = listView1.Groups[1];
                }
            }
        }

        private void ManagedSites_Load( Object sender, EventArgs e)
        {
            if (!DesignMode )
            {
    RefreshManagedSitesList ();
    certRequestSettingsIIS1.Visible = false;
            }
        }

        private void listView1_SelectedIndexChanged( Object sender, EventArgs e)
        {
   lblInfo.Text = "";

   selectedSite = null;

            //selected site
            if ( listView1.SelectedItems.Count > 0)
            {
                var selectedNode = listView1.SelectedItems[0];
                if (selectedNode.Tag != null)
                {
                    var site = siteManager.GetManagedSite(selectedNode.Tag.ToString());
                    //  if (site.RequestConfig != null && site.RequestConfig.PrimaryDomain != null)
                    {
      selectedSite = site;
      PopulateSiteDetails ( site);
                    }
                }
            }
        }

  internal void ReloadManagedSites () => RefreshManagedSitesList ();

  private void PopulateSiteDetails(ManagedSite site)
        {
   certRequestSettingsIIS1.LoadManagedSite(site);
   certRequestSettingsIIS1.Visible = true;
        }

        private async void button1_Click( Object sender, EventArgs e)
        {
            if ( selectedSite != null)
            {
                var certifyManager = new CertifyManager();
                var vaultManager = GetParentMainForm().VaultManager;

                siteManager.LoadSettings();
                var result = await certifyManager.PerformCertificateRequest(vaultManager, selectedSite );
                if (!result.IsSuccess)
                {
                    MessageBox.Show("Failed to request a new certificate.");
                }
                else
                {
                    MessageBox.Show("Certificate request completed.");
                }
            }
        }

        private void listView1_MouseUp( Object sender, MouseEventArgs e)
        {
            //check for right click
        }

        private void toolStripMenuItemDelete_Click( Object sender, EventArgs e)
        {
            //delete option selected

            // User has clicked delete in tree view context menu
            if ( listView1.SelectedItems != null && listView1.SelectedItems.Count > 0)
            {
                var node = listView1.SelectedItems[0];//.SelectedNode;

                if (node != null)
                {
                    var site = siteManager.GetManagedSite(node.Tag.ToString());
                    if (site != null)
                    {
                        if (MessageBox.Show("Are you sure you want to delete the Certify settings for the managed site '" + site.SiteName + "'?", "Confirm Delete", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                        {
       //delete site
       siteManager.DeleteManagedSite(site);

       //refresh managed sites list
       ReloadManagedSites ();
       certRequestSettingsIIS1.Visible = false;
                        }
                    }
                }
            }
        }
    }
}