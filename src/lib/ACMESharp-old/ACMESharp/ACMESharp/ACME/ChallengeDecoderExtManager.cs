using ACMESharp.Ext;
using System;
using System.Collections.Generic;

namespace ACMESharp.ACME {
 [ExtManager]
 public static class ChallengeDecoderExtManager {
  private static Config _config;
  private static readonly Object _lockObject = new Object ();

  public static IEnumerable<NamedInfo<IChallengeDecoderProviderInfo>> GetProviderInfos () {
   AssertInit ();
   foreach ( var pi in _config ) {
    yield return new NamedInfo<IChallengeDecoderProviderInfo> (
            pi.Key, pi.Value.Metadata );
   }
  }

  public static IChallengeDecoderProviderInfo GetProviderInfo ( String type ) {
   AssertInit ();
   return _config.Get ( type )?.Metadata;
  }

  public static IChallengeDecoderProvider GetProvider ( String type,
      IReadOnlyDictionary<String, Object> reservedLeaveNull = null ) {
   AssertInit ();
   return _config.Get ( type )?.Value;
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

  private class Config : ExtRegistry<IChallengeDecoderProvider, IChallengeDecoderProviderInfo> {
   public Config () : base ( _ => _.Type ) { }
  }
 }
}