using System;
using System.Runtime.Serialization;

namespace ACMESharp
{
 [Serializable]
 public class AcmeException : Exception
 {
  public AcmeException()
  {
  }

  public AcmeException(String message) : base(message)
  {
  }

  public AcmeException(String message, Exception innerException) : base(message, innerException)
  {
  }

  protected AcmeException(SerializationInfo info, StreamingContext context) : base(info, context)
  {
  }
 }
}