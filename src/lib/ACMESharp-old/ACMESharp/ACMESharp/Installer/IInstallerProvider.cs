using ACMESharp.Ext;
using System.Collections.Generic;

namespace ACMESharp.Installer {
 /// <summary>
 /// Defines the provider interface need to support discovery
 /// and instance-creation of a
 /// </summary>
 public interface IInstallerProvider {
  IEnumerable<ParameterDetail> DescribeParameters ();

  IInstaller GetInstaller ( IReadOnlyDictionary<System.String, System.Object> initParams );
 }
}