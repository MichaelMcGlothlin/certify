using System;

namespace ACMESharp.Vault.Model {
 public class ProviderProfileInfo : IIdentifiable {
  public Guid Id { get; set; }

  public String Alias { get; set; }

  public String Label { get; set; }

  public String Memo { get; set; }
 }
}