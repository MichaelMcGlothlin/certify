using System;
using System.Security.Principal;

namespace ACMESharp.Util {
 public static class SysHelper {
  /// <summary>
  /// Resolves if the current process is executing with elevated privileges.
  /// </summary>
  /// <remarks>
  /// A little help from:  http://stackoverflow.com/a/1089061
  /// </remarks>
  public static Boolean IsElevatedAdmin () {
   // Assume false unless we successfully resolve the true status
   var isElevatedAdmin = false;
   try {
    // Get currently logged-in user
    using ( var user = WindowsIdentity.GetCurrent () ) {
     isElevatedAdmin = new WindowsPrincipal ( user )
             .IsInRole ( WindowsBuiltInRole.Administrator );
    }
   } catch ( UnauthorizedAccessException ) {
    // TODO:  log or notify?
   } catch ( Exception ) {
    // TODO:  log or notify?
   }

   return isElevatedAdmin;
  }
 }
}