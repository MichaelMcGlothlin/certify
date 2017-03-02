using ACMESharp.Util;
using System.Collections.Generic;
using System.Linq;

namespace ACMESharp.DNS {
 public class ManualDnsProvider : BaseManualProvider, IXXXDnsProvider {
  public void EditTxtRecord ( System.String dnsName, IEnumerable<System.String> dnsValues ) => WriteRecord ( "TXT", dnsName, dnsValues.ToArray () );

  public void EditARecord ( System.String dnsName, System.String dnsValue ) => WriteRecord ( "A", dnsName, dnsValue );

  public void EditCnameRecord ( System.String dnsName, System.String dnsValue ) => WriteRecord ( "CNAME", dnsName, dnsValue );

  private void WriteRecord ( System.String dnsType, System.String dnsName, params System.String[] dnsValues ) {
   _writer.WriteLine ( "Manually Configure DNS Resource Record:" );
   _writer.WriteLine ( "  *   Type:  [{0}]", dnsType );
   _writer.WriteLine ( "  *   Name:  [{0}]", dnsName );

   if ( dnsValues == null || dnsValues.Length == 0 ) {
    _writer.WriteLine ( "  *  Value:  (N/A)" );
   } else {
    foreach ( var v in dnsValues ) {
     _writer.WriteLine ( "  *  Value:  [{0}]", v );
    }
   }
  }
 }
}