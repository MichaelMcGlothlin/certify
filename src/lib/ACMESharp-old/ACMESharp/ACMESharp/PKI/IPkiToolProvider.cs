using ACMESharp.Ext;
using System.Collections.Generic;

namespace ACMESharp.PKI {
 /// <summary>
 /// Defines the provider interface need to support discovery
 /// and instance-creation of a
 /// </summary>
 public interface IPkiToolProvider {
  IEnumerable<ParameterDetail> DescribeParameters ();

  IPkiTool GetPkiTool ( IReadOnlyDictionary<System.String, System.Object> initParams );
 }
}