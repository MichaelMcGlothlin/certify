using System.ComponentModel.Composition.Hosting;

namespace ACMESharp.Ext {
 public interface IExtDetail {
  CompositionContainer CompositionContainer { get; set; }
 }
}