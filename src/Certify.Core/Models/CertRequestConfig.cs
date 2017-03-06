namespace Certify.Models {
 public class CertRequestConfig {
  public System.String PrimaryDomain { get; set; }

  public System.String[] SubjectAlternativeNames { get; set; }

  public System.String WebsiteRootPath { get; set; }

  public System.Boolean PerformChallengeFileCopy { get; set; }

  public System.Boolean PerformExtensionlessConfigChecks { get; set; }

  public System.Boolean PerformExtensionlessAutoConfig { get; set; }

  public System.Boolean PerformAutomatedCertBinding { get; set; }

  public System.Boolean EnableFailureNotifications { get; set; }

  public System.String ChallengeType { get; set; }
 }
}