using ACMESharp.Ext;
using System;
using System.Collections.Generic;

namespace ACMESharp.Installer {
 [ExtManager]
 public static class InstallerExtManager {
  private static Config _config;
  private static readonly Object _lockObject = new Object ();

  public static IEnumerable<NamedInfo<IInstallerProviderInfo>> GetProviderInfos () {
   AssertInit ();
   foreach ( var pi in _config ) {
    yield return new NamedInfo<IInstallerProviderInfo> (
            pi.Key, pi.Value.Metadata );
   }
  }

  public static IInstallerProviderInfo GetProviderInfo ( String name ) {
   AssertInit ();
   return _config.Get ( name )?.Metadata;
  }

  public static IEnumerable<String> GetAliases () {
   AssertInit ();
   return _config.Aliases.Keys;
  }

  public static IInstallerProvider GetProvider ( String name,
      IReadOnlyDictionary<String, Object> reservedLeaveNull = null ) {
   AssertInit ();
   return _config.Get ( name )?.Value;
  }

  /// <summary>
  /// Release existing configuration and registry and
  /// tries to rediscover and reload any providers.
  /// </summary>
  public static void Reload () => _config = ExtCommon.ReloadExtConfig<Config> ( _config );

  private static void AssertInit () {
   if ( _config == null ) {
    lock ( _lockObject ) {
     if ( _config == null ) {
      Reload ();
     }
    }
   }
   if ( _config == null ) {
    throw new InvalidOperationException ( "could not initialize provider configuration" );
   }
  }

  private class Config : ExtRegistry<IInstallerProvider, IInstallerProviderInfo> {
   public Config () : base ( _ => _.Name ) { }
  }
 }
}