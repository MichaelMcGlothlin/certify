using ACMESharp.Ext;

namespace ACMESharp.PKI {
 public interface IPkiToolProviderInfo : IAliasesSupported {
  System.String Name { get; }

  System.String Label { get; }

  System.String Description { get; }
 }
}