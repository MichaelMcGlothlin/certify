using ACMESharp.Util;
using ACMESharp.Vault.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACMESharp.Vault.Util {
 public class EntityDictionary<TEntity> : IEnumerable<TEntity>,
     IReadOnlyDictionary<Guid, TEntity>,
     IReadOnlyDictionary<Int32, TEntity>,
     IReadOnlyDictionary<String, TEntity>
     where TEntity : IIdentifiable {
  private readonly IndexedDictionary<Guid, TEntity> _dictById = new IndexedDictionary<Guid, TEntity> ();
  private readonly Dictionary<String, TEntity> _dictByAlias = new Dictionary<String, TEntity> ();

  public EntityDictionary () { }

  public EntityDictionary ( IDictionary<Guid, TEntity> dict ) {
   foreach ( var item in dict ) {
    Add ( item.Value );
   }
  }

  public IEnumerable<Guid> Keys
          => _dictById.Keys;

  public IEnumerable<TEntity> Values
          => _dictById.Values;

  public Int32 Count
          => _dictById.Count;

  IEnumerable<Int32> IReadOnlyDictionary<Int32, TEntity>.Keys {
   get {
    for ( var i = 0; i < _dictById.Count; ++i ) {
     yield return i;
    }
   }
  }

  IEnumerable<String> IReadOnlyDictionary<String, TEntity>.Keys
          => _dictByAlias.Keys;

  public TEntity this[ String key ]
          => _dictByAlias[ key ];

  public TEntity this[ Int32 key ]
          => (TEntity) _dictById[ key ];

  public TEntity this[ Guid key ]
          => _dictById[ key ];

  public void Add ( TEntity item ) {
   _dictById.Add ( item.Id, item );
   if ( !String.IsNullOrEmpty ( item.Alias ) ) {
    _dictByAlias.Add ( item.Alias, item );
   }
  }

  /// <summary>
  /// Renames the alias under which an existing entity is stored.
  /// </summary>
  /// <param name="entityRef">
  ///     Entity reference for an existing
  ///     entity.  This may include an entity which does not
  ///     currently have an alias, in which case it would only
  ///     include an index or ID.
  /// </param>
  /// <param name="newAlias">
  ///     New alias under which to store
  ///     the resolved entity.  This may specify <c>null</c>
  ///     which would remove an existing entity alias.
  /// </param>
  public void Rename ( String entityRef, String newAlias ) {
   // Do some validations ot make sure we can do this first

   if ( String.IsNullOrEmpty ( entityRef ) ) {
    throw new ArgumentNullException ( "ref", "invalid or missing reference" );
   }

   var ent = GetByRef ( entityRef );
   if ( EqualityComparer<TEntity>.Default.Equals ( ent, default ( TEntity ) ) ) {
    throw new KeyNotFoundException ( "unresolved existing entity reference" )
            .With ( nameof ( entityRef ), entityRef )
            .With ( nameof ( newAlias ), newAlias );
   }

   if ( _dictByAlias.ContainsKey ( newAlias ) ) {
    if ( Object.Equals ( _dictByAlias[ newAlias ], ent ) ) {
     // No need to do anything
     return;
    }

    throw new System.InvalidOperationException ( "new alias conflicts with existing entity" )
            .With ( nameof ( entityRef ), entityRef )
            .With ( nameof ( newAlias ), newAlias );
   }

   // Remove existing old alias(es) if there are any
   var existingAliases = _dictByAlias.Where (
           _ => _.Value.Id == ent.Id ).ToArray ();
   foreach ( var kv in existingAliases ) {
    _dictByAlias.Remove ( kv.Key );
   }

   if ( !String.IsNullOrEmpty ( newAlias ) ) {
    _dictByAlias.Add ( newAlias, ent );
   }
  }

  public void Remove ( Guid id ) {
   var x = this[ id ];
   if ( !EqualityComparer<TEntity>.Default.Equals ( x, default ( TEntity ) ) ) {
    _dictByAlias.Remove ( x.Alias );
    _dictById.Remove ( x.Id );
   }
  }

  public TEntity GetByRef ( String entityRef, Boolean throwOnMissing = true,
          TEntity def = default ( TEntity ) ) {
   if ( String.IsNullOrEmpty ( entityRef ) ) {
    throw new ArgumentNullException ( "ref", "invalid or missing reference" );
   }

   if ( entityRef.StartsWith ( "=" ) ) {
    // Ref by ID
    var id = Guid.Parse ( entityRef.Substring ( 1 ) );
    if ( throwOnMissing || ContainsKey ( id ) ) {
     return this[ id ];
    }
   } else if ( Char.IsDigit ( entityRef, 0 )
         || ( entityRef.Length > 1 && entityRef[ 0 ] == '-' && Char.IsDigit ( entityRef, 1 ) ) ) {
    // Ref by Index
    var index = Int32.Parse ( entityRef );
    if ( index < 0 ) {
     // Index is relative from the end
     index = Count + index;
    }

    if ( throwOnMissing || ContainsKey ( index ) ) {
     return this[ index ];
    }
   } else {
    // Ref by Alias
    if ( throwOnMissing || ContainsKey ( entityRef ) ) {
     return this[ entityRef ];
    }
   }

   return def;
  }

  public IEnumerator<TEntity> GetEnumerator () {
   foreach ( var item in _dictById ) {
    yield return item.Value;
   }
  }

  IEnumerator IEnumerable.GetEnumerator () => GetEnumerator ();

  public Boolean ContainsKey ( Guid key ) => _dictById.ContainsKey ( key );

  public Boolean TryGetValue ( Guid key, out TEntity value ) => _dictById.TryGetValue ( key, out value );

  IEnumerator<KeyValuePair<Guid, TEntity>> IEnumerable<KeyValuePair<Guid, TEntity>>.GetEnumerator () => _dictById.GetEnumerator ();

  public Boolean ContainsKey ( Int32 key ) => key >= 0 && _dictById.Count > key;

  public Boolean TryGetValue ( Int32 key, out TEntity value ) {
   try {
    value = (TEntity) _dictById[ key ];
    return true;
   } catch ( Exception ) {
    value = default ( TEntity );
    return false;
   }
  }

  IEnumerator<KeyValuePair<Int32, TEntity>> IEnumerable<KeyValuePair<Int32, TEntity>>.GetEnumerator () {
   var index = 0;
   foreach ( var item in _dictById ) {
    yield return new KeyValuePair<Int32, TEntity> ( index++, item.Value );
   }
  }

  public Boolean ContainsKey ( String key ) => _dictByAlias.ContainsKey ( key );

  public Boolean TryGetValue ( String key, out TEntity value ) => _dictByAlias.TryGetValue ( key, out value );

  IEnumerator<KeyValuePair<String, TEntity>> IEnumerable<KeyValuePair<String, TEntity>>.GetEnumerator () => _dictByAlias.GetEnumerator ();
 }
}