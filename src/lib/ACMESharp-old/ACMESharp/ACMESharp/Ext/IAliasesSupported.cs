using System.Collections.Generic;

namespace ACMESharp.Ext {
 public interface IAliasesSupported {
  IEnumerable<System.String> Aliases { get; }
 }
}