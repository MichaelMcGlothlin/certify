namespace ACMESharp.Ext {
 /// <summary>
 /// Defines meta data used to describe parameters that may be provided for
 /// various stages and entry points in the extension mechanisms.
 /// </summary>
 public class ParameterDetail {
  public ParameterDetail ( System.String name, ParameterType type,
          System.Boolean isRequired = false, System.Boolean isMultiValued = false,
          System.String label = null, System.String desc = null ) {
   Name = name;
   Type = type;
   IsRequired = isRequired;
   IsMultiValued = isMultiValued;
   Label = label;
   Description = desc;
  }

  public System.String Name { get; private set; }

  public ParameterType Type { get; private set; }

  public System.Boolean IsRequired { get; private set; }

  public System.Boolean IsMultiValued { get; private set; }

  public System.String Label { get; private set; }

  public System.String Description { get; private set; }
 }

 /// <summary>
 /// Defines the different logical types that are supported
 /// for parameters in the extension mechanisms.
 /// </summary>
 public enum ParameterType {
  TEXT = 0x1,
  NUMBER = 0x2,
  BOOLEAN = 0x3,

  // TODO:  We were going to support a Secret type
  // that would be passed around as a SecuritString
  // instance, but looking into the future, it looks
  // like that might be going away because of the
  // complexity of supporting it on multiple platforms:
  //    https://github.com/dotnet/corefx/issues/1387
  //SECRET = 0xA,

  /// <summary>
  /// Key-Value pair, with a Text value (and key).
  /// </summary>
  KVP_TEXT = 0x10,
 }
}