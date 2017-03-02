using ACMESharp.Ext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACMESharp.PKI {
 [ExtManager]
 public static class PkiToolExtManager {
  private static Config _config;
  private static String _DefaultProvider;
  private static readonly Object _lockObject = new Object ();

  public static String DefaultProvider {
   get {
    _DefaultProvider = _DefaultProvider ?? GetProviderInfos ().First ().Name;

    return _DefaultProvider;
   }

   set => _DefaultProvider = value;
  }

  public static IEnumerable<NamedInfo<IPkiToolProviderInfo>> GetProviderInfos () {
   AssertInit ();
   foreach ( var pi in _config ) {
    yield return new NamedInfo<IPkiToolProviderInfo> (
            pi.Key, pi.Value.Metadata );
   }
  }

  public static IPkiToolProviderInfo GetProviderInfo ( String name = null ) {
   AssertInit ();
   if ( String.IsNullOrEmpty ( name ) ) {
    name = DefaultProvider;
   }

   return _config.Get ( name )?.Metadata;
  }

  public static IEnumerable<String> GetAliases () {
   AssertInit ();
   return _config.Aliases.Keys;
  }

  public static IPkiToolProvider GetProvider ( String name = null,
      IReadOnlyDictionary<String, Object> reservedLeaveNull = null ) {
   AssertInit ();
   if ( String.IsNullOrEmpty ( name ) ) {
    name = DefaultProvider;
   }

   return _config.Get ( name )?.Value;
  }

  /// <summary>
  /// Release existing configuration and registry and
  /// tries to rediscover and reload any providers.
  /// </summary>
  public static void Reload () => _config = ExtCommon.ReloadExtConfig<Config> ( _config );

  /// <summary>
  /// Provides a single method to resolve the provider and return an instance of the PKI tool.
  /// </summary>
  /// <returns>
  /// This is primarily provided as a convenience to mimic the previous
  /// CertificateProvider mechanims for resolving PKI tool providers.
  /// </returns>
  public static IPkiTool GetPkiTool ( String name = null,
          IReadOnlyDictionary<String, Object> initParams = null ) {
   initParams = initParams ?? new Dictionary<String, Object> ();

   return GetProvider ( name ).GetPkiTool ( initParams );
  }

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

  private class Config : ExtRegistry<IPkiToolProvider, IPkiToolProviderInfo> {
   public Config () : base ( _ => _.Name ) { }
  }
 }
}