using System;
using System.IO;
using System.Management.Automation;

namespace ACMESharp.POSH {
 [Cmdlet ( VerbsCommon.Get, "IssuerCertificate" )]
 public class GetIssuerCertificate : Cmdlet {
  [Parameter]
  public String SerialNumber { get; set; }

  [Parameter]
  public String ExportCertificatePEM { get; set; }

  [Parameter]
  public String ExportCertificateDER { get; set; }

  [Parameter]
  public SwitchParameter Overwrite { get; set; }

  [Parameter]
  public String VaultProfile { get; set; }

  protected override void ProcessRecord () {
   using ( var vlt = Util.VaultHelper.GetVault ( VaultProfile ) ) {
    vlt.OpenStorage ();
    var v = vlt.LoadVault ();

    if ( v.IssuerCertificates == null || v.IssuerCertificates.Count < 1 ) {
     throw new InvalidOperationException ( "No issuer certificates found" );
    }

    if ( String.IsNullOrEmpty ( SerialNumber ) ) {
     WriteObject ( v.IssuerCertificates.Values, true );
    } else {
     if ( !v.IssuerCertificates.ContainsKey ( SerialNumber ) ) {
      throw new ItemNotFoundException ( "Unable to find an Issuer Certificate for the given serial number" );
     }

     var ic = v.IssuerCertificates[ SerialNumber ];
     var mode = Overwrite ? FileMode.Create : FileMode.CreateNew;

     if ( !String.IsNullOrEmpty ( ExportCertificatePEM ) ) {
      if ( String.IsNullOrEmpty ( ic.CrtPemFile ) ) {
       throw new InvalidOperationException ( "Cannot export CRT; CRT hasn't been retrieved" );
      }

      GetCertificate.CopyTo ( vlt, Vault.VaultAssetType.IssuerPem, ic.CrtPemFile,
              ExportCertificatePEM, mode );
     }

     if ( !String.IsNullOrEmpty ( ExportCertificateDER ) ) {
      if ( String.IsNullOrEmpty ( ic.CrtDerFile ) ) {
       throw new InvalidOperationException ( "Cannot export CRT; CRT hasn't been retrieved" );
      }

      GetCertificate.CopyTo ( vlt, Vault.VaultAssetType.IssuerDer, ic.CrtDerFile,
              ExportCertificateDER, mode );
     }

     WriteObject ( v.IssuerCertificates[ SerialNumber ] );
    }
   }
  }
 }
}