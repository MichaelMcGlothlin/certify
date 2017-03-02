using System;
using System.ComponentModel.Composition;

namespace ACMESharp.Vault {
 [MetadataAttribute]
 [AttributeUsage ( AttributeTargets.Class, AllowMultiple = false )]
 public class VaultProviderAttribute : ExportAttribute {
  public VaultProviderAttribute ( String name )
      : base ( typeof ( IVaultProvider ) ) => Name = name;

  public String Name { get; private set; }

  public String[] Aliases { get; set; }

  public String Label { get; set; }

  public String Description { get; set; }
 }
}