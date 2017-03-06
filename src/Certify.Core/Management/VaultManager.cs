using ACMESharp.POSH;
using ACMESharp.Vault.Model;
using ACMESharp.Vault.Providers;
using Certify.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Certify {
 public class ProcessStepResult {
  public Boolean IsSuccess;
  public String ErrorMessage;
  public Object Result;
 }

 public class VaultManager {
  private VaultInfo vaultConfig;
  private PowershellManager powershellManager;
  internal String vaultFolderPath;
  private String vaultFilename;
  public List<ActionLogItem> ActionLogs { get; }

  private readonly IdnMapping idnMapping = new IdnMapping ();

  public Boolean UsePowershell { get; set; } = false;

  public String VaultFolderPath {
   get { return vaultFolderPath; }
  }

  #region Vault

  public VaultManager ( String vaultFolderPath, String vaultFilename ) {
   this.vaultFolderPath = vaultFolderPath;
   this.vaultFilename = vaultFilename;

   ActionLogs = new List<ActionLogItem> ();
   if ( UsePowershell ) {
    powershellManager = new PowershellManager ( vaultFolderPath, ActionLogs );
   }

#if DEBUG
   InitVault ( staging: true );
#else
            this.InitVault(staging: false);
#endif
   ReloadVaultConfig ();

   //register default PKI provider
   //ACMESharp.PKI.CertificateProvider.RegisterProvider<ACMESharp.PKI.Providers.OpenSslLibProvider>();
   ACMESharp.PKI.CertificateProvider.RegisterProvider<ACMESharp.PKI.Providers.BouncyCastleProvider> ();
  }

  public Boolean InitVault ( Boolean staging = true ) {
   var apiURI = InitializeVault.WELL_KNOWN_BASE_SERVICES[ InitializeVault.WELL_KNOWN_LESTAGE ];
   if ( !staging ) {
    //live api
    apiURI = InitializeVault.WELL_KNOWN_BASE_SERVICES[ InitializeVault.WELL_KNOWN_LE ];
   }

   var vaultExists = false;
   using ( var vlt = ACMESharp.POSH.Util.VaultHelper.GetVault () ) {
    vlt.OpenStorage ( true );
    var v = vlt.LoadVault ( false );
    if ( v != null ) {
     vaultExists = true;
    }
   }

   if ( !vaultExists ) {
    if ( UsePowershell ) {
     powershellManager.InitializeVault ( apiURI );
    } else {
     var cmd = new ACMESharp.POSH.InitializeVault () {
      BaseUri = apiURI
     };
     cmd.ExecuteCommand ();
    }
   } else {
    LogAction ( "InitVault", "Vault exists." );
   }

   vaultFolderPath = GetVaultPath ();

   //create default manual http provider (challenge/response by placing answer in well known location on website for server to fetch);
   //powershellManager.NewProviderConfig("Manual", "manualHttpProvider");
   return true;
  }

  private void LogAction ( String command, String result = null ) {
   if ( ActionLogs != null ) {
    ActionLogs.Add ( new ActionLogItem { Command = command, Result = result, DateTime = DateTime.Now } );
   }
  }

  public VaultInfo LoadVaultFromFile () {
   using ( var vlt = ACMESharp.POSH.Util.VaultHelper.GetVault () ) {
    vlt.OpenStorage ( true );
    var v = vlt.LoadVault ();
    return v;
   }
  }

  public VaultInfo GetVaultConfig () => vaultConfig ?? null;

  public void CleanupVault ( Guid? identifierToRemove = null, Boolean includeDupeIdentifierRemoval = false ) {
   //remove duplicate identifiers etc

   using ( var vlt = ACMESharp.POSH.Util.VaultHelper.GetVault () ) {
    vlt.OpenStorage ();
    var v = vlt.LoadVault ();

    var toBeRemoved = new List<Guid> ();
    if ( identifierToRemove != null ) {
     if ( v.Identifiers.Keys.Any ( i => i == (Guid) identifierToRemove ) ) {
      toBeRemoved.Add ( (Guid) identifierToRemove );
     }
    } else {
     //find all orphaned identified
     if ( v.Identifiers != null ) {
      foreach ( var k in v.Identifiers.Keys ) {
       var identifier = v.Identifiers[ k ];

       var certs = v.Certificates.Values.Where ( c => c.IdentifierRef == identifier.Id );
       if ( !certs.Any () ) {
        toBeRemoved.Add ( identifier.Id );
       }
      }
     }
    }

    foreach ( var i in toBeRemoved ) {
     v.Identifiers.Remove ( i );
    }
    //

    //find and remove certificates with no valid identifier in vault or with empty settings
    toBeRemoved = new List<Guid> ();

    if ( v.Certificates != null ) {
     foreach ( var c in v.Certificates ) {
      if (
          String.IsNullOrEmpty ( c.IssuerSerialNumber ) //no valid issuer serial
          ||
          !v.Identifiers.ContainsKey ( c.IdentifierRef ) //no existing Identifier
          ) {
       toBeRemoved.Add ( c.Id );
      }
     }

     foreach ( var i in toBeRemoved ) {
      v.Certificates.Remove ( i );
     }
    }

    /*if (includeDupeIdentifierRemoval)
    {
        //remove identifiers where the dns occurs more than once
        foreach (var i in v.Identifiers)
        {
            var count = v.Identifiers.Values.Where(l => l.Dns == i.Dns).Count();
            if (count > 1)
            {
                //identify most recent Identifier (based on assigned, non-expired cert), delete all the others

                toBeRemoved.Add(i.Id);
            }
        }
    }*/

    vlt.SaveVault ( v );
   }
  }

  public void ReloadVaultConfig () => vaultConfig = LoadVaultFromFile ();

  public Boolean IsValidVaultPath ( String vaultPathFolder ) {
   var vaultFile = vaultPathFolder + "\\" + LocalDiskVault.VAULT;
   if ( File.Exists ( vaultFile ) ) {
    return true;
   } else {
    return false;
   }
  }

  public String GetVaultPath () {
   using ( var vlt = (LocalDiskVault) ACMESharp.POSH.Util.VaultHelper.GetVault () ) {
    vaultFolderPath = vlt.RootPath;
   }
   return vaultFolderPath;
  }

  #endregion Vault

  public void UpdateIdentifier ( String domainIdentifierAlias ) {
   if ( UsePowershell ) {
    powershellManager.UpdateIdentifier ( domainIdentifierAlias );
   } else {
    var cmd = new ACMESharp.POSH.UpdateIdentifier () {
     IdentifierRef = domainIdentifierAlias
    };
    cmd.ExecuteCommand ();
   }
  }

  public IdentifierInfo GetIdentifier ( String aliasOrDNS, Boolean reloadVaultConfig = false ) {
   if ( reloadVaultConfig ) {
    ReloadVaultConfig ();
   }

   var identifiers = GetIdentifiers ();
   if ( identifiers != null ) {
    //find best match for given alias/id
    var result = identifiers.Find ( i => i.Alias == aliasOrDNS ) ?? identifiers.Find ( i => i.Dns == aliasOrDNS );
    result = result ?? identifiers.Find ( i => i.Id.ToString () == aliasOrDNS );
    return result;
   } else {
    return null;
   }
  }

  public APIResult SubmitCertificate ( String certAlias ) {
   if ( UsePowershell ) {
    return powershellManager.SubmitCertificate ( certAlias );
   } else {
    var cmd = new ACMESharp.POSH.SubmitCertificate () {
     CertificateRef = certAlias
    };

    try {
     cmd.ExecuteCommand ();
     return new APIResult { IsOK = true, Result = cmd.CommandResult };
    } catch ( Exception exp ) {
     return new APIResult { IsOK = false, Message = exp.ToString (), Result = cmd.CommandResult };
    }
   }
  }

  public APIResult NewCertificate ( String domainIdentifierAlias, String certAlias, String[] subjectAlternativeNameIdentifiers ) {
   if ( UsePowershell ) {
    return powershellManager.NewCertificate ( domainIdentifierAlias, certAlias, subjectAlternativeNameIdentifiers );
   } else {
    var cmd = new ACMESharp.POSH.NewCertificate () {
     IdentifierRef = domainIdentifierAlias,
     Alias = certAlias
    };
    if ( subjectAlternativeNameIdentifiers != null ) {
     cmd.AlternativeIdentifierRefs = subjectAlternativeNameIdentifiers;
    }

    cmd.Generate = new System.Management.Automation.SwitchParameter ( true );

    try {
     cmd.ExecuteCommand ();
     return new APIResult { IsOK = true, Result = cmd.CommandResult };
    } catch ( Exception exp ) {
     return new APIResult { IsOK = false, Message = exp.ToString (), Result = cmd.CommandResult };
    }
   }
  }

  public List<IdentifierInfo> GetIdentifiers () {
   if ( vaultConfig != null && vaultConfig.Identifiers != null ) {
    return vaultConfig.Identifiers.Values.ToList ();
   } else {
    return null;
   }
  }

  public ProviderProfileInfo GetProviderConfig ( String alias ) {
   var vaultConfig = GetVaultConfig ();
   if ( vaultConfig.ProviderProfiles != null ) {
    return vaultConfig.ProviderProfiles.Values.FirstOrDefault ( p => p.Alias == alias );
   } else {
    return null;
   }
  }

  #region Vault Operations

  public Boolean HasContacts () {
   if ( vaultConfig.Registrations != null && vaultConfig.Registrations.Count > 0 ) {
    return true;
   } else {
    return false;
   }
  }

  public void AddNewRegistrationAndAcceptTOS ( String contact ) {
   if ( UsePowershell ) {
    powershellManager.NewRegistration ( contact );

    powershellManager.AcceptRegistrationTOS ();
   } else {
    var cmd = new ACMESharp.POSH.NewRegistration () {
     Contacts = new String[] { contact }
    };
    cmd.ExecuteCommand ();

    var tosCmd = new ACMESharp.POSH.UpdateRegistration () {
     AcceptTos = new System.Management.Automation.SwitchParameter ( true )
    };
    tosCmd.ExecuteCommand ();
   }
  }

  public Boolean DeleteRegistrationInfo ( Guid id ) {
   using ( var vlt = ACMESharp.POSH.Util.VaultHelper.GetVault () ) {
    try {
     vlt.OpenStorage ( true );
     vaultConfig.Registrations.Remove ( id );
     vlt.SaveVault ( vaultConfig );
     return true;
    } catch ( Exception e ) {
     // TODO: Logging of errors.
     System.Windows.Forms.MessageBox.Show ( e.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error );
     return false;
    }
   }
  }

  internal Boolean DeleteIdentifierByDNS ( String dns ) {
   using ( var vlt = ACMESharp.POSH.Util.VaultHelper.GetVault () ) {
    try {
     vlt.OpenStorage ( true );
     if ( vaultConfig.Identifiers != null ) {
      var idsToRemove = vaultConfig.Identifiers.Values.Where ( i => i.Dns == dns );
      var removing = new List<Guid> ();
      foreach ( var identifier in idsToRemove ) {
       removing.Add ( identifier.Id );
      }
      foreach ( var identifier in removing ) {
       vaultConfig.Identifiers.Remove ( identifier );
      }

      vlt.SaveVault ( vaultConfig );
     }

     return true;
    } catch ( Exception e ) {
     // TODO: Logging of errors.
     System.Windows.Forms.MessageBox.Show ( e.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error );
     return false;
    }
   }
  }

  public String ComputeIdentifierAlias ( String domain ) => "ident" + Guid.NewGuid ().ToString ().Substring ( 0, 8 ).Replace ( "-", "" );

  public void SubmitChallenge ( String alias, String challengeType = "http-01" ) {
   //well known challenge all ready to be read by server
   if ( UsePowershell ) {
    powershellManager.SubmitChallenge ( alias, challengeType );
   } else {
    var cmd = new ACMESharp.POSH.SubmitChallenge () {
     IdentifierRef = alias,
     ChallengeType = challengeType
    };
    cmd.ExecuteCommand ();
   }
  }

  public Boolean CertExists ( String domainAlias ) {
   var certRef = "cert_" + domainAlias;

   if ( vaultConfig.Certificates != null && vaultConfig.Certificates.Values.Any ( c => c.Alias == certRef ) ) {
    return true;
   } else {
    return false;
   }
  }

  public String CreateAndSubmitCertificate ( String domainAlias ) {
   var certRef = "cert_" + domainAlias;

   if ( UsePowershell ) {
    powershellManager.NewCertificate ( domainAlias, certRef );
   } else {
    var cmd = new ACMESharp.POSH.NewCertificate () {
     IdentifierRef = domainAlias,
     Alias = certRef
    };
    cmd.ExecuteCommand ();
   }

   ReloadVaultConfig ();

   try {
    SubmitCertificate ( certRef );

    //give LE time to generate cert before fetching fresh status info
    Thread.Sleep ( 1000 );
   } catch ( Exception exp ) {
    System.Diagnostics.Debug.WriteLine ( exp.ToString () );
   }

   ReloadVaultConfig ();

   UpdateAndExportCertificate ( certRef );

   return certRef;
  }

  public CertificateInfo GetCertificate ( String reference, Boolean reloadVaultConfig = false ) {
   if ( reloadVaultConfig ) {
    ReloadVaultConfig ();
   }

   if ( vaultConfig.Certificates != null ) {
    var cert = vaultConfig.Certificates.Values.FirstOrDefault ( c => c.Alias == reference ) ?? vaultConfig.Certificates.Values.FirstOrDefault ( c => c.Id.ToString () == reference );
    return cert;
   }
   return null;
  }

  public void UpdateCertificate ( String certRef ) {
   if ( UsePowershell ) {
    powershellManager.UpdateCertificate ( certRef );
   } else {
    var cmd = new ACMESharp.POSH.UpdateCertificate () {
     CertificateRef = certRef
    };
    cmd.ExecuteCommand ();
   }
  }

  public void UpdateAndExportCertificate ( String certAlias ) {
   try {
    if ( UsePowershell ) {
     powershellManager.UpdateCertificate ( certAlias );
    } else {
     var cmd = new ACMESharp.POSH.UpdateCertificate () {
      CertificateRef = certAlias
     };
     cmd.ExecuteCommand ();
    }

    ReloadVaultConfig ();

    var certInfo = GetCertificate ( certAlias );

    // if we have our first cert files, lets export the pfx as well
    ExportCertificate ( certAlias, pfxOnly: true );
   } catch ( Exception exp ) {
    System.Diagnostics.Debug.WriteLine ( exp.ToString () );
   }
  }

  public String GetCertificateFilePath ( Guid id, String assetTypeFolder = LocalDiskVault.CRTDR ) {
   GetVaultPath ();
   var cert = vaultConfig.Certificates[ id ];
   if ( cert != null ) {
    return VaultFolderPath + "\\" + assetTypeFolder;
   }
   return null;
  }

  public void ExportCertificate ( String certRef, Boolean pfxOnly = false ) {
   GetVaultPath ();
   if ( !Directory.Exists ( VaultFolderPath + "\\" + LocalDiskVault.ASSET ) ) {
    Directory.CreateDirectory ( VaultFolderPath + "\\" + LocalDiskVault.ASSET );
   }

   if ( UsePowershell ) {
    powershellManager.ExportCertificate ( certRef, VaultFolderPath, pfxOnly );
   } else {
    if ( certRef.StartsWith ( "=" ) ) {
     certRef = certRef.Replace ( "=", "" );
    }

    var cmd = new ACMESharp.POSH.GetCertificate () {
     CertificateRef = certRef
    };
    if ( !pfxOnly ) {
     cmd.ExportKeyPEM = vaultFolderPath + "\\" + LocalDiskVault.KEYPM + "\\" + certRef + "-key.pem";
     cmd.ExportCsrPEM = vaultFolderPath + "\\" + LocalDiskVault.CSRPM + "\\" + certRef + "-csr.pem";
     cmd.ExportCertificatePEM = vaultFolderPath + "\\" + LocalDiskVault.CRTPM + "\\" + certRef + "-crt.pem";
     cmd.ExportCertificateDER = vaultFolderPath + "\\" + LocalDiskVault.CRTDR + "\\" + certRef + "-crt.der";
    }
    cmd.ExportPkcs12 = vaultFolderPath + "\\" + LocalDiskVault.ASSET + "\\" + certRef + "-all.pfx";
    cmd.Overwrite = new System.Management.Automation.SwitchParameter ( true );
    cmd.ExecuteCommand ();
   }
  }

  #endregion Vault Operations

  #region ACME Workflow Steps

  public PendingAuthorization BeginRegistrationAndValidation ( CertRequestConfig requestConfig, String identifierAlias, String challengeType = "http-01", String domain = null ) {
   //if no alternative domain specified, use the primary domains as the subject
   domain = domain ?? requestConfig.PrimaryDomain;

   if ( GetIdentifier ( identifierAlias ) == null ) {
    //if an identifier exists for the same dns in vault, remove it to avoid confusion
    DeleteIdentifierByDNS ( domain );

    // ACME service requires international domain names in ascii mode

    if ( UsePowershell ) {
     var result = powershellManager.NewIdentifier ( idnMapping.GetAscii ( domain ), identifierAlias, "Identifier:" + domain );
     if ( !result.IsOK ) {
      return null;
     }
    } else {
     var cmd = new ACMESharp.POSH.NewIdentifier () {
      Dns = idnMapping.GetAscii ( domain ),
      Alias = identifierAlias,
      Label = "Identifier:" + domain
     };

     try {
      cmd.ExecuteCommand ();
     } catch ( Exception exp ) {
      LogAction ( "NewIdentifier", exp.ToString () );
      return null;
     }
    }
   }

   var identifier = GetIdentifier ( identifierAlias, reloadVaultConfig: true );

   if ( identifier.Authorization.IsPending () ) {
    var ccrResultOK = false;
    if ( UsePowershell ) {
     var result = powershellManager.CompleteChallenge ( identifier.Alias, challengeType, regenerate: true );
     ccrResultOK = result.IsOK;
    } else {
     var cmd = new ACMESharp.POSH.CompleteChallenge () {
      IdentifierRef = identifier.Alias,
      ChallengeType = challengeType,
      Handler = "manual",
      Regenerate = new System.Management.Automation.SwitchParameter ( true ),
      Repeat = new System.Management.Automation.SwitchParameter ( true )
     };
     cmd.ExecuteCommand ();
     ccrResultOK = true;
    }

    //get challenge info
    ReloadVaultConfig ();
    identifier = GetIdentifier ( identifierAlias );
    var challengeInfo = identifier.Challenges.FirstOrDefault ( c => c.Value.Type == challengeType ).Value;

    //identifier challenege specification is now ready for use to prepare and answer for LetsEncrypt to check
    return new PendingAuthorization () { Challenge = challengeInfo, Identifier = identifier, TempFilePath = "", ExtensionlessConfigCheckedOK = false };
   } else {
    //identifier is already valid (previously authorized)
    return new PendingAuthorization () { Challenge = null, Identifier = identifier, TempFilePath = "", ExtensionlessConfigCheckedOK = false };
   }
  }

  public PendingAuthorization PerformIISAutomatedChallengeResponse ( CertRequestConfig requestConfig, PendingAuthorization pendingAuth ) {
   var extensionlessConfigOK = false;
   var checkViaProxy = true;

   //if copying the file for the user, attempt that now
   if ( pendingAuth.Challenge != null && requestConfig.PerformChallengeFileCopy ) {
    var httpChallenge = (ACMESharp.ACME.HttpChallenge) pendingAuth.Challenge.Challenge;
    LogAction ( "Preparing challenge response for LetsEncrypt server to check at: " + httpChallenge.FileUrl );
    LogAction ( "If the challenge response file is not accessible at this exact URL the validation will fail and a certificate will not be issued." );

    //copy temp file to path challenge expects in web folder
    var destFile = Path.Combine ( requestConfig.WebsiteRootPath, httpChallenge.FilePath );
    var destPath = Path.GetDirectoryName ( destFile );
    if ( !Directory.Exists ( destPath ) ) {
     Directory.CreateDirectory ( destPath );
    }

    //copy challenge response to web folder /.well-known/acme-challenge
    File.WriteAllText ( destFile, httpChallenge.FileContent );

    var wellknownContentPath = httpChallenge.FilePath.Substring ( 0, httpChallenge.FilePath.LastIndexOf ( "/" ) );
    var testFilePath = Path.Combine ( requestConfig.WebsiteRootPath, wellknownContentPath + "//configcheck" );
    File.WriteAllText ( testFilePath, "Extensionless File Config Test - OK" );

    //create a web.config for extensionless files, then test it (make a request for the extensionless configcheck file over http)
    var webConfigContent = Properties.Resources.IISWebConfig;

    if ( !File.Exists ( destPath + "\\web.config" ) ) {
     //no existing config, attempt auto config and perform test
     File.WriteAllText ( destPath + "\\web.config", webConfigContent );
     if ( requestConfig.PerformExtensionlessConfigChecks ) {
      if ( CheckURL ( "http://" + requestConfig.PrimaryDomain + "/" + wellknownContentPath + "/configcheck", checkViaProxy ) ) {
       extensionlessConfigOK = true;
      }
     }
    } else {
     //web config already exists, don't overwrite it, just test it

     if ( requestConfig.PerformExtensionlessConfigChecks ) {
      if ( CheckURL ( "http://" + requestConfig.PrimaryDomain + "/" + wellknownContentPath + "/configcheck", checkViaProxy ) ) {
       extensionlessConfigOK = true;
      }
      if ( !extensionlessConfigOK && requestConfig.PerformExtensionlessAutoConfig ) {
       //didn't work, try our default config
       File.WriteAllText ( destPath + "\\web.config", webConfigContent );

       if ( CheckURL ( "http://" + requestConfig.PrimaryDomain + "/" + wellknownContentPath + "/configcheck", checkViaProxy ) ) {
        extensionlessConfigOK = true;
       }
      }
     }
    }

    if ( !extensionlessConfigOK && requestConfig.PerformExtensionlessAutoConfig ) {
     //if first attempt(s) at config failed, try an alternative config
     webConfigContent = Properties.Resources.IISWebConfigAlt;

     File.WriteAllText ( destPath + "\\web.config", webConfigContent );

     if ( CheckURL ( "http://" + requestConfig.PrimaryDomain + "/" + wellknownContentPath + "/configcheck", checkViaProxy ) ) {
      //ready to complete challenge
      extensionlessConfigOK = true;
     }
    }
   }

   //configuration applied, ready to ask LE to validate our answer
   pendingAuth.ExtensionlessConfigCheckedOK = extensionlessConfigOK;
   return pendingAuth;
  }

  public Boolean CompleteIdentifierValidationProcess ( String domainIdentifierAlias ) {
   UpdateIdentifier ( domainIdentifierAlias );
   var identiferStatus = GetIdentifier ( domainIdentifierAlias, true );
   var attempts = 0;
   var maxAttempts = 3;

   while ( identiferStatus.Authorization.Status == "pending" && attempts < maxAttempts ) {
    System.Threading.Thread.Sleep ( 2000 ); //wait a couple of seconds before checking again
    UpdateIdentifier ( domainIdentifierAlias );
    identiferStatus = GetIdentifier ( domainIdentifierAlias, true );
    attempts++;
   }

   if ( identiferStatus.Authorization.Status != "valid" ) {
    //still pending or failed
    System.Diagnostics.Debug.WriteLine ( "LE Authorization problem: " + identiferStatus.Authorization.Status );
    return false;
   } else {
    //ready to get cert
    return true;
   }
  }

  public ProcessStepResult PerformCertificateRequestProcess ( String domainIdentifierRef, String[] alternativeIdentifierRefs ) {
   //

   //all good, we can request a certificate
   //if authorizing a SAN we would need to repeat the above until all domains are valid, then we can request cert
   var certAlias = "cert_" + domainIdentifierRef;

   //register cert placeholder in vault

   NewCertificate ( domainIdentifierRef, certAlias, subjectAlternativeNameIdentifiers: alternativeIdentifierRefs );

   //ask LE to issue a certificate for our domain(s)
   //if this step fails we should quit and try again later
   SubmitCertificate ( certAlias );

   //LE may now have issued a certificate, this process may not be immediate
   var certDetails = GetCertificate ( certAlias, reloadVaultConfig: true );
   var attempts = 0;
   var maxAttempts = 3;

   //cert not issued yet, wait and try again
   while ( ( certDetails == null || String.IsNullOrEmpty ( certDetails.IssuerSerialNumber ) ) && attempts < maxAttempts ) {
    System.Threading.Thread.Sleep ( 2000 ); //wait a couple of seconds before checking again
    UpdateCertificate ( certAlias );
    certDetails = GetCertificate ( certAlias, reloadVaultConfig: true );
    attempts++;
   }

   if ( certDetails != null && !String.IsNullOrEmpty ( certDetails.IssuerSerialNumber ) ) {
    //we have an issued certificate, we can go ahead and install it as required
    System.Diagnostics.Debug.WriteLine ( "Received certificate issued by LE." + JsonConvert.SerializeObject ( certDetails ) );

    //if using cert in IIS, we need to export the certificate PFX file, install it as a certificate and setup the site binding to map to this cert
    var certFolderPath = GetCertificateFilePath ( certDetails.Id, LocalDiskVault.ASSET );
    var pfxFile = certAlias + "-all.pfx";
    var pfxPath = System.IO.Path.Combine ( certFolderPath, pfxFile );

    //create folder to export PFX to, if required
    if ( !Directory.Exists ( certFolderPath ) ) {
     Directory.CreateDirectory ( certFolderPath );
    }

    //if file already exists we want to delet the old one
    if ( File.Exists ( pfxPath ) ) {
     //delete existing PFX (if any)
     File.Delete ( pfxPath );
    }

    //export the PFX file
    ExportCertificate ( certAlias, pfxOnly: true );

    if ( !File.Exists ( pfxPath ) ) {
     return new ProcessStepResult { IsSuccess = false, ErrorMessage = "Failed to export PFX. " + pfxPath, Result = pfxPath };
    } else {
     return new ProcessStepResult { IsSuccess = true, Result = pfxPath };
    }
   } else {
    return new ProcessStepResult { IsSuccess = false, ErrorMessage = "Failed to get new certificate from LetsEncrypt." };
   }
  }

  #endregion ACME Workflow Steps

  #region Utils

  public String GetActionLogSummary () {
   var output = "";
   if ( ActionLogs != null ) {
    foreach ( var a in ActionLogs ) {
     output += a.ToString () + "\r\n";
    }
   }
   return output;
  }

  private Boolean CheckURL ( String url, Boolean useProxyAPI ) {
   var checkUrl = url + "";
   if ( useProxyAPI ) {
    url = "https://certify.webprofusion.com/api/testurlaccess?url=" + url;
   }
   //check http request to test path works
   var checkSuccess = false;
   try {
    var request = WebRequest.Create ( url );
    var response = (HttpWebResponse) request.GetResponse ();

    //if checking via proxy, examine result
    if ( useProxyAPI ) {
     if ( (Int32) response.StatusCode >= 200 ) {
      var encoding = ASCIIEncoding.UTF8;
      using ( var reader = new System.IO.StreamReader ( response.GetResponseStream (), encoding ) ) {
       var jsonText = reader.ReadToEnd ();
       LogAction ( "URL Check Result: " + jsonText );
       var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.API.URLCheckResult> ( jsonText );
       if ( result.IsAccessible == true ) {
        checkSuccess = true;
       } else {
        checkSuccess = false;
       }
      }
     }
    } else {
     //not checking via proxy, base result on status code
     if ( (Int32) response.StatusCode >= 200 && (Int32) response.StatusCode < 300 ) {
      checkSuccess = true;
     }
    }

    if ( checkSuccess == false && useProxyAPI == true ) {
     //request failed using proxy api, request again using local http
     checkSuccess = CheckURL ( checkUrl, false );
    }
   } catch ( Exception ) {
    System.Diagnostics.Debug.WriteLine ( "Failed to check url for access" );
    checkSuccess = false;
   }

   return checkSuccess;
  }

  #endregion Utils
 }
}