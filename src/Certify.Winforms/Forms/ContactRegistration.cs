﻿using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Certify.Forms {
 public partial class ContactRegistration : Form {
  private VaultManager vaultManager;

  public ContactRegistration () {
   InitializeComponent ();
   btnCreateContact.Enabled = false;
   txtContacts.Select (); // set focus on text box
  }

  public ContactRegistration ( VaultManager vaultManager ) : this () => this.vaultManager = vaultManager;

  private void btnCreateContact_Click ( Object sender, EventArgs e ) {
   var isValidEmail = true;
   if ( String.IsNullOrEmpty ( txtContacts.Text ) ) {
    isValidEmail = false;
   } else {
    if ( !Regex.IsMatch ( txtContacts.Text,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds ( 250 ) ) ) {
     isValidEmail = false;
    }
   }

   if ( !isValidEmail ) {
    MessageBox.Show ( "Ooops, you forgot to provide a valid email address." );
    DialogResult = DialogResult.None;
    return;
   }

   if ( chkAgreeTandCs.Checked ) {
    if ( vaultManager != null ) {
     var vaultConfig = vaultManager.GetVaultConfig ();
     if ( vaultConfig != null ) {
      //TODO: check for dupe registration
     }

     btnCreateContact.Enabled = false;
     Cursor = Cursors.WaitCursor;
     vaultManager.AddNewRegistration ( "mailto:" + txtContacts.Text );
     Cursor = Cursors.Default;
    }
   } else {
    MessageBox.Show ( "You need to agree to the latest LetsEncrypt.org Subscriber Agreement." );
    DialogResult = DialogResult.None;
   }
  }

  private void label1_Click ( Object sender, EventArgs e ) {
  }

  private void chkAgreeTandCs_CheckedChanged ( Object sender, EventArgs e ) => btnCreateContact.Enabled = chkAgreeTandCs.Checked;
 }
}