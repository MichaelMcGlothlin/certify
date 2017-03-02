using System.Security.Cryptography.X509Certificates;

namespace Certify.Management {
 public class CertificateManager {
  public X509Certificate GetCertificate ( System.String filename ) {
   var cert = new X509Certificate ();
   cert.Import ( filename );
   return cert;
  }
 }
}