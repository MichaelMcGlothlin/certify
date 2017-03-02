using System;

namespace ACMESharp.Vault.Providers
{
 public class FileVaultAsset : VaultAsset
 {
  public FileVaultAsset(String path, String name, VaultAssetType type, System.Boolean isSensitive)
  {
   Path = path;
   Name = name;
   Type = type;
   IsSensitive = isSensitive;
  }

  public String Path { get; set; }
 }
}