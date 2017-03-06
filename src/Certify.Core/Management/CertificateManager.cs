using System.Security.Cryptography.X509Certificates;

namespace Certify.Management {
 public class CertificateManager {
  public X509Certificate2 GetCertificate ( System.String filename ) {
   var cert = new X509Certificate2 ();
   cert.Import ( filename );
   return cert;
  }
 }
}