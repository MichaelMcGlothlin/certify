using ACMESharp.Vault.Util;
using System;

namespace ACMESharp.Vault.Model {
 public class VaultInfo {
  public Guid Id { get; set; }

  public String Alias { get; set; }

  public String Label { get; set; }

  public String Memo { get; set; }

  public String BaseService { get; set; }

  public String BaseUri { get; set; }

  public String Signer { get; set; }

  public String PkiTool { get; set; }

  public Boolean GetInitialDirectory { get; set; } = true;

  public Boolean UseRelativeInitialDirectory { get; set; } = true;

  public AcmeServerDirectory ServerDirectory { get; set; }

  public ProxyConfig Proxy { get; set; }

  public EntityDictionary<ProviderProfileInfo> ProviderProfiles { get; set; }

  public EntityDictionary<InstallerProfileInfo> InstallerProfiles { get; set; }

  public EntityDictionary<RegistrationInfo> Registrations { get; set; }

  public EntityDictionary<IdentifierInfo> Identifiers { get; set; }

  public EntityDictionary<CertificateInfo> Certificates { get; set; }

  public OrderedNameMap<IssuerCertificateInfo> IssuerCertificates { get; set; }
 }
}