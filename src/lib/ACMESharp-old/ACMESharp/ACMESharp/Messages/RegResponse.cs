using System.Collections.Generic;

namespace ACMESharp.Messages {
 public class RegResponse {
  public System.Object Key { get; set; }

  public IEnumerable<System.String> Contact { get; set; }

  public System.String Agreement { get; set; }

  public System.String Authorizations { get; set; }

  public System.String Certificates { get; set; }

  public System.Object RecoveryKey { get; set; }
 }
}