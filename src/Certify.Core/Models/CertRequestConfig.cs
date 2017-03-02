namespace Certify.Models {
 public class CertRequestConfig {
  public System.String Domain { get; set; }

  public System.String WebsiteRootPath { get; set; }

  public System.Boolean PerformChallengeFileCopy { get; set; }

  public System.Boolean PerformExtensionlessConfigChecks { get; set; }

  public System.Boolean PerformExtensionlessAutoConfig { get; set; }
 }
}