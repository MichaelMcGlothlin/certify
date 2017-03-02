using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ACMESharp.Messages {
 public class ChallengePart {
  [JsonExtensionData]
  private Dictionary<String, JToken> _expando =
          new Dictionary<String, JToken> ();

  public JToken this[ String name ] {
   get => _expando[ name ]; set => _expando[ name ] = value;
  }

  public String Type { get; set; }

  public String Uri { get; set; }

  public String Token { get; set; }

  public String Status { get; set; }

  public DateTime? Validated { get; set; }

  public IDictionary<String, String> Error { get; set; }
 }

 //public class ChallengePart : Dictionary<string, object>
 //{
 //    public string Type
 //    {
 //        get { return this[nameof(Type)] as string; }
 //        set { this[nameof(Type)] = value; }
 //    }

 //    public string Uri
 //    {
 //        get { return this[nameof(Uri)] as string; }
 //        set { this[nameof(Uri)] = value; }
 //    }

 //    public string Token
 //    {
 //        get { return this[nameof(Token)] as string; }
 //        set { this[nameof(Token)] = value; }
 //    }

 //    public string Status
 //    {
 //        get { return this[nameof(Status)] as string; }
 //        set { this[nameof(Status)] = value; }
 //    }

 //    public DateTime? Validated
 //    {
 //        get { return this[nameof(Validated)] as DateTime?; }
 //        set { this[nameof(Validated)] = value; }
 //    }

 //    public IDictionary<string, string> Error
 //    {
 //        get { return this[nameof(Error)] as IDictionary<string, string>; }
 //        set { this[nameof(Error)] = value; }
 //    }
 //}
}