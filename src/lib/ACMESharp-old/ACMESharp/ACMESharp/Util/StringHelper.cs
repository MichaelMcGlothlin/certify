namespace ACMESharp.Util {
 public static class StringHelper {
  public static System.String IfNullOrEmpty ( System.String s, System.String v1 = null ) {
   if ( System.String.IsNullOrEmpty ( s ) ) {
    return v1;
   }

   return s;
  }
 }
}