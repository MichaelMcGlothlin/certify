using System;

namespace ACMESharp.ACME {
 [Flags]
 public enum ChallengeTypeKind {
  UNSPECIFIED = 0x0,

  PRIOR_KEY = 0x10,
  DNS = 0x20,
  HTTP = 0x40,
  TLS_SNI = 0x80,

  OTHER = 0x1,
 }

 public abstract class Challenge {
  protected Challenge ( ChallengeTypeKind typeKind, String type, ChallengeAnswer answer ) {
   TypeKind = typeKind;
   Type = type;
   Answer = answer ?? throw new ArgumentNullException ( nameof ( answer ), @"challenge answer must is required" );
  }

  public ChallengeTypeKind TypeKind { get; private set; }

  public String Type { get; private set; }

  public ChallengeAnswer Answer { get; private set; }
 }

 public class DnsChallenge : Challenge {
  public DnsChallenge ( String type, ChallengeAnswer answer ) : base ( ChallengeTypeKind.DNS, type, answer ) { }

  public String Token { get; set; }

  public String RecordName { get; set; }

  public String RecordValue { get; set; }
 }

 public class HttpChallenge : Challenge {
  public HttpChallenge ( String type, ChallengeAnswer answer ) : base ( ChallengeTypeKind.HTTP, type, answer ) { }

  public String Token { get; set; }

  public String FileUrl { get; set; }

  public String FilePath { get; set; }

  public String FileContent { get; set; }
 }

 public class TlsSniChallenge : Challenge {
  public TlsSniChallenge ( String type, ChallengeAnswer answer ) : base ( ChallengeTypeKind.TLS_SNI, type, answer ) { }

  public String Token { get; set; }

  public Int32 IterationCount { get; set; }

  // TODO:  this is incomplete!!!
 }
}