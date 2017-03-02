using System.Diagnostics;
using System.Windows.Forms;

namespace Certify.Forms {
 public partial class AboutDialog : Form {
  public AboutDialog () {
   InitializeComponent ();
   lblAppName.Text = Properties.Resources.LongAppName;
   lblAppVersion.Text = ProductVersion + " - " + Properties.Resources.ReleaseDate;
   lnkPublisherWebsite.Text = Properties.Resources.AppWebsiteURL;
   txtCredits.Text = Properties.Resources.Credits;
  }

  private void lnkPublisherWebsite_LinkClicked ( System.Object sender, LinkLabelLinkClickedEventArgs e ) {
   var sInfo = new ProcessStartInfo ( Properties.Resources.AppWebsiteURL );
   Process.Start ( sInfo );
  }
 }
}