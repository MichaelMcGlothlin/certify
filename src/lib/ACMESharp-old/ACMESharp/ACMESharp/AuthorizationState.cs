using ACMESharp.Messages;
using ACMESharp.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace ACMESharp {
 public class AuthorizationState {
  public const String STATUS_PENDING = "pending";
  public const String STATUS_PROCESSING = "processing";
  public const String STATUS_VALID = "valid";
  public const String STATUS_INVALID = "invalid";
  public const String STATUS_REVOKED = "revoked";

  public IdentifierPart IdentifierPart { get; set; }

  public String IdentifierType { get; set; }

  public String Identifier { get; set; }

  public String Uri { get; set; }

  public String Status { get; set; }

  public DateTime? Expires { get; set; }

  public IEnumerable<AuthorizeChallenge> Challenges { get; set; }

  public IEnumerable<IEnumerable<Int32>> Combinations { get; set; }

  public Boolean IsPending () => String.IsNullOrEmpty ( Status ) || String.Equals ( Status, STATUS_PENDING,
           StringComparison.InvariantCultureIgnoreCase );

  public void Save ( Stream s ) => JsonHelper.Save ( s, this );

  public static AuthorizationState Load ( Stream s ) => JsonHelper.Load<AuthorizationState> ( s );
 }
}