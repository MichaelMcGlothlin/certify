using System;
using System.Collections.Generic;

namespace Certify.Models {
 public class ManagedSite {
  public String SiteId { get; set; }

  public String SiteName { get; set; }

  public String Server { get; set; }

  public Boolean IncludeInAutoRenew { get; set; }

  public ManagedSiteType SiteType { get; set; }

  // public List<ManagedSiteBinding> SiteBindings { get; set; }
  public List<ManagedSiteLogItem> Logs { get; set; }

  public List<DomainOption> DomainOptions { get; set; }

  public CertRequestConfig RequestConfig { get; set; }

  public void AppendLog ( ManagedSiteLogItem logItem ) {
   Logs = Logs ?? new List<ManagedSiteLogItem> ();

   Logs.Add ( logItem );
  }
 }
}