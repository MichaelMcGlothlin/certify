using System.Collections.Generic;

namespace Certify.Models {
 public class ExtendedVaultConfig {
  public List<VaultConfigItem> ConfigItems { get; set; }
 }

 public class VaultConfigItem {
  public System.String ItemType { get; set; }

  public System.String ItemValue { get; set; }
 }
}