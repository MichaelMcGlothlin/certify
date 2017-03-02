namespace ACMESharp.Vault {
 public enum VaultAssetType {
  Other = 0,

  /// <summary>
  /// A DnsInfo or WebServerInfo file to instantiate and
  /// configure a Provider for handling a Challenge.
  /// </summary>
  ProviderConfigInfo = 1,

  /// <summary>
  /// Stores intermediate details when generating a CSR.
  /// </summary>
  CsrDetails = 2,

  /// <summary>
  /// Imported or generated private key PEM file.
  /// </summary>
  KeyPem = 3,

  /// <summary>
  /// Imported or generated CSR PEM file.
  /// </summary>
  CsrPem = 4,

  /// <summary>
  /// Generated private key full details.
  /// </summary>
  KeyGen = 5,

  /// <summary>
  /// Generated CSR full details.
  /// </summary>
  CsrGen = 6,

  /// <summary>
  /// DER-encoded form of CSR (used directly in the ACME protocol).
  /// </summary>
  CsrDer = 7,

  /// <summary>
  /// DER-encoded form of the issued cert (returned from CA as per ACME spec).
  /// </summary>
  CrtDer = 8,

  /// <summary>
  /// PEM-encoded form of the issued cert.
  /// </summary>
  CrtPem = 9,

  IssuerDer = 10,

  IssuerPem = 11,

  /// <summary>
  /// An InstallerProfileInfofile to instantiate and
  /// configure a Provider for installing a certificate.
  /// </summary>
  InstallerConfigInfo = 12,
 }
}