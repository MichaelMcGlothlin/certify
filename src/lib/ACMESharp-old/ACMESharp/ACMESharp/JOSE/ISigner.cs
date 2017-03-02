using System;
using System.IO;

namespace ACMESharp.JOSE {
 public interface ISigner : IDisposable {
  String JwsAlg { get; }

  void Init ();

  void Save ( Stream stream );

  void Load ( Stream stream );

  Object ExportJwk ( Boolean canonical = false );

  Byte[] Sign ( Byte[] raw );
 }
}