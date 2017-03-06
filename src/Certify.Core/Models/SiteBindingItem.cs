using System;

namespace Certify.Models {
 public class SiteBindingItem {
  public String Description {
   get {
    if ( Host != null ) {
     return SiteName + " - " + Protocol + "://" + Host + ":" + Port;
    } else {
     return SiteName;
    }
   }
  }

  public String SiteId { get; set; }

  public String SiteName { get; set; }

  public String Host { get; set; }

  public String IP { get; set; }

  public String PhysicalPath { get; set; }

  public Boolean IsHTTPS { get; set; }

  public String Protocol { get; set; }

  public Int32 Port { get; set; }

  public Boolean HasCertificate { get; set; }
 }
}