using ACMESharp.ACME;
using System.Collections;
using System.Collections.Generic;

namespace ACMESharp.Messages {
 public class ChallengeAnswerRequest : RequestMessage, IReadOnlyDictionary<System.String, System.Object> {
  private readonly Dictionary<System.String, System.Object> _fieldValues = new Dictionary<System.String, System.Object> ();

  private ChallengeAnswerRequest ( ChallengeAnswer answer )
          : base ( "challenge" ) {
   Answer = answer;

   // Have to reproduce base properties
   // since this class is a dictionary
   _fieldValues[ nameof ( Resource ) ] = base.Resource;

   foreach ( var field in answer.GetFields () ) {
    _fieldValues[ field ] = answer[ field ];
   }
  }

  protected ChallengeAnswer Answer { get; private set; }

  public static ChallengeAnswerRequest CreateRequest ( ChallengeAnswer answer ) => new ChallengeAnswerRequest ( answer );

  #region -- Explicit Implementation Members --

  IEnumerator<KeyValuePair<System.String, System.Object>> IEnumerable<KeyValuePair<System.String, System.Object>>.GetEnumerator () => _fieldValues.GetEnumerator ();

  IEnumerator IEnumerable.GetEnumerator () => _fieldValues.GetEnumerator ();

  System.Int32 IReadOnlyCollection<KeyValuePair<System.String, System.Object>>.Count => _fieldValues.Count;

  System.Boolean IReadOnlyDictionary<System.String, System.Object>.ContainsKey ( System.String key ) => _fieldValues.ContainsKey ( key );

  System.Boolean IReadOnlyDictionary<System.String, System.Object>.TryGetValue ( System.String key, out System.Object value ) => _fieldValues.TryGetValue ( key, out value );

  System.Object IReadOnlyDictionary<System.String, System.Object>.this[ System.String key ] => _fieldValues[ key ];

  IEnumerable<System.String> IReadOnlyDictionary<System.String, System.Object>.Keys => _fieldValues.Keys;

  IEnumerable<System.Object> IReadOnlyDictionary<System.String, System.Object>.Values => _fieldValues.Values;

  #endregion -- Explicit Implementation Members --
 }
}