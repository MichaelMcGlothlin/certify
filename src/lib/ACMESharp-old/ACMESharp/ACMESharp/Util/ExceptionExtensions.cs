using System;

namespace ACMESharp.Util {
 public static class ExceptionExtensions {
  public static Exception With ( this Exception ex, String key, Object val ) {
   ex.Data[ key ] = val;
   return ex;
  }
 }
}