namespace ACMESharp.Messages {
 public class AnswerHttpChallengeRequest : RequestMessage {
  public AnswerHttpChallengeRequest () : base ( "challenge" ) { }

  public System.String KeyAuthorization { get; set; }
 }
}