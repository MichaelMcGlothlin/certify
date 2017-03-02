using Newtonsoft.Json.Serialization;

namespace ACMESharp.JSON {
 public class AcmeJsonContractResolver : CamelCasePropertyNamesContractResolver // DefaultContractResolver
 {
  protected override System.String ResolvePropertyName ( System.String propertyName ) {
   var propName = base.ResolvePropertyName ( propertyName );
   if ( !System.String.IsNullOrWhiteSpace ( propName ) && System.Char.IsUpper ( propName[ 0 ] ) ) {
    var propNameChars = propName.ToCharArray ();
    propNameChars[ 0 ] = System.Char.ToLower ( propNameChars[ 0 ] );
    propName = new System.String ( propNameChars );
   }

   return propName;
  }
 }
}