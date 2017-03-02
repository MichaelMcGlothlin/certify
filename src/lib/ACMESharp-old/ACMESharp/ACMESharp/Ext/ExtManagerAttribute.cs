using System;

namespace ACMESharp.Ext {
 /// <summary>
 /// A tagging attribute to identify extension managers
 /// which are typically implemented as static classes.
 /// </summary>
 [AttributeUsage ( AttributeTargets.Class )]
 public class ExtManagerAttribute : Attribute { }
}