using Certify.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Certify.Management {
 /// <summary>
 /// SiteManager encapsulates settings and operations on the list of Sites we manage certificates for using Certify and is additional to the ACMESharp Vault. These could be Local IIS, Manually Configured, DNS driven etc
 /// </summary>
 public class SiteManager {
  private const String APPDATASUBFOLDER = @"Certify";
  private const String SITEMANAGERCONFIG = @"sites.json";

  /// <summary>
  /// If true, one or more of our managed sites are hosted within a Local IIS server on the same machine
  /// </summary>
  public Boolean EnableLocalIISMode { get; set; } //TODO: driven by config

  private List<ManagedSite> managedSites { get; set; }

  public SiteManager () {
   EnableLocalIISMode = true;
   managedSites = new List<ManagedSite> (); // this.Preview();
  }

  private String GetAppDataFolder () {
   var path = Path.Combine ( Environment.GetFolderPath ( Environment.SpecialFolder.CommonApplicationData ), APPDATASUBFOLDER );
   if ( !Directory.Exists ( path ) ) {
    Directory.CreateDirectory ( path );
   }
   return path;
  }

  public void StoreSettings () {
   var appDataPath = GetAppDataFolder ();
   var siteManagerConfig = JsonConvert.SerializeObject ( managedSites, Newtonsoft.Json.Formatting.Indented );
   File.WriteAllText ( Path.Combine ( appDataPath, SITEMANAGERCONFIG ), siteManagerConfig );
  }

  public void LoadSettings () {
   var appDataPath = GetAppDataFolder ();
   var configData = File.ReadAllText ( Path.Combine ( appDataPath, SITEMANAGERCONFIG ) );
   managedSites = JsonConvert.DeserializeObject<List<ManagedSite>> ( configData );
  }

  /// <summary>
  /// For current configured environment, show preview of recommended site management (for local IIS, scan sites and recommend actions)
  /// </summary>
  /// <returns></returns>
  public List<ManagedSite> Preview () {
   var sites = new List<ManagedSite> ();

   if ( EnableLocalIISMode ) {
    try {
     var iisSites = new IISManager ().GetSiteBindingList ( includeOnlyStartedSites: true ).OrderBy ( s => s.SiteId ).ThenBy ( s => s.Host );

     var siteIds = iisSites.GroupBy ( x => x.SiteId );

     foreach ( var s in siteIds ) {
      var managedSite = new ManagedSite { SiteId = s.Key };
      managedSite.SiteType = ManagedSiteType.LocalIIS;
      managedSite.Server = @"localhost";
      managedSite.SiteName = iisSites.First ( i => i.SiteId == s.Key ).SiteName;

      //TODO: replace sute binding with domain options
      //managedSite.SiteBindings = new List<ManagedSiteBinding>();

      foreach ( var binding in s ) {
       var managedBinding = new ManagedSiteBinding { Hostname = binding.Host, IP = binding.IP, Port = binding.Port, UseSNI = true, CertName = $"Certify_{binding.Host}" };
       // managedSite.SiteBindings.Add(managedBinding);
      }
      sites.Add ( managedSite );
     }
    } catch ( Exception ) {
     //can't read sites
     Debug.WriteLine ( @"Can't get IIS site list." );
    }
   }
   return sites;
  }

  public ManagedSite GetManagedSite ( String siteId, String domain = null ) {
   var site = managedSites.Find ( s => ( siteId != null && s.SiteId == siteId ) || ( domain != null && s.DomainOptions.Any ( bind => bind.Domain == domain ) ) );
   return site;
  }

  public List<ManagedSite> GetManagedSites () {
   var site = managedSites;
   return site;
  }

  public void UpdatedManagedSite ( ManagedSite managedSite ) {
   LoadSettings ();

   var existingSite = managedSites.Find ( s => s.SiteId == managedSite.SiteId );
   if ( existingSite != null ) {
    managedSites.Remove ( existingSite );
   }

   managedSites.Add ( managedSite );
   StoreSettings ();
  }

  public void DeleteManagedSite ( ManagedSite site ) {
   LoadSettings ();

   var existingSite = managedSites.Find ( s => s.SiteId == site.SiteId );
   if ( existingSite != null ) {
    managedSites.Remove ( existingSite );
   }
   StoreSettings ();
  }
 }
}