using System;
using System.Net;
using System.Text;

namespace ACMESharp.Vault.Model {
 public class ProxyConfig {
  public Boolean UseNoProxy { get; set; }

  public String ProxyUri { get; set; }

  public Boolean UseDefCred { get; set; }

  public String Username { get; set; }

  public String PasswordEncoded { get; set; }

  /// <summary>
  /// Computes a <see cref="IWebProxy">web proxy</see> resolver instance
  /// based on the combination of proxy-related settings in this vault
  /// configuration.
  /// </summary>
  /// <returns></returns>
  public IWebProxy GetWebProxy () {
   IWebProxy wp = null;

   if ( UseNoProxy ) {
    wp = GlobalProxySelection.GetEmptyWebProxy ();
   } else if ( !String.IsNullOrEmpty ( ProxyUri ) ) {
    var newwp = new WebProxy ( ProxyUri );
    if ( UseDefCred ) {
     newwp.UseDefaultCredentials = true;
    } else if ( !String.IsNullOrEmpty ( Username ) ) {
     var pw = PasswordEncoded;
     if ( !String.IsNullOrEmpty ( pw ) ) {
      pw = Encoding.Unicode.GetString ( Convert.FromBase64String ( pw ) );
     }

     newwp.Credentials = new NetworkCredential ( Username, pw );
    }
   }

   return wp;
  }
 }
}