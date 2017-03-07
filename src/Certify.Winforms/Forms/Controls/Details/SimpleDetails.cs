using System;

namespace Certify.Forms.Controls.Details {
 public partial class SimpleDetails : BaseDetailsControl, IDetailsControl<String> {
  public SimpleDetails ( MainForm parentApp ) {
   InitializeComponent ();
   this.parentApp = parentApp;
  }

  public void Populate ( String item ) => lblDetails.Text = item;
 }
}