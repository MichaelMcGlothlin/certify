using ACMESharp.Ext;
using System;
using System.Collections.Generic;

namespace ACMESharp.ACME {
 /// <summary>
 /// Defines the Provider interface needed to support discovery
 /// and instance-creation of a <see cref="IChallengeHandler"
 /// >Challenge Handler</see>.
 /// </summary>
 public interface IChallengeHandlerProvider // : IDisposable
 {
  IEnumerable<ParameterDetail> DescribeParameters ();

  Boolean IsSupported ( Challenge c );

  IChallengeHandler GetHandler ( Challenge c, IReadOnlyDictionary<String, Object> initParams );
 }

 /// <summary>
 /// Defines the interface needed to support implementations of
 /// Challenge Handlers.
 /// </summary>
 /// <remarks>
 /// Challenge Handlers are those components that are able to satisfy
 /// the Challenges issued by an ACME server as part of a request to
 /// Authorize an Identifier.
 /// </remarks>
 public interface IChallengeHandler : IDisposable {

  #region -- Properties --

  Boolean IsDisposed { get; }

  #endregion -- Properties --

  #region -- Methods --

  void Handle ( Challenge c );

  void CleanUp ( Challenge c );

  #endregion -- Methods --
 }
}