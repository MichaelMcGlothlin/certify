using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ACMESharp.ACME.Providers {
 public class ManualChallengeHandler : IChallengeHandler {

  #region -- Constants --

  public const String STD_OUT = "OUT";
  public const String STD_ERR = "ERR";
  public const String STD_DBG = "DBG";

  private static readonly TextWriter DEBUG_WRITER =
          new StreamWriter ( new DebugStream (), Encoding.UTF8 );

  #endregion -- Constants --

  #region -- Fields --

  private Stream _stream = null;
  private TextWriter _writer = Console.Out;

  #endregion -- Fields --

  #region -- Properties --

  public String WriteOutPath { get; private set; } = STD_OUT;

  public Boolean Append { get; private set; }

  public Boolean Overwrite { get; private set; }

  public Boolean IsDisposed { get; private set; }

  #endregion -- Properties --

  #region -- Methods --

  public void SetOut ( String path, Boolean append = false, Boolean overwrite = false ) {
   var priorS = _stream;
   var priorW = _writer;

   switch ( path ) {
    case STD_OUT:
     _writer = Console.Out;
     _stream = null;
     break;

    case STD_ERR:
     _writer = Console.Error;
     _stream = null;
     break;

    case STD_DBG:
     _writer = DEBUG_WRITER;
     _stream = null;
     break;

    default:
     _writer = null;

     if ( append ) {
      _stream = new FileStream ( path, FileMode.Append );
     } else if ( overwrite ) {
      _stream = new FileStream ( path, FileMode.Create );
     } else {
      _stream = new FileStream ( path, FileMode.CreateNew );
     }

     _writer = new StreamWriter ( _stream );
     break;
   }

   if ( priorS != null ) {
    try {
     priorW.Dispose ();
     priorS.Dispose ();
    } catch ( Exception ) {
     // TODO: failure to clean up the prior Out
     // should do what???
    }
   }
  }

  public void Handle ( Challenge c ) {
   AssertNotDisposed ();

   var dnsChallenge = c as DnsChallenge;
   var httpChallenge = c as HttpChallenge;

   if ( dnsChallenge != null ) {
    _writer.WriteLine ( $"== Manual Challenge Handler - DNS ==" );
    _writer.WriteLine ( $"  * Handle Time:      [{DateTime.Now}]" );
    _writer.WriteLine ( $"  * Challenge Token:  [{dnsChallenge.Token}]" );
    _writer.WriteLine ( $"To complete this Challenge please create a new Resource" );
    _writer.WriteLine ( $"Record (RR) with the following characteristics:" );
    _writer.WriteLine ( $"  * RR Type:  [TXT]" );
    _writer.WriteLine ( $"  * RR Name:  [{dnsChallenge.RecordName}]" );
    _writer.WriteLine ( $"  * RR Value: [{dnsChallenge.RecordValue}]" );
    _writer.WriteLine ( $"------------------------------------" );
    _writer.Flush ();
   } else if ( httpChallenge != null ) {
    _writer.WriteLine ( $"== Manual Challenge Handler - HTTP ==" );
    _writer.WriteLine ( $"  * Handle Time:      [{DateTime.Now}]" );
    _writer.WriteLine ( $"  * Challenge Token:  [{httpChallenge.Token}]" );
    _writer.WriteLine ( $"To complete this Challenge please create a new file" );
    _writer.WriteLine ( $"under the server that is responding to the hostname" );
    _writer.WriteLine ( $"and path given with the following characteristics:" );
    _writer.WriteLine ( $"  * HTTP URL:     [{httpChallenge.FileUrl}]" );
    _writer.WriteLine ( $"  * File Path:    [{httpChallenge.FilePath}]" );
    _writer.WriteLine ( $"  * File Content: [{httpChallenge.FileContent}]" );
    _writer.WriteLine ( $"  * MIME Type:    [text/plain]" );
    _writer.WriteLine ( $"------------------------------------" );
    _writer.Flush ();
   } else {
    var ex = new InvalidOperationException ( "unsupported Challenge type" );
    ex.Data[ "challengeType" ] = c.GetType ();
    throw ex;
   }
  }

  public void CleanUp ( Challenge c ) {
   AssertNotDisposed ();

   var dnsChallenge = c as DnsChallenge;
   var httpChallenge = c as HttpChallenge;

   if ( dnsChallenge != null ) {
    _writer.WriteLine ( $"== Manual Challenge Handler - DNS ==" );
    _writer.WriteLine ( $"  * CleanUp Time:     [{DateTime.Now}]" );
    _writer.WriteLine ( $"  * Challenge Token:  [{dnsChallenge.Token}]" );
    _writer.WriteLine ( $"The Challenge has been completed -- you can now remove the" );
    _writer.WriteLine ( $"Resource Record created previously with the following" );
    _writer.WriteLine ( $"characteristics:" );
    _writer.WriteLine ( $"  * RR Type:  [TXT]" );
    _writer.WriteLine ( $"  * RR Name:  [{dnsChallenge.RecordName}]" );
    _writer.WriteLine ( $"  * RR Value: [{dnsChallenge.RecordValue}]" );
    _writer.WriteLine ( $"------------------------------------" );
    _writer.Flush ();
   } else if ( httpChallenge != null ) {
    _writer.WriteLine ( $"== Manual Challenge Handler - HTTP ==" );
    _writer.WriteLine ( $"  * CleanUp Time:     [{DateTime.Now}]" );
    _writer.WriteLine ( $"  * Challenge Token:  [{httpChallenge.Token}]" );
    _writer.WriteLine ( $"The Challenge has been completed -- you can now remove the" );
    _writer.WriteLine ( $"file created previously with the following characteristics:" );
    _writer.WriteLine ( $"  * HTTP URL:     [{httpChallenge.FileUrl}]" );
    _writer.WriteLine ( $"  * File Path:    [{httpChallenge.FilePath}]" );
    _writer.WriteLine ( $"  * File Content: [{httpChallenge.FileContent}]" );
    _writer.WriteLine ( $"  * MIME Type:    [text/plain]" );
    _writer.WriteLine ( $"------------------------------------" );
    _writer.Flush ();
   } else {
    var ex = new InvalidOperationException ( "unsupported Challenge type" );
    ex.Data[ "challengeType" ] = c.GetType ();
    throw ex;
   }
  }

  public void Dispose () {
   if ( _stream != null ) {
    try {
     _writer.Dispose ();
     _stream.Dispose ();
    } catch ( Exception ) {
     // TODO: failure to clean up the prior Out
     // should do what???
    }
   }

   IsDisposed = true;
  }

  private void AssertNotDisposed () {
   if ( IsDisposed ) {
    throw new InvalidOperationException ( "Manual Challenge Handler is disposed" );
   }
  }

  #endregion -- Methods --

  #region -- Types --

  private class DebugStream : Stream {
   public override Boolean CanRead => false;

   public override Boolean CanSeek => false;

   public override Boolean CanWrite => true;

   public override Int64 Length => 0;

   public override Int64 Position { get; set; }

   public override void Flush () => Debug.Flush ();

   public override void Write ( Byte[] buffer, Int32 offset, Int32 count ) => Debug.Write ( Encoding.UTF8.GetString ( buffer, offset, count ) );

   public override Int64 Seek ( Int64 offset, SeekOrigin origin ) => throw new NotImplementedException ();

   public override void SetLength ( Int64 value ) => throw new NotImplementedException ();

   public override Int32 Read ( Byte[] buffer, Int32 offset, Int32 count ) => throw new NotImplementedException ();
  }

  #endregion -- Types --
 }
}