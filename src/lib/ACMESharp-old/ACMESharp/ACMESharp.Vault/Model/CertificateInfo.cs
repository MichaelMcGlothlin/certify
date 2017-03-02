using System;

namespace ACMESharp.Vault.Model {
 public class CertificateInfo : IIdentifiable {
  public Guid Id { get; set; }

  public String Alias { get; set; }

  public String Label { get; set; }

  public String Memo { get; set; }

  public Guid IdentifierRef { get; set; }

  public String IdentifierDns { get; set; }

  public String[] AlternativeIdentifierDns { get; set; }

  public String KeyPemFile { get; set; }

  public String CsrPemFile { get; set; }

  public String GenerateDetailsFile { get; set; }

  public CertificateRequest CertificateRequest { get; set; }

  public String CrtPemFile { get; set; }

  public String CrtDerFile { get; set; }

  public String IssuerSerialNumber { get; set; }

  public String SerialNumber { get; set; }

  public String Thumbprint { get; set; }

  public String Signature { get; set; }

  public String SignatureAlgorithm { get; set; }
 }
}