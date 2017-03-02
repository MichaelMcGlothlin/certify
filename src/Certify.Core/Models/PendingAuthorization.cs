using ACMESharp;
using ACMESharp.Vault.Model;

namespace Certify.Models {
 public class PendingAuthorization {
  public AuthorizeChallenge Challenge { get; set; }

  public IdentifierInfo Identifier { get; set; }

  public System.String TempFilePath { get; set; }

  public System.Boolean ExtensionlessConfigCheckedOK { get; set; }
 }
}