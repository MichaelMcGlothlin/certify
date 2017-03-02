using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ACMESharp.Vault.Util {
 public class IndexedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IOrderedDictionary {
  private List<TKey> _keyList = new List<TKey> ();
  private Dictionary<TKey, TValue> _entDict = new Dictionary<TKey, TValue> ();

  public Object this[ Int32 index ] {
   get => Get ( _keyList[ index ] );

   set => Set ( _keyList[ index ], (TValue) value );
  }

  public Object this[ Object key ] {
   get => Get ( (TKey) key );

   set => Set ( (TKey) key, (TValue) value );
  }

  public TValue this[ TKey key ] {
   get => Get ( key );

   set => Set ( key, value );
  }

  private TValue Get ( TKey key ) => ( (IDictionary<TKey, TValue>) _entDict )[ key ];

  private void Set ( TKey key, TValue value ) => ( (IDictionary<TKey, TValue>) _entDict )[ key ] = value;

  public Int32 Count => ( (IDictionary<TKey, TValue>) _entDict ).Count;

  public Boolean IsReadOnly => ( (IDictionary<TKey, TValue>) _entDict ).IsReadOnly;

  public ICollection<TKey> Keys => ( (IDictionary<TKey, TValue>) _entDict ).Keys;

  public ICollection<TValue> Values => ( (IDictionary<TKey, TValue>) _entDict ).Values;

  ICollection IDictionary.Keys => _entDict.Keys;

  ICollection IDictionary.Values => _entDict.Values;

  public Boolean IsFixedSize => false;

  public Object SyncRoot => this;

  public Boolean IsSynchronized => false;

  public void Insert ( Int32 index, Object key, Object value ) {
   if ( _entDict.ContainsKey ( (TKey) key ) ) {
    throw new ArgumentException ( "An element with the same key already exists", nameof ( key ) );
   }

   _entDict[ (TKey) key ] = (TValue) value;
   _keyList.Insert ( index, (TKey) key );
  }

  public void RemoveAt ( Int32 index ) => Remove ( _keyList[ index ] );

  public Boolean Contains ( Object key ) => _entDict.ContainsKey ( (TKey) key );

  public void Add ( Object key, Object value ) {
   // Assuming this succeeds...
   _entDict.Add ( (TKey) key, (TValue) value );
   // ...add the key to our ordered list
   _keyList.Add ( (TKey) key );
  }

  public void Remove ( Object key ) {
   if ( _entDict.Remove ( (TKey) key ) ) {
    _keyList.Remove ( (TKey) key );
   }
  }

  public void CopyTo ( Array array, Int32 index ) {
   foreach ( var item in this ) {
    array.SetValue ( item, index++ );
   }
  }

  public void Add ( KeyValuePair<TKey, TValue> item ) {
   // Assuming this succeeds...
   ( (IDictionary<TKey, TValue>) _entDict ).Add ( item );
   // ...add the key to our ordered list
   _keyList.Add ( item.Key );
  }

  public void Add ( TKey key, TValue value ) {
   // Assuming this succeeds...
   ( (IDictionary<TKey, TValue>) _entDict ).Add ( key, value );
   // ...add the key to our ordered list
   _keyList.Add ( key );
  }

  public void Clear () {
   _keyList.Clear ();
   ( (IDictionary<TKey, TValue>) _entDict ).Clear ();
  }

  public Boolean Contains ( KeyValuePair<TKey, TValue> item ) => ( (IDictionary<TKey, TValue>) _entDict ).Contains ( item );

  public Boolean ContainsKey ( TKey key ) => ( (IDictionary<TKey, TValue>) _entDict ).ContainsKey ( key );

  public void CopyTo ( KeyValuePair<TKey, TValue>[] array, Int32 arrayIndex ) => ( (IDictionary<TKey, TValue>) _entDict ).CopyTo ( array, arrayIndex );

  public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator () => ( (IDictionary<TKey, TValue>) _entDict ).GetEnumerator ();

  public Boolean Remove ( KeyValuePair<TKey, TValue> item ) => _keyList.Remove ( item.Key )
           && ( (IDictionary<TKey, TValue>) _entDict ).Remove ( item );

  public Boolean Remove ( TKey key ) => _keyList.Remove ( key )
           && ( (IDictionary<TKey, TValue>) _entDict ).Remove ( key );

  public Boolean TryGetValue ( TKey key, out TValue value ) => ( (IDictionary<TKey, TValue>) _entDict ).TryGetValue ( key, out value );

  IDictionaryEnumerator IOrderedDictionary.GetEnumerator () => new IndexedDictionaryEnumerator ( _entDict.GetEnumerator () );

  IDictionaryEnumerator IDictionary.GetEnumerator () => ( (IOrderedDictionary) this ).GetEnumerator ();

  IEnumerator IEnumerable.GetEnumerator () => ( (IDictionary<TKey, TValue>) _entDict ).GetEnumerator ();

  public class IndexedDictionaryEnumerator : IDictionaryEnumerator {
   private Dictionary<TKey, TValue>.Enumerator _baseEnum;
#pragma warning disable RECS0092 // Convert field to readonly
   private DictionaryEntry _Entry;
#pragma warning restore RECS0092 // Convert field to readonly

   public IndexedDictionaryEnumerator ( Dictionary<TKey, TValue>.Enumerator baseEnum ) => _baseEnum = baseEnum;

   public Boolean MoveNext () {
    var ret = _baseEnum.MoveNext ();
    _Entry.Key = _baseEnum.Current.Key;
    _Entry.Value = _baseEnum.Current.Value;
    return ret;
   }

   /// <summary>
   /// Throws NotImplementedException!
   /// This method of the contract is not implemented.
   /// </summary>
   public void Reset () => throw new NotImplementedException ();

   public Object Current => _baseEnum.Current;

   public DictionaryEntry Entry => _Entry;

   public Object Key => _Entry.Key;

   public Object Value => _Entry.Value;
  }
 }
}