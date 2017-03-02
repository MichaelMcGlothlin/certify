namespace ACMESharp.Messages {
 public class NewCertRequest : RequestMessage {
  public NewCertRequest () : base ( "new-cert" ) { }

  public System.String Csr { get; set; }
 }
}