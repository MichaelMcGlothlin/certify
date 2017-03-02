namespace ACMESharp.Vault {
 public class VaultAsset {
  public virtual System.String Name { get; protected set; }

  public virtual VaultAssetType Type { get; protected set; }

  public virtual System.Boolean IsSensitive { get; protected set; }
 }
}