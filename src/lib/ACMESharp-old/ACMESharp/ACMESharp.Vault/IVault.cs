using ACMESharp.Vault.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace ACMESharp.Vault {
 public interface IVault : IDisposable {

  #region -- Properties --

  Boolean IsDisposed { get; }

  Boolean IsStorageOpen { get; }

  #endregion -- Properties --

  #region -- Methods --

  Boolean TestStorage ();

  void InitStorage ( Boolean force = false );

  void OpenStorage ( Boolean initOrOpen = false );

  VaultInfo LoadVault ( Boolean required = true );

  void SaveVault ( VaultInfo vault );

  IEnumerable<VaultAsset> ListAssets ( String nameRegex = null, params VaultAssetType[] type );

  VaultAsset CreateAsset ( VaultAssetType type, String name, Boolean isSensitive = false,
          Boolean getOrCreate = false );

  VaultAsset GetAsset ( VaultAssetType type, String name );

  Stream SaveAsset ( VaultAsset asset );

  Stream LoadAsset ( VaultAsset asset );

  #endregion -- Methods --
 }
}