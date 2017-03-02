﻿using ACMESharp.POSH.Util;
using ACMESharp.Vault;
using ACMESharp.Vault.Profile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace ACMESharp.POSH {
 [Cmdlet ( VerbsCommon.Set, "VaultProfile", DefaultParameterSetName = PSET_SET )]
 public class SetVaultProfile : Cmdlet {
  public const String PSET_SET = "Set";
  public const String PSET_REMOVE = "Remove";

  [Parameter ( Mandatory = true, Position = 0 )]
  public String ProfileName { get; set; }

  [Parameter ( Mandatory = true, Position = 1, ParameterSetName = PSET_SET )]
  public String Provider { get; set; }

  [Parameter ( Mandatory = false, Position = 2, ParameterSetName = PSET_SET )]
  public Hashtable VaultParameters { get; set; }

  [Parameter ( Mandatory = false, ParameterSetName = PSET_SET )]
  public Hashtable ProviderParameters { get; set; }

  [Parameter ( Mandatory = true, ParameterSetName = PSET_REMOVE )]
  public SwitchParameter Remove { get; set; }

  [Parameter ( Mandatory = false )]
  public SwitchParameter Force { get; set; }

  protected override void ProcessRecord () {
   IVault existingVault = null;
   var existingProfile = VaultProfileManager.GetProfile ( ProfileName );

   if ( existingProfile != null ) {
    try { existingVault = Util.VaultHelper.GetVault ( ProfileName ); } catch ( Exception ) { }
   }

   if ( Remove ) {
    if ( existingProfile == null ) {
     return;
    }

    if ( !Force && existingVault != null && existingVault.TestStorage () ) {
     throw new InvalidOperationException ( "profile refers to an existing Vault;"
             + " specify -Force to remove anyway" );
    }

    VaultProfileManager.RemoveProfile ( ProfileName );
   } else {
    if ( !Force && existingProfile != null ) {
     throw new InvalidOperationException ( "existing profile found;"
             + " specify -Force to overwrite" );
    }

    var pp = (IReadOnlyDictionary<String, Object>
            ) ProviderParameters.Convert<String, Object> ();
    var vp = (IReadOnlyDictionary<String, Object>
            ) VaultParameters.Convert<String, Object> ();

    VaultProfileManager.SetProfile ( ProfileName, Provider, pp, vp );
   }
  }
 }
}