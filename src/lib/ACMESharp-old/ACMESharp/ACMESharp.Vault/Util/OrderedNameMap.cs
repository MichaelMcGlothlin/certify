using System;
using System.Collections;
using System.Collections.Generic;

namespace ACMESharp.Vault.Util {
 public class OrderedNameMap<TValue> :
     IReadOnlyDictionary<String, TValue>,
     IReadOnlyDictionary<Int32, TValue> {
  private readonly IndexedDictionary<String, TValue> _dict = new IndexedDictionary<String, TValue> ();

  public OrderedNameMap () { }

  public OrderedNameMap ( IEnumerable<KeyValuePair<String, TValue>> kvs ) {
   foreach ( var kv in kvs ) {
    _dict.Add ( kv );
   }
  }

  public TValue this[ String key ] {
   get => _dict[ key ];

   set => _dict[ key ] = value;
  }

  public TValue this[ Int32 key ] {
   get => (TValue) _dict[ key ];

   set => _dict[ key ] = (TValue) value;
  }

  public Int32 Count => _dict.Count;

  public IEnumerable<String> Keys => _dict.Keys;

  public IEnumerable<TValue> Values => _dict.Values;

  Int32 IReadOnlyCollection<KeyValuePair<Int32, TValue>>.Count => _dict.Count;

  IEnumerable<Int32> IReadOnlyDictionary<Int32, TValue>.Keys {
   get {
    for ( var i = 0; i < _dict.Count; ++i ) {
     yield return i;
    }
   }
  }

  IEnumerable<TValue> IReadOnlyDictionary<Int32, TValue>.Values => _dict.Values;

  public Boolean ContainsKey ( String key ) => _dict.ContainsKey ( key );

  public IEnumerator<KeyValuePair<String, TValue>> GetEnumerator () => _dict.GetEnumerator ();

  public Boolean TryGetValue ( String key, out TValue value ) => _dict.TryGetValue ( key, out value );

  Boolean IReadOnlyDictionary<Int32, TValue>.ContainsKey ( Int32 key ) => key < _dict.Count;

  IEnumerator<KeyValuePair<Int32, TValue>> IEnumerable<KeyValuePair<Int32, TValue>>.GetEnumerator () {
   var index = 0;
   foreach ( var item in _dict ) {
    yield return new KeyValuePair<Int32, TValue> ( index++, item.Value );
   }
  }

  IEnumerator IEnumerable.GetEnumerator () => GetEnumerator ();

  Boolean IReadOnlyDictionary<Int32, TValue>.TryGetValue ( Int32 key, out TValue value ) {
   try {
    value = (TValue) _dict[ key ];
    return true;
   } catch ( Exception ) {
    value = default ( TValue );
    return false;
   }
  }
 }
}