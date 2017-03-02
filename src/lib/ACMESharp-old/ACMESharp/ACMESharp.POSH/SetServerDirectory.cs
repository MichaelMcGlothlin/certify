using System;
using System.Collections;
using System.Management.Automation;

namespace ACMESharp.POSH {
 [Cmdlet ( VerbsCommon.Set, "ServerDirectory" )]
 public class SetServerDirectory : Cmdlet {
  public const String PSET_DEFAULT = "Default";
  public const String PSET_SINGLE_RES_ENT = "SingleResourceEntry";

  [Parameter ( ParameterSetName = PSET_DEFAULT )]
  public String IssuerCert { get; set; }

  [Parameter ( ParameterSetName = PSET_DEFAULT )]
  public Hashtable ResourceMap { get; set; }

  [Parameter ( ParameterSetName = PSET_DEFAULT )]
  public Boolean? GetInitialDirectory { get; set; }

  [Parameter ( ParameterSetName = PSET_DEFAULT )]
  public Boolean? UseRelativeInitialDirectory { get; set; }

  [Parameter ( ParameterSetName = PSET_SINGLE_RES_ENT, Mandatory = true )]
  [ValidateNotNullOrEmpty]
  public String Resource { get; set; }

  [Parameter ( ParameterSetName = PSET_SINGLE_RES_ENT, Mandatory = true )]
  [ValidateNotNullOrEmpty]
  public String Path { get; set; }

  [Parameter]
  public String VaultProfile { get; set; }

  protected override void ProcessRecord () {
   using ( var vlt = Util.VaultHelper.GetVault ( VaultProfile ) ) {
    vlt.OpenStorage ();
    var v = vlt.LoadVault ();

    if ( GetInitialDirectory.HasValue ) {
     v.GetInitialDirectory = GetInitialDirectory.Value;
    }

    if ( UseRelativeInitialDirectory.HasValue ) {
     v.UseRelativeInitialDirectory = UseRelativeInitialDirectory.Value;
    }

    if ( !String.IsNullOrEmpty ( IssuerCert ) ) {
     SetResEntry ( v.ServerDirectory, AcmeServerDirectory.RES_ISSUER_CERT, IssuerCert );
    }

    if ( !String.IsNullOrEmpty ( Resource ) && !String.IsNullOrEmpty ( Resource ) ) {
     SetResEntry ( v.ServerDirectory, Resource, Path );
    }

    if ( ResourceMap != null ) {
     foreach ( var ent in ResourceMap ) {
      var dent = (DictionaryEntry) ent;
      SetResEntry ( v.ServerDirectory, dent.Key as String, dent.Value as String );
     }
    }

    vlt.SaveVault ( v );
   }
  }

  private void SetResEntry ( AcmeServerDirectory dir, String res, String path ) {
   if ( dir == null ) {
    throw new ArgumentNullException ( nameof ( dir ), "Server Directory is required" );
   }

   if ( String.IsNullOrEmpty ( res ) ) {
    throw new ArgumentNullException ( nameof ( res ), "Resource name is required" );
   }

   if ( String.IsNullOrEmpty ( path ) ) {
    throw new ArgumentNullException ( nameof ( path ), "Resource path is required" );
   }

   // We can only set resources that are already
   // defined in the server directory mapping
   if ( !dir.Contains ( res ) ) {
    throw new ArgumentOutOfRangeException ( nameof ( res ), "Resource name is invalid or unknown" );
   }

   dir[ res ] = path;
  }
 }
}