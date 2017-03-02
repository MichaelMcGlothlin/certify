using System.Collections.Generic;

namespace ACMESharp.PKI {
 public class CsrDetails {
  /// <summary>X509 'CN'</summary>
  public System.String CommonName { get; set; }

  // <summary>X509 SAN extension</summary>
  public IEnumerable<System.String> AlternativeNames { get; set; }

  /// <summary>X509 'C'</summary>
  public System.String Country { get; set; }

  /// <summary>X509 'ST'</summary>
  public System.String StateOrProvince { get; set; }

  /// <summary>X509 'L'</summary>
  public System.String Locality { get; set; }

  /// <summary>X509 'O'</summary>
  public System.String Organization { get; set; }

  /// <summary>X509 'OU'</summary>
  public System.String OrganizationUnit { get; set; }

  /// <summary>X509 'D'</summary>
  public System.String Description { get; set; }

  /// <summary>X509 'S'</summary>
  public System.String Surname { get; set; }

  /// <summary>X509 'G'</summary>
  public System.String GivenName { get; set; }

  /// <summary>X509 'I'</summary>
  public System.String Initials { get; set; }

  /// <summary>X509 'T'</summary>
  public System.String Title { get; set; }

  /// <summary>X509 'SN'</summary>
  public System.String SerialNumber { get; set; }

  /// <summary>X509 'UID'</summary>
  public System.String UniqueIdentifier { get; set; }

  /// <summary>X509 'emailAddress'</summary>
  public System.String Email { get; set; }
 }
}