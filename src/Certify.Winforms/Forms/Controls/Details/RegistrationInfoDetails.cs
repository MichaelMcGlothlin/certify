using ACMESharp.Vault.Model;
using System;

namespace Certify.Forms.Controls.Details {
 public partial class RegistrationInfoDetails : BaseDetailsControl, IDetailsControl<RegistrationInfo> {
  private RegistrationInfo registrationInfo;

  public RegistrationInfoDetails ( MainForm parentApp ) {
   InitializeComponent ();
   this.parentApp = parentApp;
  }

  public void Populate ( RegistrationInfo item ) {
   registrationInfo = item;

   lblID.Text = item.Id.ToString ();
   lblAlias.Text = item.Alias;
   lblLabel.Text = item.Label;
   lblMemo.Text = item.Memo;
   lblSignerProvider.Text = item.SignerProvider;
   lblContacts.Text = "";
   if ( item.Registration?.Contacts != null ) {
    foreach ( var c in item.Registration.Contacts ) {
     lblContacts.Text += c;
    }
   }

   lnkUri.Text = item.Registration.RegistrationUri;
  }

  private void btnDelete_Click ( Object sender, EventArgs e ) {
   if ( registrationInfo != null ) {
    var success = parentApp.DeleteVaultItem ( registrationInfo );
    if ( success ) {
     Hide ();
    }
   }
  }
 }
}