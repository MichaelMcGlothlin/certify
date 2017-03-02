using ACMESharp.Vault.Model;
using ACMESharp.Vault.Providers;
using Certify.Management;

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Certify.Forms.Controls.Details {
 public partial class CertificateDetails : BaseDetailsControl, IDetailsControl<CertificateInfo> {
  private CertificateInfo item;

  public CertificateDetails ( MainForm parentApp ) {
   InitializeComponent ();
   this.parentApp = parentApp;
  }

  public void Populate ( CertificateInfo item ) {
   this.item = item;

   lblID.Text = item.Id.ToString ();
   lblAlias.Text = item.Alias;
   if ( item.CertificateRequest != null ) {
    var certManager = new CertificateManager ();
    var certPath = parentApp.VaultManager.GetCertificateFilePath ( item.Id );
    var crtDerFilePath = certPath + "\\" + item.CrtDerFile;
    lblFilePath.Text = crtDerFilePath;

    if ( File.Exists ( crtDerFilePath ) ) {
     var cert = certManager.GetCertificate ( crtDerFilePath );
     lblExpiryDate.Text = cert.GetExpirationDateString ();
     lblIssuer.Text = cert.Issuer;
     lblSubject.Text = cert.Subject;

     var expiryDate = DateTime.Parse ( cert.GetExpirationDateString () );
     var timeLeft = expiryDate - DateTime.Now;
     lblDaysRemaining.Text = timeLeft.Days.ToString ();
     if ( timeLeft.Days < 7 ) {
      lblDaysRemaining.ForeColor = Color.Red;
     } else {
      lblDaysRemaining.ForeColor = Color.Black;
     }
    } else {
     lblFilePath.Text = "[Not Found] " + lblFilePath.Text;
    }
   }
  }

  private void btnRenew_Click ( Object sender, EventArgs e ) {
   //attempt to renew and then re-export the selected certificate
   if ( item != null ) {
    Cursor = Cursors.WaitCursor;
    //update and create certificate
    //renew cert: parentApp.VaultManager.RenewCertificate(item.IdentifierRef)
    /*if ()
    {
        Populate(item); // update display with renewed info

        MessageBox.Show("Renewal requested. Check certificate info for expiry. Auto Apply to update IIS certificate");
    }
    else
    {
        MessageBox.Show("Could not process renewal.");
    }*/
    Cursor = Cursors.Default;
   }
  }

  private void button1_Click ( Object sender, EventArgs e ) {
   if ( item != null ) {
    parentApp.VaultManager.ExportCertificate ( "=" + item.Id, pfxOnly: true );
    MessageBox.Show ( "PFX file has been exported." );
   }
  }

  private void btnApply_Click ( Object sender, EventArgs e ) {
   //attempt to match iis site with cert domain, auto create mappinngs
   var ident = parentApp.VaultManager.GetIdentifier ( item.IdentifierRef.ToString () );
   if ( ident != null ) {
    var certFolderPath = parentApp.VaultManager.GetCertificateFilePath ( item.Id, LocalDiskVault.ASSET );
    var pfxFile = item.Id.ToString () + "-all.pfx";
    var pfxPath = Path.Combine ( certFolderPath, pfxFile );

    var iisManager = new IISManager ();
    if ( iisManager.InstallCertForDomain ( ident.Dns, pfxPath, cleanupCertStore: true, skipBindings: false ) ) {
     //all done
     MessageBox.Show ( "Certificate installed and SSL bindings updated for " + ident.Dns );
     return;
    }
   }

   MessageBox.Show ( "Could not match certificate identifier to site." );
  }
 }
}