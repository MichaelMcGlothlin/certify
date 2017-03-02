using System;

namespace ACMESharp.Vault.Model {
 public class RegistrationInfo : IIdentifiable {
  public Guid Id { get; set; }

  public String Alias { get; set; }

  public String Label { get; set; }

  public String Memo { get; set; }

  public String SignerProvider { get; set; }

  public String SignerState { get; set; }

  public AcmeRegistration Registration { get; set; }
 }
}