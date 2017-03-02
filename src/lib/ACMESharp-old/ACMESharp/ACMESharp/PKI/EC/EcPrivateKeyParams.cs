namespace ACMESharp.PKI.EC {
 /// <summary>
 /// Defines the parameters that may be provided as input to generate
 /// an <see cref="EcKeyPair"/>.
 /// </summary>
 public class EcPrivateKeyParams : PrivateKeyParams {
  /// <summary>
  /// The EC curve to use, using NIST curve names such as "P-256".
  /// </summary>
  public System.String CurveName { get; set; } = "P-256";

  public System.Boolean NamedCurveEncoding { get; set; }
 }
}