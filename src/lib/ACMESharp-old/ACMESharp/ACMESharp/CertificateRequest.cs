using ACMESharp.JOSE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ACMESharp {
 public class CertificateRequest {
  public String CsrContent { get; set; }

  public String Uri { get; set; }

  public IEnumerable<String> Links { get; set; }

  public HttpStatusCode StatusCode { get; set; }

  public DateTime? RetryAfter { get; set; }

  public String CertificateContent { get; set; }

  public void SetCertificateContent ( Byte[] raw ) {
   if ( raw?.Length > 0 ) {
    CertificateContent = JwsHelper.Base64UrlEncode ( raw );
   } else {
    CertificateContent = null;
   }
  }

  public Byte[] GetCertificateContent () {
   if ( String.IsNullOrEmpty ( CertificateContent ) ) {
    return null;
   }

   return JwsHelper.Base64UrlDecode ( CertificateContent );
  }

  public void Save ( Stream s ) {
   using ( var w = new StreamWriter ( s ) ) {
    w.Write ( JsonConvert.SerializeObject ( this, Formatting.Indented ) );
   }
  }

  public void SaveCertificate ( Stream s ) {
   if ( String.IsNullOrEmpty ( CertificateContent ) ) {
    throw new InvalidOperationException ( "Certificate content is missing or empty" );
   }

   var raw = JwsHelper.Base64UrlDecode ( CertificateContent );
   s.Write ( raw, 0, raw.Length );
  }

  public static CertificateRequest Load ( Stream s ) {
   using ( var r = new StreamReader ( s ) ) {
    return JsonConvert.DeserializeObject<CertificateRequest> ( r.ReadToEnd () );
   }
  }
 }
}