using System;

namespace Certify.Models {
 public class ManagedSiteBinding {
  public String Hostname { get; set; }

  public Int32 Port { get; set; }

  /// <summary>
  /// IP is either * (all unassigned) or a specific IP
  /// </summary>
  public String IP { get; set; }

  public Boolean UseSNI { get; set; }

  public String CertName { get; set; }

  public PlannedActionType PlannedAction { get; set; }

  /// <summary>
  /// The primary domain is the main domain listed on the certificate
  /// </summary>
  public Boolean IsPrimaryCertificateDomain { get; set; }

  /// <summary>
  /// For SAN certificates, indicate if this name is an alternative name to be associated with a primary domain certificate
  /// </summary>
  public Boolean IsSubjectAlternativeName { get; set; }
 }
}