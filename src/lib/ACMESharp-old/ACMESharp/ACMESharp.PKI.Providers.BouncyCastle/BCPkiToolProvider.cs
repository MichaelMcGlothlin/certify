using ACMESharp.Ext;
using System.Collections.Generic;
using System.Linq;

namespace ACMESharp.PKI.Providers {
 [PkiToolProvider ( BouncyCastleProvider.PROVIDER_NAME,
         Aliases = new[] { "BC" },
         Label = "Bouncy Castle PKI Tool",
         Description = "PKI Tooling implemented using the Bouncy Castle library." )]
 public class BCPkiToolProvider : IPkiToolProvider {
  public IEnumerable<ParameterDetail> DescribeParameters () => new ParameterDetail[ 0 ];

  public IPkiTool GetPkiTool ( IReadOnlyDictionary<System.String, System.Object> initParams ) {
   var kvPairs = initParams.Select ( _ =>
             new KeyValuePair<System.String, System.String> ( _.Key, _.Value.ToString () ) );
   var newParams = kvPairs.ToDictionary (
           _ => _.Key,
           _ => _.Value );
   return new BCPkiTool ( newParams );
  }
 }
}