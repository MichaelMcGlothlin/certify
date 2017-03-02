namespace ACMESharp.ACME {
 public interface IChallengeDecoderProviderInfo {
  System.String Type { get; }

  ChallengeTypeKind SupportedType { get; }

  System.String Label { get; }

  System.String Description { get; }
 }
}