using ACMESharp.Ext;

namespace ACMESharp.Installer {
 public interface IInstallerProviderInfo : IAliasesSupported {
  System.String Name { get; }

  System.String Label { get; }

  System.String Description { get; }

  System.Boolean IsUninstallSupported { get; }
 }
}