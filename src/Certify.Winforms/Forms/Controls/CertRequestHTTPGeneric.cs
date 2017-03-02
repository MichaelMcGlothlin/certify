using System;
using System.Windows.Forms;

namespace Certify.Forms.Controls {
 public partial class CertRequestHTTPGeneric : CertRequestBaseControl {
  private Int32 wizardStep = 1;

  public CertRequestHTTPGeneric () => InitializeComponent ();

  private void tabPage1_Click ( Object sender, EventArgs e ) {
  }

  private Boolean IsStepValid () {
   if ( wizardStep == 1 ) {
    if ( String.IsNullOrEmpty ( txtDomain.Text ) ) {
     return false;
    }
   }
   return true;
  }

  private void btnNext_Click ( Object sender, EventArgs e ) {
   if ( IsStepValid () ) {
    wizardStep++;
   } else {
    MessageBox.Show ( "Invalid settings. Please check before proceeding." );
   }
  }
 }
}