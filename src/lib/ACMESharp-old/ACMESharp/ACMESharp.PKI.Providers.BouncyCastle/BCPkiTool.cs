using System.Collections.Generic;

namespace ACMESharp.PKI.Providers {
 public class BCPkiTool : BouncyCastleProvider, IPkiTool {
  public BCPkiTool ( IReadOnlyDictionary<System.String, System.String> newParams )
      : base ( newParams ) { }
 }
}