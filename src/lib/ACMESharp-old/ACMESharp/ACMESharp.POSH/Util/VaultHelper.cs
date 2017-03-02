using ACMESharp.Util;
using ACMESharp.Vault;
using ACMESharp.Vault.Profile;
using System;

namespace ACMESharp.POSH.Util {
 public static class VaultHelper {
  static VaultHelper () => PoshHelper.BeforeExtAccess ();

  public static IVault GetVault ( String profileName = null ) {
   profileName = VaultProfileManager.ResolveProfileName ( profileName );
   if ( String.IsNullOrEmpty ( profileName ) ) {
    throw new InvalidOperationException ( "unable to resolve effective profile name" );
   }

   var profile = VaultProfileManager.GetProfile ( profileName );
   if ( profile == null ) {
    throw new InvalidOperationException ( "unable to resolve effective profile" )
            .With ( nameof ( profileName ), profileName );
   }

   var provider = VaultExtManager.GetProvider ( profile.ProviderName, null );
   if ( provider == null ) {
    throw new InvalidOperationException ( "unable to resolve Vault Provider" )
            .With ( nameof ( profileName ), profileName )
            .With ( nameof ( profile.ProviderName ), profile.ProviderName );
   }

   return provider.GetVault ( profile.VaultParameters );
  }
 }
}