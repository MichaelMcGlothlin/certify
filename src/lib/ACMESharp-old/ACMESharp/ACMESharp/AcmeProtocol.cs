namespace ACMESharp {
 public static class AcmeProtocol {

  #region -- Constants --

  public const System.String HTTP_METHOD_HEAD = "HEAD";
  public const System.String HTTP_METHOD_GET = "GET";
  public const System.String HTTP_METHOD_POST = "POST";
  public const System.String HTTP_CONTENT_TYPE_JSON = "application/json";
  public const System.String HTTP_USER_AGENT_FMT = "ACMEdotNET v{0} (ACME 1.0)";

  public const System.String HEADER_REPLAY_NONCE = "Replay-nonce";
  public const System.String HEADER_LOCATION = "Location";
  public const System.String HEADER_LINK = "Link";
  public const System.String HEADER_RETRY_AFTER = "Retry-After";

  /// <summary>
  /// Identifier type indicator indicator for
  /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-01#section-5.3"
  /// >fully-qualified domain name (DNS)</see>.
  /// </summary>
  public const System.String IDENTIFIER_TYPE_DNS = "dns";

  /// <summary>
  /// Identifier validation challenge type indicator for
  /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-01#section-7.5"
  /// >DNS</see>.
  /// </summary>
  public const System.String CHALLENGE_TYPE_DNS = "dns-01";

  /// <summary>
  /// Identifier validation challenge type indicator for
  /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-01#section-7.2"
  /// >HTTP (non-SSL/TLS)</see>.
  /// </summary>
  public const System.String CHALLENGE_TYPE_HTTP = "http-01";

  /// <summary>
  /// Identifier validation challenge type indicator for
  /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-01#section-7.3"
  /// >TLS SNI</see>.  Currently UNSUPPORTED.
  /// </summary>
  public const System.String CHALLENGE_TYPE_SNI = "tls-sni-01";

  /// <summary>
  /// Identifier validation challenge type indicator for
  /// <see cref="https://tools.ietf.org/html/draft-ietf-acme-acme-01#section-7.4"
  /// >Proof of Possession of a Prior Key</see>.  Currently UNSUPPORTED.
  /// </summary>
  public const System.String CHALLENGE_TYPE_PRIORKEY = "proofOfPossession-01";

  public const System.String DNS_CHALLENGE_NAMEPREFIX = "_acme-challenge.";

  public const System.String DNS_CHALLENGE_RECORDTYPE = "TXT";

  public const System.String HTTP_CHALLENGE_PATHPREFIX = ".well-known/acme-challenge/";

  /// <summary>
  /// The relation name for the "Terms of Service" related link header.
  /// </summary>
  /// <remarks>
  /// Link headers can be returned as part of a registration:
  ///   HTTP/1.1 201 Created
  ///   Content-Type: application/json
  ///   Location: https://example.com/acme/reg/asdf
  ///   Link: <https://example.com/acme/new-authz>;rel="next"
  ///   Link: <https://example.com/acme/recover-reg>;rel="recover"
  ///   Link: <https://example.com/acme/terms>;rel="terms-of-service"
  ///
  /// The "terms-of-service" URI should be included in the "agreement" field
  /// in a subsequent registration update
  /// </remarks>
  public const System.String LINK_HEADER_REL_TOS = "terms-of-service";

  #endregion -- Constants --
 }
}