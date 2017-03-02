namespace ACMESharp.PKI.RSA {
 public class RsaPrivateKey : PrivateKey {
  public RsaPrivateKey ( System.Int32 bits, System.String e, System.String pem ) {
   Bits = bits;
   E = e;
   Pem = pem;
  }

  public System.Int32 Bits { get; private set; }

  public System.String E { get; private set; }

  public System.Object BigNumber { get; set; }

  public System.String Pem { get; private set; }
 }
}