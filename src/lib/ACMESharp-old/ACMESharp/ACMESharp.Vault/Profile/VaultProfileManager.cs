using ACMESharp.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ACMESharp.Vault.Profile {
 public static class VaultProfileManager {

  #region -- Constants --

  public const String APP_ROOT_NAME = "ACMESharp";
  public const String PROFILES_ROOT_NAME = "vaultProfiles";
  public const String PROFILE_DEFAULT_SYS_NAME = ":sys";
  public const String PROFILE_DEFAULT_USER_NAME = ":user";
  public const String PROFILE_DEFAULT_SYS_ROOT_NAME = "sysVault";
  public const String PROFILE_DEFAULT_USER_ROOT_NAME = "userVault";

  public static readonly String PROFILE_ENV_VAR = "ACMESHARP_VAULT_PROFILE";

  /*
  ** These constants will be computed in the type
  ** ctor for the state of the current process
  */

  /// <summary>
  /// This will resolve to the full, user-specific path where Vault Profile definitions
  /// are stored.
  /// </summary>
  public static readonly String PROFILES_ROOT_PATH;

  /// <summary>
  /// This will resolve to the root path of the local disk Vault Provider used
  /// for the built-in default system-level Vault Profile.
  /// </summary>
  public static readonly String APP_SYS_ROOT_PATH;

  /// <summary>
  /// This will resolve to the root path of the local disk Vault Provider used
  /// for the built-in default user-level Vault Profile.
  /// </summary>
  public static readonly String APP_USER_ROOT_PATH;

  /// <summary>
  /// Special constant indication missing or empty Vault Profile.
  /// </summary>
  public static readonly VaultProfile NONE = null;

  /// <summary>
  /// Built-in definition of the default system-level Vault Profile.
  /// </summary>
  public static readonly VaultProfile PROFILE_DEFAULT_SYS;

  /// <summary>
  /// Built-in definition of the default user-level Vault Profile.
  /// </summary>
  public static readonly VaultProfile PROFILE_DEFAULT_USER;

  /// <summary>
  /// Enumeration of all built-in VP definitions.
  /// </summary>
  private static readonly IEnumerable<KeyValuePair<String, VaultProfile>> BUILTIN_PROFILES;

  #endregion -- Constants --

  #region -- Constructors --

  static VaultProfileManager () {
   var sysAppData = Environment.GetFolderPath (
           Environment.SpecialFolder.CommonApplicationData );
   var userAppData = Environment.GetFolderPath (
           Environment.SpecialFolder.LocalApplicationData );

   APP_SYS_ROOT_PATH = Path.Combine ( sysAppData, APP_ROOT_NAME );
   APP_USER_ROOT_PATH = Path.Combine ( userAppData, APP_ROOT_NAME );
   PROFILES_ROOT_PATH = Path.Combine ( APP_USER_ROOT_PATH, PROFILES_ROOT_NAME );

   PROFILE_DEFAULT_SYS = new VaultProfile (
           PROFILE_DEFAULT_SYS_NAME,
           Providers.LocalDiskVaultProvider.PROVIDER_NAME, new Dictionary<String, Object> {
            [ Providers.LocalDiskVaultProvider.ROOT_PATH.Name ] =
                       Path.Combine ( APP_SYS_ROOT_PATH, PROFILE_DEFAULT_SYS_ROOT_NAME ),
            [ Providers.LocalDiskVaultProvider.CREATE_PATH.Name ] = true,
           } );

   PROFILE_DEFAULT_USER = new VaultProfile (
           PROFILE_DEFAULT_USER_NAME,
           Providers.LocalDiskVaultProvider.PROVIDER_NAME, new Dictionary<String, Object> {
            [ Providers.LocalDiskVaultProvider.ROOT_PATH.Name ] =
                       Path.Combine ( APP_USER_ROOT_PATH, PROFILE_DEFAULT_USER_ROOT_NAME ),
            [ Providers.LocalDiskVaultProvider.CREATE_PATH.Name ] = true,
           } );

   BUILTIN_PROFILES = new KeyValuePair<String, VaultProfile>[]
   {
                new KeyValuePair<String, VaultProfile>(PROFILE_DEFAULT_SYS_NAME, PROFILE_DEFAULT_SYS),
                new KeyValuePair<String, VaultProfile>(PROFILE_DEFAULT_USER_NAME, PROFILE_DEFAULT_USER),
   };
  }

  #endregion -- Constructors --

  #region -- Methods --

  public static String ResolveProfileName ( String name = null ) {
   // Incoming name takes precedence if it is provided

   // Next, comes an environment variable in scope for the current process
   if ( String.IsNullOrEmpty ( name ) ) {
    name = Environment.GetEnvironmentVariable ( PROFILE_ENV_VAR );
   }

   // Finally, we specify a default built-in name based on the
   // current elevated privilege status for the current process
   if ( String.IsNullOrEmpty ( name ) ) {
    name = SysHelper.IsElevatedAdmin ()
            ? PROFILE_DEFAULT_SYS_NAME
            : PROFILE_DEFAULT_USER_NAME;
   }

   return name;
  }

  public static IEnumerable<String> GetProfileNames () {
   foreach ( var kv in BUILTIN_PROFILES ) {
    yield return kv.Key;
   }

   if ( Directory.Exists ( PROFILES_ROOT_PATH ) ) {
    foreach ( var f in Directory.GetFiles ( PROFILES_ROOT_PATH ) ) {
     yield return Path.GetFileName ( f );
    }
   }
  }

  public static void SetProfile ( String name, String providerName,
          IReadOnlyDictionary<String, Object> providerParams = null,
          IReadOnlyDictionary<String, Object> vaultParams = null ) {
   if ( name.StartsWith ( ":" ) ) {
    throw new ArgumentException ( "invalid profile name", nameof ( name ) )
            .With ( nameof ( name ), name );
   }

   if ( !Directory.Exists ( PROFILES_ROOT_PATH ) ) {
    Directory.CreateDirectory ( PROFILES_ROOT_PATH );
   }

   var profileFile = Path.Combine ( PROFILES_ROOT_PATH, name );
   var profile = new VaultProfile ( name, providerName, providerParams, vaultParams );

   using ( var fs = new FileStream ( profileFile, FileMode.Create ) ) {
    JsonHelper.Save ( fs, profile );
   }
  }

  public static VaultProfile GetProfile ( String name ) {
   if ( name.StartsWith ( ":" ) ) {
    return BUILTIN_PROFILES.FirstOrDefault ( x =>
              x.Key.Equals ( name, StringComparison.OrdinalIgnoreCase ) ).Value;
   }

   var profile = NONE;
   var profileFile = Path.Combine ( PROFILES_ROOT_PATH, name );
   if ( File.Exists ( profileFile ) ) {
    using ( var fs = new FileStream ( profileFile, FileMode.Open ) ) {
     profile = JsonHelper.Load<VaultProfile> ( fs );
    }
   }

   return profile;
  }

  public static void RemoveProfile ( String name ) {
   if ( name.StartsWith ( ":" ) ) {
    throw new ArgumentException ( "invalid profile name", nameof ( name ) )
            .With ( nameof ( name ), name );
   }

   var profileFile = Path.Combine ( PROFILES_ROOT_PATH, name );
   if ( File.Exists ( profileFile ) ) {
    File.Delete ( profileFile );
   }
  }

  #endregion -- Methods --
 }
}