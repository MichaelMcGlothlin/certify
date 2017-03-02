using ACMESharp.POSH;
using ACMESharp.Vault.Model;
using ACMESharp.Vault.Providers;
using ACMESharp.WebServer;
using Certify.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Certify {
 public class VaultManager {
  private VaultInfo vaultConfig;
  private PowershellManager powershellManager;
  private String vaultFolderPath;
  private String vaultFilename;
  public List<ActionLogItem> ActionLogs { get; }

  public String VaultFolderPath => vaultFolderPath;

  public PowershellManager PowershellManager => powershellManager;

  #region Vault

  public Boolean InitVault ( Boolean staging = true ) {
   var apiURI = InitializeVault.WELL_KNOWN_BASE_SERVICES[ InitializeVault.WELL_KNOWN_LESTAGE ];
   if ( !staging ) {
    //live api
    apiURI = InitializeVault.WELL_KNOWN_BASE_SERVICES[ InitializeVault.WELL_KNOWN_LE ];
   }
   powershellManager.InitializeVault ( apiURI );

   vaultFolderPath = GetVaultPath ();

   //create default manual http provider (challenge/response by placing answer in well known location on website for server to fetch);
   //powershellManager.NewProviderConfig("Manual", "manualHttpProvider");
   return true;
  }

  public VaultManager ( String vaultFolderPath, String vaultFilename ) {
   this.vaultFolderPath = vaultFolderPath;
   this.vaultFilename = vaultFilename;

   ActionLogs = new List<ActionLogItem> ();

   powershellManager = new PowershellManager ( vaultFolderPath, ActionLogs );
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

  public VaultInfo GetVaultConfig () {
   if ( vaultConfig != null ) {
    return vaultConfig;
   } else {
    return null;
   }
  }

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

  public Boolean HasContacts () {
   if ( vaultConfig.Registrations?.Count > 0 ) {
    return true;
   } else {
    return false;
   }
  }

  public IdentifierInfo GetIdentifier ( String alias, Boolean reloadVaultConfig = false ) {
   if ( reloadVaultConfig ) {
    ReloadVaultConfig ();
   }

   var identifiers = GetIdentifiers ();
   if ( identifiers != null ) {
    //find best match for given alias/id
    var result = identifiers.Find ( i => i.Alias == alias ) ?? identifiers.Find ( i => i.Dns == alias );
    result = result ?? identifiers.Find ( i => i.Id.ToString () == alias );
    return result;
   } else {
    return null;
   }
  }

  public List<IdentifierInfo> GetIdentifiers () {
   if ( vaultConfig?.Identifiers != null ) {
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

  #endregion Vault

  #region Registration

  public void AddNewRegistration ( String contacts ) {
   powershellManager.NewRegistration ( contacts );

   powershellManager.AcceptRegistrationTOS ();
  }

  internal Boolean DeleteRegistrationInfo ( Guid id ) {
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

  public Boolean DeleteRegistrationInfo ( String Id ) => false;

  #endregion Registration

  #region Certificates

  public Boolean CertExists ( String domainAlias ) {
   var certRef = "cert_" + domainAlias;

   if ( vaultConfig.Certificates?.Values.Any ( c => c.Alias == certRef ) == true ) {
    return true;
   } else {
    return false;
   }
  }

  public void UpdateAndExportCertificate ( String certAlias ) {
   try {
    powershellManager.UpdateCertificate ( certAlias );
    ReloadVaultConfig ();

    var certInfo = GetCertificate ( certAlias );

    // if we have our first cert files, lets export the pfx as well
    ExportCertificate ( certAlias, pfxOnly: true );
   } catch ( Exception exp ) {
    System.Diagnostics.Debug.WriteLine ( exp.ToString () );
   }
  }

  public String CreateCertificate ( String domainAlias ) {
   var certRef = "cert_" + domainAlias;

   powershellManager.NewCertificate ( domainAlias, certRef );

   ReloadVaultConfig ();

   try {
    var apiResult = powershellManager.SubmitCertificate ( certRef );

    //give LE time to generate cert before fetching fresh status info
    Thread.Sleep ( 1000 );
   } catch ( Exception exp ) {
    System.Diagnostics.Debug.WriteLine ( exp.ToString () );
   }

   ReloadVaultConfig ();

   UpdateAndExportCertificate ( certRef );

   return certRef;
  }

  public String GetCertificateFilePath ( Guid id, String assetTypeFolder = LocalDiskVault.CRTDR ) {
   GetVaultPath ();
   var cert = vaultConfig.Certificates[ id ];
   if ( cert != null ) {
    return VaultFolderPath + "\\" + assetTypeFolder;
   }
   return null;
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

  public void ExportCertificate ( String certRef, Boolean pfxOnly = false ) {
   GetVaultPath ();
   if ( !Directory.Exists ( VaultFolderPath + "\\" + LocalDiskVault.ASSET ) ) {
    Directory.CreateDirectory ( VaultFolderPath + "\\" + LocalDiskVault.ASSET );
   }
   powershellManager.ExportCertificate ( certRef, VaultFolderPath, pfxOnly );
  }

  public PendingAuthorization BeginRegistrationAndValidation ( CertRequestConfig requestConfig, String identifierAlias ) {
   var domain = requestConfig.Domain;

   if ( GetIdentifier ( identifierAlias ) == null ) {
    //if an identifier exists for the same dns in vault, remove it to avoid confusion
    DeleteIdentifierByDNS ( domain );
    var result = powershellManager.NewIdentifier ( domain, identifierAlias, "Identifier:" + domain );
    if ( !result.IsOK ) {
     return null;
    }
   }

   var identifier = GetIdentifier ( identifierAlias, reloadVaultConfig: true );

   /*
   //config file now has a temp path to write to, begin challenge (writes to temp file with challenge content)
   */
   if ( identifier.Authorization.IsPending () ) {
    var ccrResult = powershellManager.CompleteChallenge ( identifier.Alias, regenerate: true );

    if ( ccrResult.IsOK ) {
     var extensionlessConfigOK = false;
     var checkViaProxy = true;

     //get challenge info
     ReloadVaultConfig ();
     identifier = GetIdentifier ( identifierAlias );
     var challengeInfo = identifier.Challenges.FirstOrDefault ( c => c.Value.Type == "http-01" ).Value;

     //if copying the file for the user, attempt that now
     if ( challengeInfo != null && requestConfig.PerformChallengeFileCopy ) {
      var httpChallenge = (ACMESharp.ACME.HttpChallenge) challengeInfo.Challenge;
      LogAction ( "Preparing challenge response for LetsEncrypt server to check at: " + httpChallenge.FileUrl );
      LogAction ( "If the challenge response file is not accessible at this exact URL the validation will fail and a certificate will not be issued." );

      //copy temp file to path challenge expects in web folder
      var destFile = Path.Combine ( requestConfig.WebsiteRootPath, httpChallenge.FilePath );
      var destPath = Path.GetDirectoryName ( destFile );
      if ( !Directory.Exists ( destPath ) ) {
       Directory.CreateDirectory ( destPath );
      }

      //copy challenge response to web folder /.well-known/acme-challenge
      System.IO.File.WriteAllText ( destFile, httpChallenge.FileContent );

      var wellknownContentPath = httpChallenge.FilePath.Substring ( 0, httpChallenge.FilePath.LastIndexOf ( "/" ) );
      var testFilePath = Path.Combine ( requestConfig.WebsiteRootPath, wellknownContentPath + "//configcheck" );
      System.IO.File.WriteAllText ( testFilePath, "Extensionless File Config Test - OK" );

      //create a web.config for extensionless files, then test it (make a request for the extensionless configcheck file over http)
      var webConfigContent = Properties.Resources.IISWebConfig;

      if ( !File.Exists ( destPath + "\\web.config" ) ) {
       //no existing config, attempt auto config and perform test
       System.IO.File.WriteAllText ( destPath + "\\web.config", webConfigContent );
       if ( requestConfig.PerformExtensionlessConfigChecks ) {
        if ( CheckURL ( "http://" + domain + "/" + wellknownContentPath + "/configcheck", checkViaProxy ) ) {
         extensionlessConfigOK = true;
        }
       }
      } else {
       //web config already exists, don't overwrite it, just test it

       if ( requestConfig.PerformExtensionlessConfigChecks ) {
        if ( CheckURL ( "http://" + domain + "/" + wellknownContentPath + "/configcheck", checkViaProxy ) ) {
         extensionlessConfigOK = true;
        }
        if ( !extensionlessConfigOK && requestConfig.PerformExtensionlessAutoConfig ) {
         //didn't work, try our default config
         System.IO.File.WriteAllText ( destPath + "\\web.config", webConfigContent );

         if ( CheckURL ( "http://" + domain + "/" + wellknownContentPath + "/configcheck", checkViaProxy ) ) {
          extensionlessConfigOK = true;
         }
        }
       }
      }

      if ( !extensionlessConfigOK && requestConfig.PerformExtensionlessAutoConfig ) {
       //if first attempt(s) at config failed, try an alternative config
       webConfigContent = Properties.Resources.IISWebConfigAlt;

       System.IO.File.WriteAllText ( destPath + "\\web.config", webConfigContent );

       if ( CheckURL ( "http://" + domain + "/" + wellknownContentPath + "/configcheck", checkViaProxy ) ) {
        //ready to complete challenge
        extensionlessConfigOK = true;
       }
      }
     }

     return new PendingAuthorization () { Challenge = challengeInfo, Identifier = identifier, TempFilePath = "", ExtensionlessConfigCheckedOK = extensionlessConfigOK };
    } else {
     return null;
    }
   } else {
    //identifier is already valid (previously authorized)
    return new PendingAuthorization () { Challenge = null, Identifier = identifier, TempFilePath = "", ExtensionlessConfigCheckedOK = false };
   }
  }

  #endregion Certificates

  public String ComputeIdentifierAlias ( String domain ) => "ident" + Guid.NewGuid ().ToString ().Substring ( 0, 8 ).Replace ( "-", "" );

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
       checkSuccess = result.IsAccessible;
      }
     }
    } else {
     //not checking via proxy, base result on status code
     if ( (Int32) response.StatusCode >= 200 && (Int32) response.StatusCode < 300 ) {
      checkSuccess = true;
     }
    }

    if ( ( !checkSuccess ) && useProxyAPI ) {
     //request failed using proxy api, request again using local http
     checkSuccess = CheckURL ( checkUrl, false );
    }
   } catch ( Exception ) {
    System.Diagnostics.Debug.WriteLine ( "Failed to check url for access" );
    checkSuccess = false;
   }

   return checkSuccess;
  }

  public void SubmitChallenge ( String alias, String challengeType = "http-01" ) {
   //well known challenge all ready to be read by server
   powershellManager.SubmitChallenge ( alias, challengeType );

   UpdateIdentifierStatus ( alias, challengeType );
  }

  public void UpdateIdentifierStatus ( String alias, String challengeType = "http-01" ) => powershellManager.UpdateIdentifier ( alias, challengeType );

  public String GetActionLogSummary () {
   var output = "";
   if ( ActionLogs != null ) {
    foreach ( var a in ActionLogs ) {
     output += a.ToString () + "\r\n";
    }
   }
   return output;
  }

  public void PermissionTest () {
   if ( IisSitePathProvider.IsAdministrator () ) {
    System.Diagnostics.Debug.WriteLine ( "User is an administrator" );

    var iisPathProvider = new IisSitePathProvider () {
     WebSiteRoot = @"C:\inetpub\wwwroot\"
    };
    using ( var fs = File.OpenRead ( @"C:\temp\log.txt" ) ) {
     var fileURI = new System.Uri ( iisPathProvider.WebSiteRoot + "/.temp/test/test123" );
     iisPathProvider.UploadFile ( fileURI, fs );
    }
   } else {
    System.Diagnostics.Debug.WriteLine ( "User is not an administrator" );
   }
  }
 }
}