namespace ACMESharp.PKI.RSA {
 /// <summary>
 /// Defines the parameters that may be provided as input to generate
 /// an <see cref="RsaPrivateKey"/>.
 /// </summary>
 public class RsaPrivateKeyParams : PrivateKeyParams {
  public delegate System.Int32 RsaKeyGeneratorCallback ( System.Int32 p, System.Int32 n, System.Object cbArg );

  /// <summary>
  /// The number of bits in the generated key. If not specified 2048 is used.
  /// </summary>
  public System.Int32 NumBits { get; set; }

  /// <summary>
  /// The RSA public exponent value. This can be a large decimal or hexadecimal value
  /// if preceded by 0x.  Default value is 65537.
  /// </summary>
  public System.String PubExp { get; set; }

  public RsaKeyGeneratorCallback Callback { get; set; }

  public System.Object CallbackArg { get; set; }
 }
}