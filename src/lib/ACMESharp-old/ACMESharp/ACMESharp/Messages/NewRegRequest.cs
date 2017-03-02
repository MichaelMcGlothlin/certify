using System.Collections.Generic;

namespace ACMESharp.Messages {
 public class NewRegRequest : RequestMessage {
  public NewRegRequest () : base ( "new-reg" ) { }

  protected NewRegRequest ( System.String resource ) : base ( resource ) { }

  public IEnumerable<System.String> Contact { get; set; }

  public System.String Agreement { get; set; }

  public System.String Authorizations { get; set; }

  public System.String Certificates { get; set; }
 }
}