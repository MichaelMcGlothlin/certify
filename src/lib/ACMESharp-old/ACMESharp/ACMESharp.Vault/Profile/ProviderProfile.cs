using System.Collections.Generic;

namespace ACMESharp.Vault.Profile {
 public enum ProviderType {
  CUSTOM = 0,

  VAULT = 1,
  CHALLENGE_DECODER = 2,
  CHALLENGE_HANDLER = 3,
  PKI = 4,
  INSTALLER = 5,
 }

 public class ProviderProfile {
  public ProviderType ProviderType { get; set; }

  public System.String ProviderCustomType { get; set; }

  public System.String ProviderName { get; set; }

  public IReadOnlyDictionary<System.String, System.Object> ProviderParameters { get; set; }

  public IReadOnlyDictionary<System.String, System.Object> InstanceParameters { get; set; }

  public IReadOnlyDictionary<System.String, System.Object> ProfileParameters { get; set; }
 }
}