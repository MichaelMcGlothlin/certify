using System;
using System.Collections.Generic;

namespace ACMESharp.Messages {
 public class AuthzStatusResponse {
  public String Status { get; set; }

  public DateTime? Expires { get; set; }

  public IdentifierPart Identifier { get; set; }

  public IEnumerable<ChallengePart> Challenges { get; set; }

  public IEnumerable<IEnumerable<Int32>> Combinations { get; set; }
 }
}