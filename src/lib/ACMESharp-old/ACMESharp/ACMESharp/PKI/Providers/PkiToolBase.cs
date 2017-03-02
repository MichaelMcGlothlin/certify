using Newtonsoft.Json;
using System.IO;

namespace ACMESharp.PKI.Providers {
 public abstract class PkiToolBase //: IPkiTool
 {
  /// <summary>
  /// Default implementation of saving a private key serializes as a JSON object.
  /// </summary>
  public virtual void SavePrivateKey ( PrivateKey pk, Stream target ) {
   using ( var w = new StreamWriter ( target ) ) {
    w.Write ( JsonConvert.SerializeObject ( pk ) );
   }
  }

  /// <summary>
  /// Default implementation of loading a JSON-serialized private key.
  /// </summary>
  public virtual PrivateKey LoadPrivateKey ( Stream source ) {
   using ( var r = new StreamReader ( source ) ) {
    //return JsonConvert.DeserializeObject<RsaPrivateKey>(r.ReadToEnd());
    return LoadPrivateKey ( r.ReadToEnd () );
   }
  }

  public abstract PrivateKey LoadPrivateKey ( System.String ser );

  /// <summary>
  /// Default implementation of saving a private key serializes as a JSON object.
  /// </summary>
  public virtual void SaveCsrParams ( CsrParams csrParams, Stream target ) {
   using ( var w = new StreamWriter ( target ) ) {
    w.Write ( JsonConvert.SerializeObject ( csrParams ) );
   }
  }

  public virtual CsrParams LoadCsrParams ( Stream source ) {
   using ( var r = new StreamReader ( source ) ) {
    return JsonConvert.DeserializeObject<CsrParams> ( r.ReadToEnd () );
   }
  }

  /// <summary>
  /// Default implementation of saving a private key serializes as a JSON object.
  /// </summary>
  public virtual void SaveCsr ( Csr csr, Stream target ) {
   using ( var w = new StreamWriter ( target ) ) {
    w.Write ( JsonConvert.SerializeObject ( csr ) );
   }
  }

  /// <summary>
  /// Default implementation of loading a JSON-serialized private key.
  /// </summary>
  public virtual Csr LoadCsr ( Stream source ) {
   using ( var r = new StreamReader ( source ) ) {
    return JsonConvert.DeserializeObject<Csr> ( r.ReadToEnd () );
   }
  }
 }
}