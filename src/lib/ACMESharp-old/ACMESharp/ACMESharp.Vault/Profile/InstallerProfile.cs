using System.Collections.Generic;

namespace ACMESharp.Vault.Profile {
 public class InstallerProfile {
  public System.String InstallerProvider { get; set; }

  public IReadOnlyDictionary<System.String, System.Object> InstanceParameters { get; set; }
 }
}