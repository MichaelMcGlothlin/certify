using System;
using System.Collections;
using System.Collections.Generic;

namespace ACMESharp {
 public class AcmeServerDirectory : IDisposable, IEnumerable<KeyValuePair<String, String>>,
         IReadOnlyDictionary<String, String>//, ILookup<string, string>
 {
  /// <summary>
  /// Initial resource used to trieve the very first "nonce" header value before starting
  /// a dialogue with the ACME server.  Typically this may just be a one of the other
  /// resource paths, such as the directory instead of a dedicated resource.
  /// </summary>
  public const String RES_INIT = "init";

  public const String RES_DIRECTORY = "directory";
  public const String RES_NEW_REG = "new-reg";
  public const String RES_RECOVER_REG = "recover-reg";
  public const String RES_NEW_AUTHZ = "new-authz";
  public const String RES_NEW_CERT = "new-cert";
  public const String RES_REVOKE_CERT = "revoke-cert";

  /// <summary>
  /// Non-standard, as per the ACME spec, but defined in Boulder.
  /// </summary>
  public const String RES_ISSUER_CERT = "issuer-cert";

  protected const String DEFAULT_PATH_INIT = "/directory";
  protected const String DEFAULT_PATH_DIRECTORY = "/directory";
  protected const String DEFAULT_PATH_NEW_REG = "/new-reg";
  protected const String DEFAULT_PATH_RECOVER_REG = "/recover-reg";
  protected const String DEFAULT_PATH_NEW_AUTHZ = "/new-authz";
  protected const String DEFAULT_PATH_NEW_CERT = "/new-cert";
  protected const String DEFAULT_PATH_REVOKE_CERT = "/revoke-cert";

  protected const String DEFAULT_PATH_ISSUER_CERT = "/acme/issuer-cert";

  private Dictionary<String, String> _dirMap = new Dictionary<String, String> ();

  public AcmeServerDirectory () => InitDirMap ();

  public AcmeServerDirectory ( IDictionary<String, String> dict ) {
   InitDirMap ();
   foreach ( var item in dict ) {
    this[ item.Key ] = item.Value;
   }
  }

  public Int32 Count => _dirMap.Count;

  public IEnumerable<String> Keys => _dirMap.Keys;

  public IEnumerable<String> Values => _dirMap.Values;

  public String this[ String key ] {
   get {
    if ( _dirMap.ContainsKey ( key ) ) {
     return _dirMap[ key ];
    }

    throw new KeyNotFoundException ( "Resource key not found" );
   }

   set => _dirMap[ key ] = value;
  }

  //IEnumerable<string> ILookup<string, string>.this[string key]
  //{
  //    get
  //    {
  //        if (_dirMap.ContainsKey(key))
  //            yield return _dirMap[key];
  //    }
  //}

  private void InitDirMap () {
   // Populate the default path mappings
   _dirMap[ RES_INIT ] = DEFAULT_PATH_INIT;
   _dirMap[ RES_DIRECTORY ] = DEFAULT_PATH_DIRECTORY;
   _dirMap[ RES_NEW_REG ] = DEFAULT_PATH_NEW_REG;
   _dirMap[ RES_RECOVER_REG ] = DEFAULT_PATH_RECOVER_REG;
   _dirMap[ RES_NEW_AUTHZ ] = DEFAULT_PATH_NEW_AUTHZ;
   _dirMap[ RES_NEW_CERT ] = DEFAULT_PATH_NEW_CERT;
   _dirMap[ RES_REVOKE_CERT ] = DEFAULT_PATH_REVOKE_CERT;

   _dirMap[ RES_ISSUER_CERT ] = DEFAULT_PATH_ISSUER_CERT;
  }

  public void Dispose () {
   _dirMap.Clear ();
   _dirMap = null;
  }

  public IEnumerator<KeyValuePair<String, String>> GetEnumerator () => _dirMap.GetEnumerator ();

  IEnumerator IEnumerable.GetEnumerator () => _dirMap.GetEnumerator ();

  public Boolean Contains ( String key ) => _dirMap.ContainsKey ( key );

  //IEnumerator<IGrouping<string, string>> IEnumerable<IGrouping<string, string>>.GetEnumerator()
  //{
  //    foreach (var item in _dirMap)
  //    {
  //        yield return new Grouping { KeyValuePair = item };
  //    }
  //}

  public Boolean ContainsKey ( String key ) => _dirMap.ContainsKey ( key );

  public Boolean TryGetValue ( String key, out String value ) => _dirMap.TryGetValue ( key, out value );

  //private class Grouping : IGrouping<string, string>
  //{
  //    public KeyValuePair<string, string> KeyValuePair
  //    { get; set; }
  //
  //    public string Key
  //    {
  //        get { return KeyValuePair.Key; }
  //    }
  //
  //    public IEnumerator<string> GetEnumerator()
  //    {
  //        yield return KeyValuePair.Value;
  //    }
  //
  //    IEnumerator IEnumerable.GetEnumerator()
  //    {
  //        return (this as IEnumerable<string>).GetEnumerator();
  //    }
  //}
 }
}