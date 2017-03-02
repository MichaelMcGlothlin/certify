namespace Certify.Models {
 public class AppVersion {
  public System.Int32 Major { get; set; }

  public System.Int32 Minor { get; set; }

  public System.Int32 Patch { get; set; }

  public static AppVersion FromString ( System.String version ) {
   var versionComponents = version.Split ( '.' );

   var current = new AppVersion {
    Major = System.Int32.Parse ( versionComponents[ 0 ] ),
    Minor = System.Int32.Parse ( versionComponents[ 1 ] ),
    Patch = System.Int32.Parse ( versionComponents[ 2 ] )
   };
   return current;
  }

  public static System.Boolean IsOtherVersionNewer ( AppVersion currentVersion, AppVersion otherVersion ) {
   if ( currentVersion.Major >= otherVersion.Major ) {
    if ( currentVersion.Major > otherVersion.Major ) {
     return false;
    }

    //current major version is same, check minor
    if ( currentVersion.Minor >= otherVersion.Minor ) {
     if ( currentVersion.Patch < otherVersion.Patch ) {
      return true;
     } else {
      return false;
     }
    }

    //current minor version is less
    if ( currentVersion.Minor < otherVersion.Minor ) {
     return true;
    }
   } else {
    //other Major version is newer
    return true;
   }

   return false;
  }
 }
}