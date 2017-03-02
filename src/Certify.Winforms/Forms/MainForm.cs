using ACMESharp.Vault.Model;
using ACMESharp.Vault.Providers;
using Certify.Forms;
using Certify.Management;
using Microsoft.ApplicationInsights;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Certify {
 internal enum ImageList {
  Vault = 0,
  Globe = 1,
  Cert = 2,
  Person = 3
 }

 public partial class MainForm : Form {
  internal VaultManager VaultManager = null;
  private TelemetryClient tc = null;

  public MainForm () {
   InitializeComponent ();

   Text = Properties.Resources.LongAppName;
   if ( Properties.Settings.Default.CheckForUpdatesAtStartup ) {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    PerformCheckForUpdates ( silent: true );
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
   }
  }

  private void InitAI () {
   if ( Properties.Settings.Default.EnableAppTelematics ) {
    tc = new TelemetryClient ();
    tc.Context.InstrumentationKey = Properties.Resources.AIInstrumentationKey;
    tc.InstrumentationKey = Properties.Resources.AIInstrumentationKey;

    // Set session data:

    tc.Context.Session.Id = Guid.NewGuid ().ToString ();
    tc.Context.Device.OperatingSystem = Environment.OSVersion.ToString ();
   } else {
    tc = null;
   }
  }

  internal void TrackPageView ( String pageName ) => tc?.TrackPageView ( pageName );

  private void fileToolStripMenuItem_Click ( Object sender, EventArgs e ) => Application.Exit ();

  private void populateTreeView ( VaultInfo vaultConfig ) {
   if ( treeView1.Nodes != null ) {
    treeView1.Nodes.Clear ();
   }

   var certManager = new CertificateManager ();
   // start off by adding a base treeview node
   var mainNode = new TreeNode () {
    Name = "Vault",
    Text = "Vault",
    ImageIndex = (Int32) ImageList.Vault
   };
   mainNode.SelectedImageIndex = mainNode.ImageIndex;

   treeView1.Nodes.Add ( mainNode );

   if ( vaultConfig.Identifiers != null ) {
    var domainsNode = new TreeNode ( "Domains & Certificates (" + vaultConfig.Identifiers.Count + ")" ) {
     ImageIndex = (Int32) ImageList.Globe
    };
    domainsNode.SelectedImageIndex = domainsNode.ImageIndex;

    foreach ( var i in vaultConfig.Identifiers ) {
     var node = new TreeNode ( i.Dns ) {
      Tag = i,

      ImageIndex = (Int32) ImageList.Globe
     };
     node.SelectedImageIndex = node.ImageIndex;

     if ( vaultConfig.Certificates != null ) {
      foreach ( var c in vaultConfig.Certificates ) {
       if ( c.IdentifierRef == i.Id ) {
        //add cert
        var certNode = new TreeNode ( c.Alias ) {
         Tag = c,

         ImageIndex = (Int32) ImageList.Cert
        };
        certNode.SelectedImageIndex = certNode.ImageIndex;

        //get info from get if possible, use that to style the parent node (expiry warning)

        var certPath = VaultManager.GetCertificateFilePath ( c.Id );
        var crtDerFilePath = certPath + "\\" + c.CrtDerFile;

        if ( File.Exists ( crtDerFilePath ) ) {
         var cert = certManager.GetCertificate ( crtDerFilePath );

         var expiryDate = DateTime.Parse ( cert.GetExpirationDateString () );
         var timeLeft = expiryDate - DateTime.Now;
         node.Text += " (" + timeLeft.Days + " days remaining)";
         if ( timeLeft.Days < 30 ) {
          node.ForeColor = Color.Orange;
         }
         if ( timeLeft.Days < 7 ) {
          node.ForeColor = Color.Red;
         }
        } else {
         node.ForeColor = Color.Gray;
        }
        node.Nodes.Add ( certNode );
       }
      }
     }
     domainsNode.Nodes.Add ( node );
    }

    mainNode.Nodes.Add ( domainsNode );
    domainsNode.Expand ();
   }

   if ( vaultConfig.Registrations != null ) {
    var contactsNode = new TreeNode ( "Registered Contacts (" + vaultConfig.Registrations.Count + ")" ) {
     ImageIndex = (Int32) ImageList.Person
    };
    contactsNode.SelectedImageIndex = contactsNode.ImageIndex;

    foreach ( var i in vaultConfig.Registrations ) {
     var title = i.Registration.Contacts.FirstOrDefault ();
     var node = new TreeNode ( title ) {
      Tag = i,

      ImageIndex = (Int32) ImageList.Person
     };
     node.SelectedImageIndex = node.ImageIndex;

     contactsNode.Nodes.Add ( node );
    }

    mainNode.Nodes.Add ( contactsNode );

    contactsNode.Expand ();
   }

   if ( mainNode.Nodes.Count == 0 ) {
    mainNode.Nodes.Add ( "(Empty)" );
   } else {
    mainNode.Expand ();
   }

   // this.treeView1.ExpandAll();
  }

  private void ReloadVault () {
   VaultManager.ReloadVaultConfig ();
   var vaultInfo = VaultManager.GetVaultConfig ();
   if ( vaultInfo != null ) {
    lblVaultLocation.Text = VaultManager.VaultFolderPath;
    lblAPIBaseURI.Text = vaultInfo.BaseUri;

    populateTreeView ( vaultInfo );

    txtOutput.Text = VaultManager.GetActionLogSummary ();

    //store setting for current vault path
    if ( Properties.Settings.Default.VaultPath != VaultManager.VaultFolderPath ) {
     Properties.Settings.Default.VaultPath = VaultManager.VaultFolderPath;
     Properties.Settings.Default.Save ();
    }
   }
  }

  private void ShowCertificateRequestDialog () {
   try {
    using ( var form = new CertRequestDialog ( VaultManager ) ) {
     form.ShowDialog ();
    }
   } catch ( ObjectDisposedException ) {
   }
   ReloadVault ();
  }

  private void ShowSettingsDialog () {
   try {
    using ( var form = new Certify.Forms.Settings () ) {
     var result = form.ShowDialog ();
     if ( result == DialogResult.OK ) {
      form.SaveSettings ();
     }
    }
   } catch ( ObjectDisposedException ) {
   }
  }

  private void ShowContactRegistrationDialog () {
   using ( var form = new ContactRegistration ( VaultManager ) ) {
    var result = form.ShowDialog ();
   }
   ReloadVault ();
  }

  private void MainForm_Shown ( Object sender, EventArgs e ) {
   InitAI ();
   TrackPageView ( nameof ( MainForm ) );

   var powershellVersion = PowershellManager.GetPowershellVersion ();
   if ( powershellVersion < 4 ) {
    MessageBox.Show ( "This application requires PowerShell version 4.0 or higher. You can update it using the latest Windows Management Framework download from Microsoft.", Properties.Resources.AppName );

    Application.Exit ();
    return;
   }

   VaultManager = new VaultManager ( Properties.Settings.Default.VaultPath, LocalDiskVault.VAULT );

   var manager = VaultManager.PowershellManager;

   /*if (!manager.IsAcmeSharpModuleInstalled())
   {
       if (MessageBox.Show("The required PowerShell module 'ACMESharp' cannot be found. Please see https://www.powershellgallery.com/packages/ACMESharp/ or install from PowerShell command line as an administrator using: 'Install-Module -Name ACMESharp'",
               "ACMESharp Missing", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
       {
           // Application.Exit();
       }
   }*/

   if ( Properties.Settings.Default.ShowBetaWarning ) {
    lblGettingStarted.Text += "\r\n\r\n" + Properties.Resources.BetaWarning;
   }

   var vaultInfo = VaultManager.GetVaultConfig ();

   if ( vaultInfo?.Registrations == null ) {
    //got an existing vault. If no contact registrations setup, prompt to add one
    var promptResult = MessageBox.Show ( "No certificate contact registrations have been setup. Would you like to add a new contact now? ", "Create New Contact?", MessageBoxButtons.YesNo );

    if ( promptResult == DialogResult.Yes ) {
     ShowContactRegistrationDialog ();
    }
   }
   ReloadVault ();
  }

  private void treeView1_AfterSelect ( Object sender, TreeViewEventArgs e ) {
   if ( treeView1.SelectedNode != null ) {
    var selectedItem = treeView1.SelectedNode.Tag;
    if ( selectedItem is RegistrationInfo ) {
     var i = (RegistrationInfo) selectedItem;
     panelItemInfo.Controls.Clear ();
     var infoControl = new Forms.Controls.Details.RegistrationInfoDetails ( this );
     infoControl.Populate ( i );
     panelItemInfo.Controls.Add ( infoControl );
    }

    if ( selectedItem is CertificateInfo ) {
     var i = (CertificateInfo) selectedItem;
     panelItemInfo.Controls.Clear ();
     var infoControl = new Forms.Controls.Details.CertificateDetails ( this );
     infoControl.Populate ( i );
     infoControl.Dock = DockStyle.Fill;
     panelItemInfo.Controls.Add ( infoControl );
    }

    if ( selectedItem is IdentifierInfo ) {
     var i = (IdentifierInfo) selectedItem;
     panelItemInfo.Controls.Clear ();
     var infoControl = new Forms.Controls.Details.SimpleDetails ( this );
     infoControl.Populate ( i.Dns + " : " + i.Id );
     panelItemInfo.Controls.Add ( infoControl );
    }
   }
  }

  public Boolean DeleteVaultItem ( Object item ) {
   if ( item is RegistrationInfo ) {
    var dialogResult = MessageBox.Show ( "Are you sure you wish to delete this item?", "Delete Vault Item", MessageBoxButtons.YesNo );
    if ( dialogResult == DialogResult.Yes ) {
     var success = VaultManager.DeleteRegistrationInfo ( ( (RegistrationInfo) item ).Id.ToString () );
     return success;
    }
   }

   return false;
  }

  private void reloadVaultToolStripMenuItem_Click ( Object sender, EventArgs e ) => ReloadVault ();

  private void aboutToolStripMenuItem_Click ( Object sender, EventArgs e ) {
   using ( var aboutDialog = new AboutDialog () ) {
    aboutDialog.ShowDialog ();
   }
  }

  private async void checkForUpdatesToolStripMenuItem_Click ( Object sender, EventArgs e ) => await PerformCheckForUpdates ( silent: false ).ConfigureAwait ( false );

  private async Task<Boolean> PerformCheckForUpdates ( Boolean silent = false ) {
   var updateCheck = await new Util ().CheckForUpdates ( Application.ProductVersion ).ConfigureAwait ( false );

   if ( updateCheck != null ) {
    if ( updateCheck.IsNewerVersion ) {
     var gotoDownload = MessageBox.Show ( updateCheck.Message.Body + "\r\nVisit download page now?", Properties.Resources.AppName, MessageBoxButtons.YesNo );
     if ( gotoDownload == DialogResult.Yes ) {
      var sInfo = new ProcessStartInfo ( Properties.Resources.AppWebsiteURL );
      Process.Start ( sInfo );
     }
    } else {
     if ( !silent ) {
      MessageBox.Show ( Properties.Resources.UpdateCheckLatestVersion, Properties.Resources.AppName );
     }
    }
   }
   return true;
  }

  private void changeVaultToolStripMenuItem_Click ( Object sender, EventArgs e ) => LocateOrCreateVault ( false );

  private Boolean LocateOrCreateVault ( Boolean useDefaultCreationPath = true ) {
   var promptResult = MessageBox.Show ( "Do you want to create a new vault? Choose 'No' to browse to an existing vault folder.", "Change Vault", MessageBoxButtons.YesNoCancel );

   if ( promptResult == DialogResult.Yes ) {
    var useProductionPrompt = MessageBox.Show ( "Do you want to use the live LetsEncrypt.org API? Choose 'No' to use the staging (test) API for this vault.", Properties.Resources.AppName, MessageBoxButtons.YesNo );

    var useStagingAPI = false;
    if ( useProductionPrompt == DialogResult.No ) {
     useStagingAPI = true;
    }

    var useDefaultPath = MessageBox.Show ( "Do you want to use the default vault path of " + Properties.Settings.Default.DefaultVaultPath + "?", Properties.Resources.AppName, MessageBoxButtons.YesNo );
    if ( useDefaultPath == DialogResult.Yes ) {
     useDefaultCreationPath = true;
    }

    var newVaultPath = Properties.Settings.Default.DefaultVaultPath;
    if ( !useDefaultCreationPath ) {
     //browse to a follder to store vault in
     var d = new FolderBrowserDialog ();
     var dialogResult = d.ShowDialog ();
     if ( dialogResult == DialogResult.OK ) {
      newVaultPath = d.SelectedPath;
     } else {
      return false;
     }
    }

    if ( Directory.Exists ( newVaultPath ) && Directory.GetFiles ( newVaultPath ).Any () ) {
     MessageBox.Show ( "You need to create the vault in a new empty folder. The specified folder is not empty." );
     return false;
    }

    if ( VaultManager.InitVault ( useStagingAPI ) ) {
     //vault created

     ReloadVault ();
     return true;
    }
   }

   if ( promptResult == DialogResult.No ) {
    //folder picker browse to vault folder
    var d = new FolderBrowserDialog ();
    var dialogResult = d.ShowDialog ();
    if ( dialogResult == DialogResult.OK ) {
     if ( VaultManager.IsValidVaultPath ( d.SelectedPath ) ) {
      VaultManager = new VaultManager ( d.SelectedPath, LocalDiskVault.VAULT );
      ReloadVault ();
      return true;
     } else {
      MessageBox.Show ( "The selected folder is not a valid vault." );
      return false;
     }
    }
   }

   return false;
  }

  private void toolStripButtonNewContact_Click ( Object sender, EventArgs e ) => ShowContactRegistrationDialog ();

  private void contactRegistrationToolStripMenuItem_Click ( Object sender, EventArgs e ) => ShowContactRegistrationDialog ();

  private void toolStripButtonNewCertificate_Click ( Object sender, EventArgs e ) => ShowCertificateRequestDialog ();

  private void domainCertificateToolStripMenuItem_Click ( Object sender, EventArgs e ) => ShowCertificateRequestDialog ();

  private void websiteToolStripMenuItem_Click ( Object sender, EventArgs e ) {
   var sInfo = new ProcessStartInfo ( Properties.Resources.AppWebsiteURL );
   Process.Start ( sInfo );
  }

  private void settingsToolStripMenuItem_Click ( Object sender, EventArgs e ) => ShowSettingsDialog ();

  private void MainForm_FormClosing ( Object sender, FormClosingEventArgs e ) {
   Cursor.Current = Cursors.WaitCursor;
   if ( tc != null ) {
    tc.Flush (); // only for desktop apps

    // Allow time for flushing:
    System.Threading.Thread.Sleep ( 1000 );
   }
   base.OnClosing ( e );
  }

  private void cleanupVaultToolStripMenuItem_Click ( Object sender, EventArgs e ) {
   VaultManager.CleanupVault ();
   ReloadVault ();
  }

  private void deleteToolStripMenuItem_Click ( Object sender, EventArgs e ) {
   // User has clicked delete in tree view context menu
   var node = treeView1.SelectedNode;

   if ( node != null ) {
    if ( node.Tag is IdentifierInfo i ) {
     VaultManager.CleanupVault ( i.Id );
     ReloadVault ();
     return;
    } else {
     DeleteVaultItem ( node.Tag );
     ReloadVault ();
     return;
    }
   }
  }

  private void treeView1_MouseUp ( Object sender, MouseEventArgs e ) {
   // right click on treeview node
   if ( e.Button == MouseButtons.Right ) {
    // Point where the mouse is clicked.
    var p = new Point ( e.X, e.Y );

    // Get the node that the user has clicked.
    var node = treeView1.GetNodeAt ( p );

    if ( node != null ) {
     if ( node.Tag is IdentifierInfo ) {
      treeView1.SelectedNode = node;
      treeViewContextMenu.Show ( treeView1, p );
     } else {
      treeView1.SelectedNode = node;
      treeViewContextMenu.Show ( treeView1, p );
     }
    }
   }
  }

  private void MainForm_Load ( Object sender, EventArgs e ) {
  }
 }
}