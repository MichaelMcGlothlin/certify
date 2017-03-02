using System;
using System.ComponentModel.Composition;

namespace ACMESharp.PKI {
 [MetadataAttribute]
 [AttributeUsage ( AttributeTargets.Class, AllowMultiple = false )]
 public class PkiToolProviderAttribute : ExportAttribute {
  public PkiToolProviderAttribute ( String name )
      : base ( typeof ( IPkiToolProvider ) ) => Name = name;

  public String Name { get; private set; }

  public String[] Aliases { get; set; }

  public String Label { get; set; }

  public String Description { get; set; }
 }
}