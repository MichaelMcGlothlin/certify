namespace ACMESharp.Ext {
 public struct NamedInfo<TInfo> {
  public NamedInfo ( System.String name, TInfo info ) {
   Name = name;
   Info = info;
  }

  public System.String Name { get; private set; }

  public TInfo Info { get; private set; }
 }
}