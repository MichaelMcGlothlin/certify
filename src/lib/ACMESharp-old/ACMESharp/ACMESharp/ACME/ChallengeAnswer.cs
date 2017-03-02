using System.Collections.Generic;

namespace ACMESharp.ACME {
 public abstract class ChallengeAnswer {
  private readonly Dictionary<System.String, System.Object> _fieldValues = new Dictionary<System.String, System.Object> ();

  public System.Object this[ System.String field ] {
   get => _fieldValues[ field ]; protected set => _fieldValues[ field ] = value;
  }

  //public IReadOnlyDictionary<string, object> ToResponseMessage()
  //{
  //    return _fieldValues;
  //}

  public IEnumerable<System.String> GetFields () => _fieldValues.Keys;

  protected void Remove ( System.String field ) => _fieldValues.Remove ( field );
 }

 public class DnsChallengeAnswer : ChallengeAnswer {
  public System.String KeyAuthorization {
   get => this[ nameof ( KeyAuthorization ) ] as System.String; set => this[ nameof ( KeyAuthorization ) ] = value;
  }
 }

 public class HttpChallengeAnswer : ChallengeAnswer {
  public System.String KeyAuthorization {
   get => this[ nameof ( KeyAuthorization ) ] as System.String; set => this[ nameof ( KeyAuthorization ) ] = value;
  }
 }

 public class TlsSniChallengeAnswer : ChallengeAnswer {
  public System.String KeyAuthorization {
   get => this[ nameof ( KeyAuthorization ) ] as System.String; set => this[ nameof ( KeyAuthorization ) ] = value;
  }
 }

 public class PriorKeyChallengeAnswer : ChallengeAnswer {
  public System.String Authorization {
   get => this[ nameof ( KeyAuthorization ) ] as System.String; set => this[ nameof ( KeyAuthorization ) ] = value;
  }

  public System.String KeyAuthorization {
   get => this[ nameof ( KeyAuthorization ) ] as System.String; set => this[ nameof ( KeyAuthorization ) ] = value;
  }

  // TODO:  WORK IN PROGRESS!!!
  //    https://tools.ietf.org/html/draft-ietf-acme-acme-01#section-7.4
  public class Authz {
   public System.String Payload { get; set; }

   public System.String Signature { get; set; }

   public System.String HeaderAlg { get; set; }

   public System.Object HeaderJwk { get; set; }
  }
 }
}