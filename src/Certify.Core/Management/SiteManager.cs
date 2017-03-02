using Certify.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Certify.Management {
 public class SiteManager {
  private const String APPDATASUBFOLDER = "Certify";
  private const String SITEMANAGERCONFIG = "sites.json";
  public Boolean IsLocalIISMode { get; set; } //TODO: driven by config

  private List<ManagedSite> managedSites { get; set; }

  public SiteManager () {
   IsLocalIISMode = true;
   managedSites = Preview ();
  }

  private String GetAppDataFolder () {
   var path = Environment.GetFolderPath ( Environment.SpecialFolder.CommonApplicationData ) + "\\" + APPDATASUBFOLDER;
   if ( !System.IO.Directory.Exists ( path ) ) {
    System.IO.Directory.CreateDirectory ( path );
   }
   return path;
  }

  public void StoreSettings () {
   var appDataPath = GetAppDataFolder ();
   var siteManagerConfig = Newtonsoft.Json.JsonConvert.SerializeObject ( managedSites );
   System.IO.File.WriteAllText ( appDataPath + "\\" + SITEMANAGERCONFIG, siteManagerConfig );
  }

  public void LoadSettings () {
   var appDataPath = GetAppDataFolder ();
   var configData = System.IO.File.ReadAllText ( appDataPath + "\\" + SITEMANAGERCONFIG );
   managedSites = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ManagedSite>> ( configData );
  }

  /// <summary>
  /// For current configured environment, show preview of recommended site management (for local IIS, scan sites and recommend actions)
  /// </summary>
  /// <returns></returns>
  public List<ManagedSite> Preview () {
   var sites = new List<ManagedSite> ();

   if ( IsLocalIISMode ) {
    try {
     var iisSites = new IISManager ().GetSiteList ( includeOnlyStartedSites: true ).OrderBy ( s => s.SiteId ).ThenBy ( s => s.Host );

     var siteIds = iisSites.GroupBy ( x => x.SiteId );

     foreach ( var s in siteIds ) {
      var managedSite = new ManagedSite { SiteId = s.Key };
      managedSite.SiteType = ManagedSiteType.LocalIIS;
      managedSite.Server = "localhost";
      managedSite.SiteName = iisSites.First ( i => i.SiteId == s.Key ).SiteName;
      managedSite.SiteBindings = new List<ManagedSiteBinding> ();

      foreach ( var binding in s ) {
       var managedBinding = new ManagedSiteBinding { Hostname = binding.Host, IP = binding.IP, Port = binding.Port, UseSNI = true, CertName = "Certify_" + binding.Host };
       managedSite.SiteBindings.Add ( managedBinding );
      }
      sites.Add ( managedSite );
     }
    } catch ( Exception ) {
     //can't read sites
     System.Diagnostics.Debug.WriteLine ( "Can't get IIS site list." );
    }
   }
   return sites;
  }
 }
}