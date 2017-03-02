using ACMESharp.Vault.Model;
using ACMESharp.Vault.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace ACMESharp.POSH {
 [Cmdlet ( VerbsData.Initialize, "Vault", DefaultParameterSetName = PSET_BASE_SERVICE )]
 public class InitializeVault : Cmdlet {
  public const System.String PSET_BASE_SERVICE = "BaseService";
  public const System.String PSET_BASE_URI = "BaseUri";

  public const System.String WELL_KNOWN_LE = "LetsEncrypt";
  public const System.String WELL_KNOWN_LESTAGE = "LetsEncrypt-STAGING";

  public static readonly IReadOnlyDictionary<System.String, System.String> WELL_KNOWN_BASE_SERVICES =
          new ReadOnlyDictionary<System.String, System.String> ( new IndexedDictionary<System.String, System.String> {
           [ WELL_KNOWN_LE ] = "https://acme-v01.api.letsencrypt.org/",
           [ WELL_KNOWN_LESTAGE ] = "https://acme-staging.api.letsencrypt.org/",
          } );

  [Parameter ( ParameterSetName = PSET_BASE_SERVICE )]
  [ValidateSet (
          WELL_KNOWN_LE,
          WELL_KNOWN_LESTAGE,
          IgnoreCase = true )]
  public System.String BaseService { get; set; } = WELL_KNOWN_LE;

  [Parameter ( ParameterSetName = PSET_BASE_URI, Mandatory = true )]
  [ValidateNotNullOrEmpty]
  public System.String BaseUri { get; set; }

  [Parameter]
  public SwitchParameter Force { get; set; }

  [Parameter]
  public System.String Alias { get; set; }

  [Parameter]
  public System.String Label { get; set; }

  [Parameter]
  public System.String Memo { get; set; }

  [Parameter]
  public System.String VaultProfile { get; set; }

  protected override void ProcessRecord () {
   var baseUri = BaseUri;
   if ( System.String.IsNullOrEmpty ( baseUri ) ) {
    if ( !System.String.IsNullOrEmpty ( BaseService )
            && WELL_KNOWN_BASE_SERVICES.ContainsKey ( BaseService ) ) {
     baseUri = WELL_KNOWN_BASE_SERVICES[ BaseService ];
     WriteVerbose ( $"Resolved Base URI from Base Service [{baseUri}]" );
    } else {
     throw new PSInvalidOperationException ( "either a base service or URI is required" );
    }
   }

   using ( var vlt = Util.VaultHelper.GetVault ( VaultProfile ) ) {
    WriteVerbose ( "Initializing Storage Backend" );
    vlt.InitStorage ( Force );
    var v = new VaultInfo {
     Id = EntityHelper.NewId (),
     Alias = Alias,
     Label = Label,
     Memo = Memo,
     BaseService = BaseService,
     BaseUri = baseUri,
     ServerDirectory = new AcmeServerDirectory ()
    };

    vlt.SaveVault ( v );
   }
  }
 }
}