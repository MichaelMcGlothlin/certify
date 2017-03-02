using ACMESharp.Ext;
using System.Collections.Generic;

namespace ACMESharp.Vault.Providers {
 [VaultProvider ( PROVIDER_NAME,
         Label = "Local Disk Vault",
         Description = "Vault provider based on system-local folder and files." )]
 public class LocalDiskVaultProvider : IVaultProvider {
  public const System.String PROVIDER_NAME = "local";

  public static readonly ParameterDetail ROOT_PATH = new ParameterDetail (
          nameof ( LocalDiskVault.RootPath ), ParameterType.TEXT,
          isRequired: true, label: "Root Path",
          desc: "Specifies the directory path where vault data files will be rooted." );

  public static readonly ParameterDetail CREATE_PATH = new ParameterDetail (
          nameof ( LocalDiskVault.CreatePath ), ParameterType.BOOLEAN,
          isRequired: true, label: "Create Path",
          desc: "Specifies the Root Path should be created if it does not exist." );

  public static readonly ParameterDetail BYPASS_EFS = new ParameterDetail (
          nameof ( LocalDiskVault.BypassEFS ), ParameterType.BOOLEAN,
          isRequired: false, label: "Bypass Encrypting File System (EFS)",
          desc: "Specifies not to use the OS-level support for encrypting files;"
                  + " this may be necessary on any file system that does not support EFS" );

  private static readonly ParameterDetail[] PARAMS =
  {
            ROOT_PATH,
            CREATE_PATH,
            BYPASS_EFS,
        };

  public IEnumerable<ParameterDetail> DescribeParameters () => PARAMS;

  public IVault GetVault ( IReadOnlyDictionary<System.String, System.Object> initParams ) {
   var vault = new LocalDiskVault ();

   if ( initParams.ContainsKey ( ROOT_PATH.Name ) ) {
    vault.RootPath = initParams[ ROOT_PATH.Name ] as System.String;
   }

   if ( initParams.ContainsKey ( CREATE_PATH.Name ) ) {
    vault.CreatePath = ( initParams[ CREATE_PATH.Name ]
            as System.Boolean? ).GetValueOrDefault ();
   }

   if ( initParams.ContainsKey ( BYPASS_EFS.Name ) ) {
    vault.BypassEFS = ( initParams[ BYPASS_EFS.Name ]
            as System.Boolean? ).GetValueOrDefault ();
   }

   vault.Init ();

   return vault;
  }

  public void Dispose () { }
 }
}