using ACMESharp.Ext;
using System.Collections.Generic;

namespace ACMESharp.Vault {
 public interface IVaultProvider // : IDisposable
 {
  IEnumerable<ParameterDetail> DescribeParameters ();

  IVault GetVault ( IReadOnlyDictionary<System.String, System.Object> initParams );
 }
}