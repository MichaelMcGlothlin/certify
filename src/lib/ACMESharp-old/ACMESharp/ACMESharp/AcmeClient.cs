using ACMESharp.ACME;
using ACMESharp.HTTP;
using ACMESharp.JOSE;
using ACMESharp.JSON;
using ACMESharp.Messages;
using ACMESharp.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace ACMESharp {
 /// <summary>
 /// The ACME client encapsulates all the protocol rules to interact
 /// with an ACME client as specified by the ACME specficication.
 /// </summary>
 public class AcmeClient : IDisposable {

  #region -- Fields --

  private JsonSerializerSettings _jsonSettings = new JsonSerializerSettings {
   Formatting = Formatting.Indented,
   ContractResolver = new AcmeJsonContractResolver (),
  };

  #endregion -- Fields --

  #region -- Constructors --

  public AcmeClient ( Uri rootUrl = null, AcmeServerDirectory dir = null,
          ISigner signer = null, AcmeRegistration reg = null ) {
   RootUrl = rootUrl;
   Directory = dir;
   Signer = signer;
   Registration = reg;

   UserAgent = String.Format ( AcmeProtocol.HTTP_USER_AGENT_FMT,
           GetType ().Assembly.GetName ().Version );
  }

  #endregion -- Constructors --

  #region -- Properties --

  public String UserAgent { get; private set; }

  public Uri RootUrl { get; set; }

  public IWebProxy Proxy { get; set; }

  public Action<HttpWebRequest> BeforeGetResponseAction { get; set; }

  public AcmeServerDirectory Directory { get; set; }

  public ISigner Signer { get; set; }

  public AcmeRegistration Registration { get; set; }

  public Boolean Initialized { get; private set; }

  public String NextNonce { get; private set; }

  public AcmeHttpResponse LastResponse { get; private set; }

  #endregion -- Properties --

  #region -- Methods --

  public void Init () {
   if ( RootUrl == null ) {
    throw new InvalidOperationException ( "Missing ACME server root URL (RootUrl)" );
   }

   if ( Signer == null ) {
    throw new InvalidOperationException ( "Missing request message signer (Signer)" );
   }

   Directory = Directory ?? new AcmeServerDirectory ();

   // TODO:  according to ACME 5.5 we *should* be able to issue a HEAD
   // request to get an initial replay-nonce, but this is not working,
   // so we do a GET against the root URL to get that initial nonce
   //requ.Method = AcmeProtocol.HTTP_METHOD_HEAD;

   var requUri = new Uri ( RootUrl, Directory[ AcmeServerDirectory.RES_INIT ] );
   var resp = RequestHttpGet ( requUri );

   Initialized = true;
  }

  public void Dispose () => Initialized = false;

  protected void AssertInit () {
   if ( !Initialized ) {
    throw new InvalidOperationException ( "Client is not initialized" );
   }
  }

  protected void AssertRegistration () {
   if ( Registration == null ) {
    throw new InvalidOperationException ( "Client is missing registration info" );
   }
  }

  public AcmeServerDirectory GetDirectory ( Boolean saveRelative = false ) {
   AssertInit ();

   var requUri = new Uri ( RootUrl, Directory[ AcmeServerDirectory.RES_DIRECTORY ] );
   var resp = RequestHttpGet ( requUri );

   var resMap = JObject.Parse ( resp.ContentAsString );
   foreach ( var kv in resMap ) {
    if ( kv.Value.Type == JTokenType.String ) {
     var urlValue = ( kv.Value as JValue ).Value as String;

     if ( saveRelative ) {
      urlValue = new Uri ( urlValue ).PathAndQuery;
     }

     Directory[ kv.Key ] = urlValue;
    }
   }

   return Directory;
  }

  public AcmeRegistration Register ( String[] contacts ) {
   AssertInit ();

   var requMsg = new NewRegRequest {
    Contact = contacts,
   };

   var resp = RequestHttpPost ( new Uri ( RootUrl,
           Directory[ AcmeServerDirectory.RES_NEW_REG ] ), requMsg );

   // HTTP 409 (Conflict) response for a previously registered pub key
   //    Location:  still had the regUri
   if ( resp.IsError ) {
    if ( resp.StatusCode == HttpStatusCode.Conflict ) {
     throw new AcmeWebException ( resp.Error as WebException,
             "Conflict due to previously registered public key", resp );
    } else if ( resp.IsError ) {
     throw new AcmeWebException ( resp.Error as WebException,
             "Unexpected error", resp );
    }
   }

   var regUri = resp.Headers[ AcmeProtocol.HEADER_LOCATION ];
   if ( String.IsNullOrEmpty ( regUri ) ) {
    throw new AcmeException ( "server did not provide a registration URI in the response" );
   }

   var respMsg = JsonConvert.DeserializeObject<RegResponse> ( resp.ContentAsString );

   var newReg = new AcmeRegistration {
    PublicKey = Signer.ExportJwk (),
    RegistrationUri = regUri,
    Contacts = respMsg.Contact,
    Links = resp.Links,
    /// Extracts the "Terms of Service" related link header if there is one and
    /// returns the URI associated with it.  Otherwise returns <c>null</c>.
    TosLinkUri = resp.Links[ AcmeProtocol.LINK_HEADER_REL_TOS ].FirstOrDefault (),
    AuthorizationsUri = respMsg.Authorizations,
    CertificatesUri = respMsg.Certificates,
    TosAgreementUri = respMsg.Agreement,
   };

   Registration = newReg;

   return Registration;
  }

  public AcmeRegistration UpdateRegistration ( Boolean useRootUrl = false, Boolean agreeToTos = false, String[] contacts = null ) {
   AssertInit ();
   AssertRegistration ();

   var requMsg = new UpdateRegRequest ();

   if ( contacts != null ) {
    requMsg.Contact = contacts;
   }

   if ( agreeToTos && !String.IsNullOrWhiteSpace ( Registration.TosLinkUri ) ) {
    requMsg.Agreement = Registration.TosLinkUri;
   }

   // Compute the URL to submit the request to, either exactly as
   // provided in the Registration object or relative to the Root URL
   var requUri = new Uri ( Registration.RegistrationUri );
   if ( useRootUrl ) {
    requUri = new Uri ( RootUrl, requUri.PathAndQuery );
   }

   var resp = RequestHttpPost ( requUri, requMsg );

   if ( resp.IsError ) {
    throw new AcmeWebException ( resp.Error as WebException,
            "Unexpected error", resp );
   }

   var respMsg = JsonConvert.DeserializeObject<RegResponse> ( resp.ContentAsString );

   var updReg = new AcmeRegistration {
    PublicKey = Signer.ExportJwk (),
    RegistrationUri = Registration.RegistrationUri,
    Contacts = respMsg.Contact,
    Links = resp.Links,
    /// Extracts the "Terms of Service" related link header if there is one and
    /// returns the URI associated with it.  Otherwise returns <c>null</c>.
    TosLinkUri = resp.Links[ AcmeProtocol.LINK_HEADER_REL_TOS ].FirstOrDefault (),
    AuthorizationsUri = respMsg.Authorizations,
    CertificatesUri = respMsg.Certificates,
    TosAgreementUri = respMsg.Agreement,
   };

   Registration = updReg;

   return Registration;
  }

  public AuthorizationState AuthorizeIdentifier ( String dnsIdentifier ) {
   AssertInit ();
   AssertRegistration ();

   var requMsg = new NewAuthzRequest {
    Identifier = new IdentifierPart {
     Type = AcmeProtocol.IDENTIFIER_TYPE_DNS,
     Value = dnsIdentifier
    }
   };

   var resp = RequestHttpPost ( new Uri ( RootUrl,
           Directory[ AcmeServerDirectory.RES_NEW_AUTHZ ] ), requMsg );

   if ( resp.IsError ) {
    throw new AcmeWebException ( resp.Error as WebException,
            "Unexpected error", resp );
   }

   var uri = resp.Headers[ AcmeProtocol.HEADER_LOCATION ];
   if ( String.IsNullOrEmpty ( uri ) ) {
    throw new AcmeProtocolException ( "Response is missing an identifier authorization resource URI", resp );
   }

   var respMsg = JsonConvert.DeserializeObject<NewAuthzResponse> ( resp.ContentAsString );

   var authzState = new AuthorizationState {
    IdentifierPart = respMsg.Identifier,
    IdentifierType = respMsg.Identifier.Type,
    Identifier = respMsg.Identifier.Value,
    Uri = uri,
    Status = respMsg.Status,
    Expires = respMsg.Expires,
    Combinations = respMsg.Combinations,

    // Simple copy/conversion from one form to another
    Challenges = respMsg.Challenges.Select ( x => new AuthorizeChallenge {
     ChallengePart = x,
     Type = x.Type,
     Status = x.Status,
     Uri = x.Uri,
     Token = x.Token,
    } ),
   };

   return authzState;
  }

  public AuthorizationState RefreshIdentifierAuthorization ( AuthorizationState authzState, Boolean useRootUrl = false ) {
   AssertInit ();
   AssertRegistration ();

   var requUri = new Uri ( authzState.Uri );
   if ( useRootUrl ) {
    requUri = new Uri ( RootUrl, requUri.PathAndQuery );
   }

   var resp = RequestHttpGet ( requUri );

   if ( resp.IsError ) {
    throw new AcmeWebException ( resp.Error as WebException,
            "Unexpected error", resp );
   }

   var uri = authzState.Uri;
   if ( resp.Headers.AllKeys.Contains ( AcmeProtocol.HEADER_LOCATION ) ) {
    uri = resp.Headers[ AcmeProtocol.HEADER_LOCATION ];
   }

   var respMsg = JsonConvert.DeserializeObject<AuthzStatusResponse> ( resp.ContentAsString );

   var authzStatusState = new AuthorizationState {
    // This is computed above
    Uri = uri,

    // These are updated
    IdentifierPart = respMsg.Identifier,
    IdentifierType = respMsg.Identifier.Type,
    Identifier = respMsg.Identifier.Value,
    Status = respMsg.Status,
    Expires = respMsg.Expires,
    Combinations = respMsg.Combinations,

    Challenges = respMsg.Challenges.Select ( x => new AuthorizeChallenge {
     ChallengePart = x,
     Type = x.Type,
     Status = x.Status,
     Uri = x.Uri,
     Token = x.Token,
    } ),
   };

   return authzStatusState;
  }

  public void RefreshAuthorizeChallenge ( AuthorizationState authzState, String type, Boolean useRootUrl = false ) {
   AssertInit ();
   AssertRegistration ();

   var c = authzState.Challenges.FirstOrDefault ( x => x.Type == type );
   if ( c == null ) {
    throw new ArgumentOutOfRangeException ( nameof ( type ), "no challenge found matching requested type" )
            .With ( nameof ( type ), type );
   }

   var requUri = new Uri ( c.Uri );
   if ( useRootUrl ) {
    requUri = new Uri ( RootUrl, requUri.PathAndQuery );
   }

   var resp = RequestHttpGet ( requUri );
   var cp = JsonConvert.DeserializeObject<ChallengePart> ( resp.ContentAsString );

   c.Type = cp.Type;
   c.Uri = cp.Uri;
   c.Token = cp.Token;
   c.Status = cp.Status;
  }

  [Obsolete]
  public AuthorizeChallenge GenerateAuthorizeChallengeAnswer ( AuthorizationState authzState, String type ) {
   AssertInit ();
   AssertRegistration ();

   var c = authzState.Challenges.FirstOrDefault ( x => x.Type == type );
   if ( c == null ) {
    throw new ArgumentOutOfRangeException ( nameof ( type ),
            "no challenge found matching requested type" );
   }

   switch ( type ) {
    case AcmeProtocol.CHALLENGE_TYPE_DNS:
     c.OldChallengeAnswer = c.GenerateDnsChallengeAnswer ( authzState.Identifier, Signer );
     c.ChallengeAnswerMessage = new AnswerDnsChallengeRequest {
      KeyAuthorization = c.OldChallengeAnswer.Value,
     };
     break;

    case AcmeProtocol.CHALLENGE_TYPE_HTTP:
     c.OldChallengeAnswer = c.GenerateHttpChallengeAnswer ( authzState.Identifier, Signer );
     c.ChallengeAnswerMessage = new AnswerHttpChallengeRequest {
      KeyAuthorization = c.OldChallengeAnswer.Value,
     };
     break;

    case AcmeProtocol.CHALLENGE_TYPE_SNI:
    case AcmeProtocol.CHALLENGE_TYPE_PRIORKEY:
     throw new ArgumentException ( "unimplemented or unsupported challenge type", nameof ( type ) );

    default:
     throw new ArgumentException ( "unknown challenge type", nameof ( type ) );
   }

   return c;
  }

  public AuthorizeChallenge DecodeChallenge ( AuthorizationState authzState, String challengeType ) {
   AssertInit ();
   AssertRegistration ();

   var authzChallenge = authzState.Challenges.FirstOrDefault ( x => x.Type == challengeType );
   if ( authzChallenge == null ) {
    throw new ArgumentOutOfRangeException ( nameof ( challengeType ),
            "no challenge found matching requested type" )
            .With ( "challengeType", challengeType );
   }

   var provider = ChallengeDecoderExtManager.GetProvider ( challengeType );
   if ( provider == null ) {
    throw new NotSupportedException ( "no provider exists for requested challenge type" )
            .With ( "challengeType", challengeType );
   }

   using ( var decoder = provider.GetDecoder ( authzState.IdentifierPart, authzChallenge.ChallengePart ) ) {
    authzChallenge.Challenge = decoder.Decode ( authzState.IdentifierPart, authzChallenge.ChallengePart, Signer );

    if ( authzChallenge.Challenge == null ) {
     throw new InvalidDataException ( "challenge decoder produced no output" );
    }
   }

   return authzChallenge;
  }

  public AuthorizeChallenge HandleChallenge ( AuthorizationState authzState,
          String challengeType,
          String handlerName, IReadOnlyDictionary<String, Object> handlerParams,
          Boolean cleanUp = false ) {
   var provider = ChallengeHandlerExtManager.GetProvider ( handlerName );
   if ( provider == null ) {
    throw new InvalidOperationException ( "unable to resolve Challenge Handler provider" )
            .With ( "handlerName", handlerName );
   }

   var authzChallenge = authzState.Challenges.FirstOrDefault ( x => x.Type == challengeType );
   if ( authzChallenge == null ) {
    throw new ArgumentOutOfRangeException ( nameof ( challengeType ),
            "no challenge found matching requested type" )
            .With ( "challengeType", challengeType );
   }

   if ( !provider.IsSupported ( authzChallenge.Challenge ) ) {
    throw new InvalidOperationException ( "Challenge Handler does not support given Challenge" )
            .With ( "handlerName", handlerName )
            .With ( "challengeType", authzChallenge.Challenge.Type );
   }

   var handler = provider.GetHandler ( authzChallenge.Challenge, handlerParams );
   if ( handler == null ) {
    throw new InvalidOperationException ( "no Challenge Handler provided for given Challenge" )
            .With ( "handlerName", handlerName )
            .With ( "challengeType", authzChallenge.Challenge.Type );
   }

   authzChallenge.HandlerName = handlerName;

   if ( cleanUp ) {
    handler.CleanUp ( authzChallenge.Challenge );
    authzChallenge.HandlerCleanUpDate = DateTime.Now;
   } else {
    handler.Handle ( authzChallenge.Challenge );
    authzChallenge.HandlerHandleDate = DateTime.Now;
   }

   handler.Dispose ();

   return authzChallenge;
  }

  public AuthorizeChallenge SubmitChallengeAnswer ( AuthorizationState authzState,
          String type, Boolean useRootUrl = false ) {
   AssertInit ();
   AssertRegistration ();

   var authzChallenge = authzState.Challenges.FirstOrDefault ( x => x.Type == type );
   if ( authzChallenge == null ) {
    throw new ArgumentException ( "no challenge found matching requested type" );
   }

   if ( authzChallenge.Challenge == null ) {
    throw new InvalidOperationException ( "challenge has not been decoded" );
   }

   if ( authzChallenge.Challenge.Answer == null ) {
    throw new InvalidOperationException ( "challenge answer has not been generated" );
   }

   var requUri = new Uri ( authzChallenge.Uri );
   if ( useRootUrl ) {
    requUri = new Uri ( RootUrl, requUri.PathAndQuery );
   }

   var requ = ChallengeAnswerRequest.CreateRequest ( authzChallenge.Challenge.Answer );
   var resp = RequestHttpPost ( requUri, requ );

   if ( resp.IsError ) {
    throw new AcmeWebException ( resp.Error as WebException,
            "Unexpected error", resp );
   }

   authzChallenge.SubmitResponse = resp;
   authzChallenge.SubmitDate = DateTime.Now;

   return authzChallenge;
  }

  [Obsolete]
  public AuthorizeChallenge SubmitAuthorizeChallengeAnswer ( AuthorizationState authzState,
          String challengeType, Boolean useRootUrl = false ) {
   AssertInit ();
   AssertRegistration ();

   var authzChallenge = authzState.Challenges.FirstOrDefault ( x => x.Type == challengeType );
   if ( authzChallenge == null ) {
    throw new ArgumentException ( "no challenge found matching requested type" );
   }

   if ( authzChallenge.OldChallengeAnswer.Key == null
           || authzChallenge.OldChallengeAnswer.Value == null
           || authzChallenge.ChallengeAnswerMessage == null ) {
    throw new InvalidOperationException ( "challenge answer has not been generated" );
   }

   var requUri = new Uri ( authzChallenge.Uri );
   if ( useRootUrl ) {
    requUri = new Uri ( RootUrl, requUri.PathAndQuery );
   }

   var resp = RequestHttpPost ( requUri, authzChallenge.ChallengeAnswerMessage );

   if ( resp.IsError ) {
    throw new AcmeWebException ( resp.Error as WebException,
            "Unexpected error", resp );
   }

   authzChallenge.HandlerHandleDate = DateTime.Now;

   return authzChallenge;
  }

  public CertificateRequest RequestCertificate ( String csrContent ) {
   AssertInit ();
   AssertRegistration ();

   var requMsg = new NewCertRequest {
    Csr = csrContent
   };

   var resp = RequestHttpPost ( new Uri ( RootUrl,
           Directory[ AcmeServerDirectory.RES_NEW_CERT ] ), requMsg );

   if ( resp.IsError ) {
    throw new AcmeWebException ( resp.Error as WebException,
            "Unexpected error", resp );
   }

   if ( resp.StatusCode != HttpStatusCode.Created ) {
    throw new AcmeProtocolException ( "Unexpected response status code", resp );
   }

   var uri = resp.Headers[ AcmeProtocol.HEADER_LOCATION ];
   if ( String.IsNullOrEmpty ( uri ) ) {
    throw new AcmeProtocolException ( "Response is missing a certificate resource URI", resp );
   }

   // This may be available immediately or it may need to be requeried for
   var certRequ = new CertificateRequest {
    StatusCode = resp.StatusCode,
    CsrContent = csrContent,
    Uri = uri,
    Links = resp.Links,
   };
   certRequ.SetCertificateContent ( resp.RawContent );

   return certRequ;
  }

  public void RefreshCertificateRequest ( CertificateRequest certRequ, Boolean useRootUrl = false ) {
   AssertInit ();
   AssertRegistration ();

   var requUri = new Uri ( certRequ.Uri );
   if ( useRootUrl ) {
    requUri = new Uri ( RootUrl, requUri.PathAndQuery );
   }

   var acmeResp = RequestHttpGet ( requUri );

   if ( acmeResp.StatusCode != HttpStatusCode.OK && acmeResp.StatusCode != HttpStatusCode.Accepted ) {
    throw new AcmeProtocolException ( "Unexpected response status code", acmeResp );
   }

   certRequ.StatusCode = acmeResp.StatusCode;
   certRequ.Links = acmeResp.Links;
   certRequ.SetCertificateContent ( acmeResp.RawContent );
   certRequ.RetryAfter = null;

   var certContent = acmeResp.RawContent;
   var retryAfter = acmeResp.Headers[ AcmeProtocol.HEADER_RETRY_AFTER ];
   if ( !String.IsNullOrEmpty ( retryAfter ) ) {
    // According to spec (http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.37)
    // this could be a number of seconds or a date, so we have to parse appropriately
    if ( Regex.IsMatch ( retryAfter, "[0-9]+" ) ) {
     certRequ.RetryAfter = DateTime.Now.AddSeconds ( Int32.Parse ( retryAfter ) );
    } else {
     certRequ.RetryAfter = DateTime.Parse ( retryAfter );
    }
   }
  }

  private AcmeHttpResponse RequestHttpGet ( Uri uri ) {
   var requ = (HttpWebRequest) WebRequest.Create ( uri );
   if ( Proxy != null ) {
    requ.Proxy = Proxy;
   }

   requ.Method = AcmeProtocol.HTTP_METHOD_GET;
   requ.UserAgent = UserAgent;

   try {
    BeforeGetResponseAction?.Invoke ( requ );

    using ( var resp = (HttpWebResponse) requ.GetResponse () ) {
     ExtractNonce ( resp );
     var acmeResp = new AcmeHttpResponse ( resp );
     LastResponse = acmeResp;
     return acmeResp;
    }
   } catch ( WebException ex ) when ( ex.Response != null ) {
    using ( var resp = (HttpWebResponse) ex.Response ) {
     var acmeResp = new AcmeHttpResponse ( resp ) {
      IsError = true,
      Error = ex,
     };
     LastResponse = acmeResp;

     if ( ProblemDetailResponse.CONTENT_TYPE == resp.ContentType
             && !String.IsNullOrEmpty ( acmeResp.ContentAsString ) ) {
      acmeResp.ProblemDetail = JsonConvert.DeserializeObject<ProblemDetailResponse> (
              acmeResp.ContentAsString );
     }

     return acmeResp;
    }
   }
  }

  /// <summary>
  /// Submits an ACME protocol request via an HTTP POST with the necessary semantics
  /// and protocol details.  The result is a simplified and canonicalized response
  /// object capturing the error state, HTTP response headers and content of the
  /// response body.
  /// </summary>
  /// <param name="uri"></param>
  /// <param name="message"></param>
  /// <returns></returns>
  private AcmeHttpResponse RequestHttpPost ( Uri uri, Object message ) {
   var acmeSigned = ComputeAcmeSigned ( message, Signer );
   var acmeBytes = Encoding.ASCII.GetBytes ( acmeSigned );

   var requ = (HttpWebRequest) WebRequest.Create ( uri );
   if ( Proxy != null ) {
    requ.Proxy = Proxy;
   }

   requ.Method = AcmeProtocol.HTTP_METHOD_POST;
   requ.ContentType = AcmeProtocol.HTTP_CONTENT_TYPE_JSON;
   requ.ContentLength = acmeBytes.Length;
   requ.UserAgent = UserAgent;
   try {
    BeforeGetResponseAction?.Invoke ( requ );

    using ( var s = requ.GetRequestStream () ) {
     s.Write ( acmeBytes, 0, acmeBytes.Length );
    }

    using ( var resp = (HttpWebResponse) requ.GetResponse () ) {
     ExtractNonce ( resp );
     var acmeResp = new AcmeHttpResponse ( resp );
     LastResponse = acmeResp;
     return acmeResp;
    }
   } catch ( WebException ex ) when ( ex.Response != null ) {
    using ( var resp = (HttpWebResponse) ex.Response ) {
     var acmeResp = new AcmeHttpResponse ( resp ) {
      IsError = true,
      Error = ex,
     };
     LastResponse = acmeResp;

     if ( ProblemDetailResponse.CONTENT_TYPE == resp.ContentType
             && !String.IsNullOrEmpty ( acmeResp.ContentAsString ) ) {
      acmeResp.ProblemDetail = JsonConvert.DeserializeObject<ProblemDetailResponse> (
              acmeResp.ContentAsString );
      acmeResp.ProblemDetail.OrignalContent = acmeResp.ContentAsString;
     }

     return acmeResp;
    }
   }
  }

  /// <summary>
  /// Computes the JWS-signed ACME request body for the given message object
  /// and signer instance.
  /// </summary>
  /// <param name="message"></param>
  /// <param name="signer"></param>
  /// <returns></returns>
  private String ComputeAcmeSigned ( Object message, ISigner signer ) {
   var protectedHeader = new {
    nonce = NextNonce
   };
   var unprotectedHeader = new {
    alg = Signer.JwsAlg,
    jwk = Signer.ExportJwk ()
   };

   var payload = String.Empty;
   if ( message is JObject ) {
    payload = ( (JObject) message ).ToString ( Formatting.None );
   } else {
    payload = JsonConvert.SerializeObject ( message, Formatting.None );
   }

   var acmeSigned = JwsHelper.SignFlatJson ( Signer.Sign, payload,
           protectedHeader, unprotectedHeader );

   return acmeSigned;
  }

  /// <summary>
  /// Extracts the next ACME protocol nonce from the argument Web response
  /// and remembers it for the next protocol request.
  /// </summary>
  /// <param name="resp"></param>
  private void ExtractNonce ( WebResponse resp ) {
   var nonceHeader = Array.Find ( resp.Headers.AllKeys, x => x.Equals ( AcmeProtocol.HEADER_REPLAY_NONCE, StringComparison.OrdinalIgnoreCase ) );
   if ( String.IsNullOrEmpty ( nonceHeader ) ) {
    throw new AcmeException ( "Missing initial replay-nonce header" );
   }

   NextNonce = resp.Headers[ nonceHeader ];
   if ( String.IsNullOrEmpty ( NextNonce ) ) {
    throw new AcmeException ( "Missing initial replay-nonce header value" );
   }
  }

  #endregion -- Methods --

  #region -- Nested Types --

  public class AcmeHttpResponse {
   private String _ContentAsString;

   public AcmeHttpResponse ( HttpWebResponse resp ) {
    StatusCode = resp.StatusCode;
    Headers = resp.Headers;
    Links = new LinkCollection ( Headers.GetValues ( AcmeProtocol.HEADER_LINK ) );

    var rs = resp.GetResponseStream ();
    using ( var ms = new MemoryStream () ) {
     rs.CopyTo ( ms );
     RawContent = ms.ToArray ();
    }
   }

   public HttpStatusCode StatusCode { get; set; }

   public WebHeaderCollection Headers { get; set; }

   public LinkCollection Links { get; set; }

   public Byte[] RawContent { get; set; }

   public String ContentAsString {
    get {
     if ( _ContentAsString == null ) {
      if ( RawContent == null || RawContent.Length == 0 ) {
       return null;
      }

      using ( var ms = new MemoryStream ( RawContent ) ) {
       using ( var sr = new StreamReader ( ms ) ) {
        _ContentAsString = sr.ReadToEnd ();
       }
      }
     }
     return _ContentAsString;
    }
   }

   public Boolean IsError { get; set; }

   public Exception Error { get; set; }

   public ProblemDetailResponse ProblemDetail { get; set; }
  }

  public class AcmeWebException : AcmeException {
   public AcmeWebException ( WebException innerException, String message = null,
           AcmeHttpResponse response = null ) : base ( message, innerException ) {
    Response = response;
    if ( Response?.ProblemDetail?.OrignalContent != null ) {
     this.With ( nameof ( Response.ProblemDetail ),
             Response.ProblemDetail.OrignalContent );
    }
   }

   protected AcmeWebException ( SerializationInfo info, StreamingContext context ) : base ( info, context ) { }

   public WebException WebException => InnerException as WebException;

   public AcmeHttpResponse Response { get; private set; }

   public override String Message {
    get {
     if ( Response != null ) {
      return base.Message + "\n +Response from server:\n\t+ Code: " + Response.StatusCode.ToString () + "\n\t+ Content: " + Response.ContentAsString;
     } else {
      return base.Message + "\n +No response from server";
     }
    }
   }
  }

  public class AcmeProtocolException : AcmeException {
   public AcmeProtocolException ( String message, AcmeHttpResponse response = null,
           Exception innerException = null ) : base ( message, innerException ) => Response = response;

   protected AcmeProtocolException ( SerializationInfo info, StreamingContext context ) : base ( info, context ) { }

   public AcmeHttpResponse Response { get; private set; }

   public override String Message {
    get {
     if ( Response != null ) {
      return base.Message + "\n +Response from server:\n\t+ Code: " + Response.StatusCode.ToString () + "\n\t+ Content: " + Response.ContentAsString;
     } else {
      return base.Message + "\n +No response from server";
     }
    }
   }
  }

  #endregion -- Nested Types --
 }
}