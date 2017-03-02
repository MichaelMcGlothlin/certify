using System;
using System.IO;

namespace ACMESharp.Util {
 public abstract class BaseManualProvider {
  public const String STD_OUT = "OUT";
  public const String STD_ERR = "ERR";

  protected String _WriteOutPath = STD_OUT;
  protected TextWriter _writer = Console.Out;

  public String WriteOutPath {
   get => _WriteOutPath;

   set {
    TextWriter newWriter = null;

    if ( String.IsNullOrEmpty ( value ) || value == STD_OUT ) {
     newWriter = Console.Out;
    } else if ( value == STD_ERR ) {
     newWriter = Console.Error;
    } else {
     newWriter = new StreamWriter ( _WriteOutPath, true );
    }

    if ( _writer != null && newWriter != _writer
            && _writer != Console.Out && _writer != Console.Error ) {
     _writer.Close ();
    }
    _writer = newWriter;
   }
  }
 }
}