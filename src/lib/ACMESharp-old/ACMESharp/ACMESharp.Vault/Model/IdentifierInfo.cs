using System;
using System.Collections.Generic;

namespace ACMESharp.Vault.Model {
 public class IdentifierInfo : IIdentifiable {
  public Guid Id { get; set; }

  public String Alias { get; set; }

  public String Label { get; set; }

  public String Memo { get; set; }

  public Guid RegistrationRef { get; set; }

  public String Dns { get; set; }

  public AuthorizationState Authorization { get; set; }

  public Dictionary<String, AuthorizeChallenge> Challenges { get; set; }

  public Dictionary<String, DateTime?> ChallengeCompleted { get; set; }

  public Dictionary<String, DateTime?> ChallengeCleanedUp { get; set; }
 }
}