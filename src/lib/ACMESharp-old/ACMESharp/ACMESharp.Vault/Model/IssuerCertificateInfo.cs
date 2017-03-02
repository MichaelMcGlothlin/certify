namespace ACMESharp.Vault.Model {
 public class IssuerCertificateInfo {
  public System.String SerialNumber { get; set; }

  public System.String Thumbprint { get; set; }

  public System.String Signature { get; set; }

  public System.String SignatureAlgorithm { get; set; }

  public System.String CrtPemFile { get; set; }

  public System.String CrtDerFile { get; set; }
 }
}