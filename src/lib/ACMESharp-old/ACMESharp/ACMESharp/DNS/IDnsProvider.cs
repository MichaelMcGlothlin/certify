using System.Collections.Generic;

namespace ACMESharp.DNS {
 public interface IXXXDnsProvider {
  void EditTxtRecord ( System.String dnsName, IEnumerable<System.String> dnsValues );

  void EditARecord ( System.String dnsName, System.String dnsValue );

  void EditCnameRecord ( System.String dnsName, System.String dnsValue );
 }
}