using System.Collections.Generic;

namespace Certify.Models {
 public enum ManagedSiteType {
  LocalIIS = 1
 }

 public enum PlannedActionType {
  NewCertificate = 0,
  ReplaceCertificate = 1,
  KeepCertificate = 2,
  Ignore = 3
 }

 public class ManagedSiteBinding {
  public System.String Hostname { get; set; }

  public System.Int32 Port { get; set; }

  /// <summary>
  /// IP is either * (all unassigned) or a specific IP
  /// </summary>
  public System.String IP { get; set; }

  public System.Boolean UseSNI { get; set; }

  public System.String CertName { get; set; }

  public PlannedActionType PlannedAction { get; set; }
 }

 public class ManagedSite {
  public System.String SiteId { get; set; }

  public System.String SiteName { get; set; }

  public System.String Server { get; set; }

  public ManagedSiteType SiteType { get; set; }

  public List<ManagedSiteBinding> SiteBindings { get; set; }
 }

 public class SiteBindingItem {
  public System.String Description => SiteName + " - " + Protocol + "://" + Host + ":" + Port;

  public System.String SiteId { get; set; }

  public System.String SiteName { get; set; }

  public System.String Host { get; set; }

  public System.String IP { get; set; }

  public System.String PhysicalPath { get; set; }

  public System.Boolean IsHTTPS { get; set; }

  public System.String Protocol { get; set; }

  public System.Int32 Port { get; set; }

  public System.Boolean HasCertificate { get; set; }
 }
}