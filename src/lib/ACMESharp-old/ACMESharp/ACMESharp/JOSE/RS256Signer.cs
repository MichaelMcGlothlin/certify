using System.IO;
using System.Security.Cryptography;

namespace ACMESharp.JOSE {
 public class RS256Signer : ISigner {
  private RSACryptoServiceProvider _rsa;
  private SHA256CryptoServiceProvider _sha;
  private System.Object _jwk;

  public System.String JwsAlg => "RS256";

  public System.Int32 KeySize { get; set; } = 2048;

  public void Init () {
   _rsa = new RSACryptoServiceProvider ( KeySize );
   _sha = new SHA256CryptoServiceProvider ();
  }

  public void Dispose () {
   if ( _rsa != null ) {
    _rsa.Dispose ();
   }

   _rsa = null;
  }

  public void Save ( Stream stream ) {
   using ( var w = new StreamWriter ( stream ) ) {
    w.Write ( _rsa.ToXmlString ( true ) );
   }
  }

  public void Load ( Stream stream ) {
   using ( var r = new StreamReader ( stream ) ) {
    _rsa.FromXmlString ( r.ReadToEnd () );
   }
  }

  /// <summary>
  ///
  /// </summary>
  /// <param name="canonical"></param>
  /// <returns></returns>
  public System.Object ExportJwk ( System.Boolean canonical = false ) {
   // Note, we only produce a canonical form of the JWK
   // for export therefore we ignore the canonical param

   if ( _jwk == null ) {
    var keyParams = _rsa.ExportParameters ( false );
    _jwk = new {
     // As per RFC 7638 Section 3, these are the *required* elements of the
     // JWK and are sorted in lexicographic order to produce a canonical form
     e = JwsHelper.Base64UrlEncode ( keyParams.Exponent ),
     kty = "RSA",
     n = JwsHelper.Base64UrlEncode ( keyParams.Modulus ),
    };
   }

   return _jwk;
  }

  public System.Byte[] Sign ( System.Byte[] raw ) => _rsa.SignData ( raw, _sha );
 }
}