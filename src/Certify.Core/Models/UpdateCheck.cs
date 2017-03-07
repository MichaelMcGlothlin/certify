namespace Certify.Models {
 public class UpdateCheck {
  public AppVersion Version { get; set; }

  public UpdateMessage Message { get; set; }

  public System.Boolean IsNewerVersion { get; set; }
 }
}