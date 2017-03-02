using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ACMESharp.Vault.Profile
{
 [Serializable]
 public class VaultProfile : ISerializable
 {
  public VaultProfile(String name, String providerName) : this(name, providerName, null, null) { }

  public VaultProfile(String name, String providerName, IReadOnlyDictionary<String, Object> vaultParams) : this(name, providerName, null, vaultParams) { }

  public VaultProfile(String name, String providerName, IReadOnlyDictionary<String, Object> providerParams = null, IReadOnlyDictionary<String, Object> vaultParams = null)
  {
   Name = name;
   ProviderName = providerName;
   ProviderParameters = providerParams;
   VaultParameters = vaultParams;
  }

  public String Name { get; }

  public String ProviderName { get; }

  public IReadOnlyDictionary<String, Object> ProviderParameters { get; }

  public IReadOnlyDictionary<String, Object> VaultParameters { get; }

  #region -- Custom Serialization --

  public void GetObjectData(SerializationInfo info, StreamingContext context)
  {
   info.AddValue(nameof(Name), Name);
   info.AddValue(nameof(ProviderName), ProviderName);
   info.AddValue(nameof(ProviderParameters), ProviderParameters);
   info.AddValue(nameof(VaultParameters), VaultParameters);
  }

  protected VaultProfile(SerializationInfo info, StreamingContext context)
  {
   Name = info.GetString(nameof(Name));
   ProviderName = info.GetString(nameof(ProviderName));
   ProviderParameters = (Dictionary<String, Object>)info.GetValue(nameof(ProviderParameters), typeof(Dictionary<String, Object>));
   VaultParameters = (Dictionary<String, Object>)info.GetValue(nameof(VaultParameters), typeof(Dictionary<String, Object>));
  }

  #endregion -- Custom Serialization --
 }
}