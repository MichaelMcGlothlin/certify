using ACMESharp.POSH.Util;
using ACMESharp.Vault.Model;
using ACMESharp.Vault.Util;
using System.Management.Automation;

namespace ACMESharp.POSH {
 [Cmdlet ( VerbsCommon.New, "Registration" )]
 public class NewRegistration : Cmdlet {
  [Parameter ( Mandatory = true )]
  [ValidateCount ( 1, 100 )]
  public System.String[] Contacts { get; set; }

  [Parameter]
  [ValidateSet ( "RS256" )]
  public System.String Signer { get; set; } = "RS256";

  [Parameter]
  public SwitchParameter AcceptTos { get; set; }

  [Parameter]
  public System.String Alias { get; set; }

  [Parameter]
  public System.String Label { get; set; }

  [Parameter]
  public System.String Memo { get; set; }

  [Parameter]
  public System.String VaultProfile { get; set; }

  protected override void ProcessRecord () {
   using ( var vlt = Util.VaultHelper.GetVault ( VaultProfile ) ) {
    vlt.OpenStorage ();
    var v = vlt.LoadVault ();

    AcmeRegistration r = null;
    var ri = new RegistrationInfo {
     Id = EntityHelper.NewId (),
     Alias = Alias,
     Label = Label,
     Memo = Memo,
     SignerProvider = Signer,
    };

    try {
     using ( var c = ClientHelper.GetClient ( v, ri ) ) {
      c.Init ();
      c.GetDirectory ( true );

      r = c.Register ( Contacts );
      if ( AcceptTos ) {
       r = c.UpdateRegistration ( agreeToTos: true );
      }

      ri.Registration = r;

      v.Registrations = v.Registrations ?? new EntityDictionary<RegistrationInfo> ();

      v.Registrations.Add ( ri );
     }
    } catch ( AcmeClient.AcmeWebException ex ) {
     ThrowTerminatingError ( PoshHelper.CreateErrorRecord ( ex, ri ) );
     return;
    }

    vlt.SaveVault ( v );

    WriteObject ( r );
   }
  }
 }
}