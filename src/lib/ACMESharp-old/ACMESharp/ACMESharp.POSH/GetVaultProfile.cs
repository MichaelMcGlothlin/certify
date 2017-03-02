using ACMESharp.Vault;
using ACMESharp.Vault.Profile;
using System.Management.Automation;

namespace ACMESharp.POSH {
 [Cmdlet ( VerbsCommon.Get, "VaultProfile", DefaultParameterSetName = PSET_LIST )]
 [OutputType ( typeof ( VaultProfile ), ParameterSetName = new System.String[] { PSET_GET } )]
 public class GetVaultProfile : Cmdlet {
  public const System.String PSET_LIST = "List";
  public const System.String PSET_GET = "Get";
  public const System.String PSET_RELOAD_PROVIDERS = "ReloadProviders";

  [Parameter ( ParameterSetName = PSET_LIST )]
  public SwitchParameter ListProfiles { get; set; }

  [Parameter ( ParameterSetName = PSET_GET, Position = 0 )]
  public System.String ProfileName { get; set; }

  [Parameter ( ParameterSetName = PSET_RELOAD_PROVIDERS )]
  public SwitchParameter ReloadProviders { get; set; }

  protected override void ProcessRecord () {
   if ( ReloadProviders ) {
    VaultExtManager.Reload ();
   } else if ( ListProfiles ) {
    WriteObject ( VaultProfileManager.GetProfileNames (), true );
   } else {
    var profileName = VaultProfileManager.ResolveProfileName ( ProfileName );
    WriteObject ( VaultProfileManager.GetProfile ( profileName ) );
   }
  }
 }
}