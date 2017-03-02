using ACMESharp.Ext;

namespace ACMESharp.Vault {
 public interface IVaultProviderInfo : IAliasesSupported {
  System.String Name { get; }

  System.String Label { get; }

  System.String Description { get; }
 }
}