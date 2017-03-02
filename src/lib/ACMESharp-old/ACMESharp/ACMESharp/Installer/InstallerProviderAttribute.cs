using System;
using System.ComponentModel.Composition;

namespace ACMESharp.Installer {
 [MetadataAttribute]
 [AttributeUsage ( AttributeTargets.Class, AllowMultiple = false )]
 public class InstallerProviderAttribute : ExportAttribute {
  public InstallerProviderAttribute ( String name )
      : base ( typeof ( IInstallerProvider ) ) => Name = name;

  public String Name { get; private set; }

  public String[] Aliases { get; set; }

  public String Label { get; set; }

  public String Description { get; set; }

  public Boolean IsUninstallSupported { get; set; }
 }
}