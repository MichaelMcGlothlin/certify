using ACMESharp.Ext;

namespace ACMESharp.ACME {
 public interface IChallengeHandlerProviderInfo : IAliasesSupported {
  System.String Name { get; }

  ChallengeTypeKind SupportedTypes { get; }

  System.String Label { get; }

  System.String Description { get; }
 }
}