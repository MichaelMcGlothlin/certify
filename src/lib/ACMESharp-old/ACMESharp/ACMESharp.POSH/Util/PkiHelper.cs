using ACMESharp.PKI;

namespace ACMESharp.POSH.Util {
 public static class PkiHelper {
  public static IPkiTool GetPkiTool ( System.String name ) => System.String.IsNullOrEmpty ( name )
       //? CertificateProvider.GetProvider()
       //: CertificateProvider.GetProvider(name);
       ? PkiToolExtManager.GetPkiTool ()
       : PkiToolExtManager.GetPkiTool ( name );
 }
}