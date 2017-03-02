using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ACMESharp {
 public class AcmeRegistration {
  public IEnumerable<System.String> Contacts { get; set; }

  public System.Object PublicKey { get; set; }

  public System.Object RecoveryKey { get; set; }

  public System.String RegistrationUri { get; set; }

  public IEnumerable<System.String> Links { get; set; }

  public System.String TosLinkUri { get; set; }

  public System.String TosAgreementUri { get; set; }

  public System.String AuthorizationsUri { get; set; }

  public System.String CertificatesUri { get; set; }

  public void Save ( Stream s ) {
   using ( var w = new StreamWriter ( s ) ) {
    w.Write ( JsonConvert.SerializeObject ( this, Formatting.Indented ) );
   }
  }

  public static AcmeRegistration Load ( Stream s ) {
   using ( var r = new StreamReader ( s ) ) {
    return JsonConvert.DeserializeObject<AcmeRegistration> ( r.ReadToEnd () );
   }
  }
 }
}