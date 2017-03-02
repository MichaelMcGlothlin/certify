namespace ACMESharp {
 public abstract class RequestMessage {
  protected RequestMessage ( System.String resource ) => Resource = resource;

  public System.String Resource { get; }
 }
}