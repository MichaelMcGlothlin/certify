using System;

namespace ACMESharp.Vault.Model {
 public interface IIdentifiable {
  Guid Id { get; }

  String Alias { get; }
 }
}