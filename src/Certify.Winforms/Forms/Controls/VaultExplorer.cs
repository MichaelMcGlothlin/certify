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
using ACMESharp.Vault.Model;
using System.IO;

namespace Certify.Forms.Controls
{
    public partial class VaultExplorer : UserControl
    {
        internal VaultManager VaultManager = null;

  private MainForm GetParentMainForm () => (MainForm) Parent.FindForm ();

  public VaultExplorer()
        {
            InitializeComponent();
        }

        private void populateTreeView(VaultInfo vaultConfig)
        {
            if ( treeView1.Nodes != null)
            {
    treeView1.Nodes.Clear();
            }

            var certManager = new CertificateManager();
   // start off by adding a base treeview node
   var mainNode = new TreeNode () {
    Name = "Vault",
    Text = "Vault",
    ImageIndex = (Int32) ImageList.Vault
   };
   mainNode.SelectedImageIndex = mainNode.ImageIndex;

   treeView1.Nodes.Add(mainNode);

            if (vaultConfig.Identifiers != null)
            {
    var domainsNode = new TreeNode ( "Domains & Certificates (" + vaultConfig.Identifiers.Count + ")" ) {
     ImageIndex = (Int32) ImageList.Globe
    };
    domainsNode.SelectedImageIndex = domainsNode.ImageIndex;

                foreach (var i in vaultConfig.Identifiers)
                {
     var node = new TreeNode ( i.Dns ) {
      Tag = i,

      ImageIndex = (Int32) ImageList.Globe
     };
     node.SelectedImageIndex = node.ImageIndex;

                    if (vaultConfig.Certificates != null)
                    {
                        foreach (var c in vaultConfig.Certificates)
                        {
                            if (c.IdentifierRef == i.Id)
                            {
        //add cert
        var certNode = new TreeNode ( c.Alias ) {
         Tag = c,

         ImageIndex = (Int32) ImageList.Cert
        };
        certNode.SelectedImageIndex = certNode.ImageIndex;

        //get info from get if possible, use that to style the parent node (expiry warning)

        var certPath = VaultManager.GetCertificateFilePath(c.Id);
        var crtDerFilePath = certPath + "\\" + c.CrtDerFile;

                                if (File.Exists(crtDerFilePath))
                                {
                                    var cert = certManager.GetCertificate(crtDerFilePath);

                                    var expiryDate = DateTime.Parse(cert.GetExpirationDateString());
                                    var timeLeft = expiryDate - DateTime.Now;
                                    node.Text += " (" + timeLeft.Days + " days remaining)";
                                    if (timeLeft.Days < 30)
                                    {
                                        node.ForeColor = Color.Orange;
                                    }
                                    if (timeLeft.Days < 7)
                                    {
                                        node.ForeColor = Color.Red;
                                    }
                                }
                                else
                                {
                                    node.ForeColor = Color.Gray;
                                }
                                node.Nodes.Add(certNode);
                            }
                        }
                    }
                    domainsNode.Nodes.Add(node);
                }

                mainNode.Nodes.Add(domainsNode);
                domainsNode.Expand();
            }

            if (vaultConfig.Registrations != null)
            {
    var contactsNode = new TreeNode ( "Registered Contacts (" + vaultConfig.Registrations.Count + ")" ) {
     ImageIndex = (Int32) ImageList.Person
    };
    contactsNode.SelectedImageIndex = contactsNode.ImageIndex;

                foreach (var i in vaultConfig.Registrations)
                {
                    var title = i.Registration.Contacts.FirstOrDefault();
     var node = new TreeNode ( title ) {
      Tag = i,

      ImageIndex = (Int32) ImageList.Person
     };
     node.SelectedImageIndex = node.ImageIndex;

                    contactsNode.Nodes.Add(node);
                }

                mainNode.Nodes.Add(contactsNode);

                contactsNode.Expand();
            }

            if (mainNode.Nodes.Count == 0)
            {
                mainNode.Nodes.Add("(Empty)");
            }
            else
            {
                mainNode.Expand();
            }

            // this.treeView1.ExpandAll();
        }

