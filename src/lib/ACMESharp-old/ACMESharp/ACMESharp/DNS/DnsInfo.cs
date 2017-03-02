using System.Text;

namespace ACMESharp.DNS {
 public class DnsInfo {
  // TOOD: this is repeated from WebServerInfo, clean this up!
  private static Newtonsoft.Json.JsonSerializerSettings JSS =
          new Newtonsoft.Json.JsonSerializerSettings {
           Formatting = Newtonsoft.Json.Formatting.Indented,
           TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
          };

  public System.String DefaultDomain { get; set; }

  public IXXXDnsProvider Provider { get; set; }

  public void Save ( System.IO.Stream s ) {
   using ( var w = new System.IO.StreamWriter ( s ) ) {
    w.Write ( Newtonsoft.Json.JsonConvert.SerializeObject ( this, JSS ) );
   }
  }

  public System.String Save () {
   using ( var w = new System.IO.MemoryStream () ) {
    Save ( w );
    return Encoding.UTF8.GetString ( w.ToArray () );
   }
  }

  public static DnsInfo Load ( System.IO.Stream s ) {
   using ( var r = new System.IO.StreamReader ( s ) ) {
    return Newtonsoft.Json.JsonConvert.DeserializeObject<DnsInfo> (
            r.ReadToEnd (), JSS );
   }
  }

  public static DnsInfo Load ( System.String json ) {
   using ( var r = new System.IO.MemoryStream ( Encoding.UTF8.GetBytes ( json ) ) ) {
    return Load ( r );
   }
  }
 }
}