namespace ACMESharp.Messages {
 public class AnswerDnsChallengeRequest : RequestMessage {
  public AnswerDnsChallengeRequest () : base ( "challenge" ) { }

  public System.String KeyAuthorization { get; set; }
 }
}