        public void ReloadVault()
        {
   VaultManager = ((MainForm) Parent.FindForm()).VaultManager;
            VaultManager.ReloadVaultConfig();
            var vaultInfo = VaultManager.GetVaultConfig();
            if (vaultInfo != null)
            {
    lblVaultLocation.Text = VaultManager.VaultFolderPath;
    lblAPIBaseURI.Text = vaultInfo.BaseUri;

                populateTreeView(vaultInfo);

    UpdateLogView ( VaultManager.GetActionLogSummary());

                //store setting for current vault path
                if (Properties.Settings.Default.VaultPath != VaultManager.VaultFolderPath)
                {
                    Properties.Settings.Default.VaultPath = VaultManager.VaultFolderPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void UpdateLogView( String logContent )
        {
            //TODO: update app log view
        }

        private void treeView1_MouseClick( Object sender, MouseEventArgs e)
        {
            // right click on treeview node
            if (e.Button == MouseButtons.Right)
            {
                // Point where the mouse is clicked.
                var p = new Point(e.X, e.Y);

                // Get the node that the user has clicked.
                var node = treeView1.GetNodeAt(p);

                if (node != null)
                {
                    if (node.Tag is IdentifierInfo)
                    {
                        treeView1.SelectedNode = node;
                        treeViewContextMenu.Show(treeView1, p);
                    }
                    else
                    {
                        treeView1.SelectedNode = node;
                        treeViewContextMenu.Show(treeView1, p);
                    }
                }
            }
        }

        private void treeView1_AfterSelect( Object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
    var selectedItem = treeView1.SelectedNode.Tag;
                if (selectedItem is RegistrationInfo)
                {
                    var i = (RegistrationInfo)selectedItem;
                    panelItemInfo.Controls.Clear();
                    var infoControl = new Forms.Controls.Details.RegistrationInfoDetails( GetParentMainForm ());
                    infoControl.Populate(i);
                    panelItemInfo.Controls.Add(infoControl);
                }

                if (selectedItem is CertificateInfo)
                {
                    var i = (CertificateInfo)selectedItem;
                    panelItemInfo.Controls.Clear();
                    var infoControl = new Forms.Controls.Details.CertificateDetails( GetParentMainForm ());
                    infoControl.Populate(i);
                    infoControl.Dock = DockStyle.Fill;
                    panelItemInfo.Controls.Add(infoControl);
                }

                if (selectedItem is IdentifierInfo)
                {
                    var i = (IdentifierInfo)selectedItem;
                    panelItemInfo.Controls.Clear();
                    var infoControl = new Forms.Controls.Details.SimpleDetails( GetParentMainForm ());
                    infoControl.Populate(i.Dns + " : " + i.Id);
                    panelItemInfo.Controls.Add(infoControl);
                }
            }
        }

        public Boolean DeleteVaultItem ( Object item )
        {
            if (item is RegistrationInfo)
            {
                var dialogResult = MessageBox.Show("Are you sure you wish to delete this item?", "Delete Vault Item", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
     var success = VaultManager.DeleteRegistrationInfo(((RegistrationInfo)item).Id);
                    return success;
                }
            }

            return false;
        }

        private void deleteToolStripMenuItem_Click( Object sender, EventArgs e)
        {
            // User has clicked delete in tree view context menu
            var node = treeView1.SelectedNode;

            if (node != null)
            {
                if (node.Tag is IdentifierInfo)
                {
                    var i = (IdentifierInfo)node.Tag;
                    VaultManager.CleanupVault(i.Id);
                    ReloadVault();
                    return;
                }
                else
                {
                    DeleteVaultItem(node.Tag);
                    ReloadVault();
                    return;
                }
            }
        }

        private void VaultExplorer_Load( Object sender, EventArgs e)
        {
            //this.ReloadVault();
            if (!DesignMode )
            {
    ReloadVault ();
            }
        }

        private void groupBoxVaultInfo_Enter( Object sender, EventArgs e)
        {
        }
    }
}