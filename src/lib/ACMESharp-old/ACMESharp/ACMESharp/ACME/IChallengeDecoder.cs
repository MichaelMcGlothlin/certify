using ACMESharp.JOSE;
using ACMESharp.Messages;
using System;

namespace ACMESharp.ACME {
 /// <summary>
 /// Defines the Provider interface needed to support discovery
 /// and instance-creation of a <see cref="IChallengeDecoder"
 /// >Challenge Decoder</see>.
 /// </summary>
 public interface IChallengeDecoderProvider // : IDisposable
 {
  Boolean IsSupported ( IdentifierPart ip, ChallengePart cp );

  IChallengeDecoder GetDecoder ( IdentifierPart ip, ChallengePart cp );
 }

 /// <summary>
 /// Defines the interface needed to support implementations of
 /// Challenge Decoder.
 /// </summary>
 /// <remarks>
 /// Challenge Decoders are those components that are able to parse
 /// the Challenge part of a new Authorization response message and
 /// compute the needed elements of a Challenge Response which will
 /// be handled by a <see cref="IChallengeHandler">Challenge Handler</see>.
 /// They are also responsible for computing an <see cref="ChallengeAnswer"
 /// >answer</see> which will be used in computing an answer request
 /// message to be sent by the ACME client to the server once a
 /// Challenge has been handled and satisfied.
 /// </remarks>
 public interface IChallengeDecoder : IDisposable {

  #region -- Properties --

  Boolean IsDisposed { get; }

  #endregion -- Properties --

  #region -- Methods --

  Challenge Decode ( IdentifierPart ip, ChallengePart cp, ISigner signer );

  #endregion -- Methods --
 }
}