using Certify.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Certify.Management {
 public class Util {
  public async Task<UpdateCheck> CheckForUpdates ( String appVersion ) {
   /* AppVersion v1 = new AppVersion { Major = 1, Minor = 0, Patch = 1 };
    AppVersion v2 = new AppVersion { Major = 1, Minor = 0, Patch = 2 };
    bool isNewer = AppVersion.IsOtherVersionNewer(v1, v2);

    v2.Patch = 1;
    isNewer = AppVersion.IsOtherVersionNewer(v1, v2);
    v2.Major = 2;
    isNewer = AppVersion.IsOtherVersionNewer(v1, v2);
    v2.Major = 1;
    v2.Minor = 1;
    isNewer = AppVersion.IsOtherVersionNewer(v1, v2);*/

   //get app version
   try {
    var client = new HttpClient ();
    var response = await client.GetAsync ( Properties.Resources.AppUpdateCheckURI + "?v=" + appVersion ).ConfigureAwait ( false );
    if ( response.IsSuccessStatusCode ) {
     var json = await response.Content.ReadAsStringAsync ().ConfigureAwait ( false );
     var checkResult = Newtonsoft.Json.JsonConvert.DeserializeObject<UpdateCheck> ( json );
     checkResult.IsNewerVersion = AppVersion.IsOtherVersionNewer ( AppVersion.FromString ( appVersion ), checkResult.Version );
     return checkResult;
    }

    return new UpdateCheck { IsNewerVersion = false };
   } catch ( Exception ) {
    return null;
   }
  }
 }
}