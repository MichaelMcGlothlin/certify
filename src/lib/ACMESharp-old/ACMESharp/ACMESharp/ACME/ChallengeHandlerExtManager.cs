using ACMESharp.Ext;
using System;
using System.Collections.Generic;

namespace ACMESharp.ACME {
 [ExtManager]
 public static class ChallengeHandlerExtManager {
  private static Config _config;
  private static readonly Object _lockObject = new Object ();

  public static IEnumerable<NamedInfo<IChallengeHandlerProviderInfo>> GetProviderInfos () {
   AssertInit ();
   foreach ( var pi in _config ) {
    yield return new NamedInfo<IChallengeHandlerProviderInfo> (
            pi.Key, pi.Value.Metadata );
   }
  }

  public static IChallengeHandlerProviderInfo GetProviderInfo ( String name ) {
   AssertInit ();
   return _config.Get ( name )?.Metadata;
  }

  public static IEnumerable<String> GetAliases () {
   AssertInit ();
   return _config.Aliases.Keys;
  }

  public static IChallengeHandlerProvider GetProvider ( String name,
      IReadOnlyDictionary<String, Object> reservedLeaveNull = null ) {
   AssertInit ();
   return _config.Get ( name ).Value;
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

  private class Config : ExtRegistry<IChallengeHandlerProvider, IChallengeHandlerProviderInfo> {
   public Config () : base ( _ => _.Name ) { }
  }
 }
}