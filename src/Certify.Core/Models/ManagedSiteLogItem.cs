using System;

namespace Certify.Models {
 public class ManagedSiteLogItem {
  public DateTime EventDate { get; set; }

  public String Message { get; set; }

  public LogItemType LogItemType { get; set; }
 }
}