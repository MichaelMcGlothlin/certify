using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACMESharp.HTTP {
 /// <summary>
 /// A collection of HTTP header <see cref="Link">Link</see> objects.
 /// </summary>
 /// <remarks>
 /// This collection implements multiple generic-typed enumerable interfaces
 /// but for backward compatibility, the default implemenation, i.e. the
 /// IEnumerable non-generic implementation, returns a sequence of strings
 /// which are the complete, formatted Link values.
 /// </remarks>
 public class LinkCollection : IEnumerable<System.String>, IEnumerable<Link>, ILookup<System.String, System.String> {
  private List<Link> _Links = new List<Link> ();

  public LinkCollection () { }

  public LinkCollection ( IEnumerable<Link> links ) {
   if ( links != null ) {
    foreach ( var l in links ) {
     Add ( l );
    }
   }
  }

  public LinkCollection ( IEnumerable<System.String> linkValues ) {
   if ( linkValues != null ) {
    foreach ( var lv in linkValues ) {
     Add ( new Link ( lv ) );
    }
   }
  }

  public IEnumerable<System.String> this[ System.String key ] => _Links.Where ( x => x.Relation == key ).Select ( x => x.Uri );

  public System.Int32 Count => _Links.Count;

  public void Add ( Link link ) => _Links.Add ( link );

  public Link GetFirstOrDefault ( System.String key ) => _Links.Find ( x => x.Relation == key );

  public System.Boolean Contains ( System.String key ) => GetFirstOrDefault ( key ) != null;

  IEnumerator<Link> IEnumerable<Link>.GetEnumerator () => _Links.GetEnumerator ();

  IEnumerator<System.String> IEnumerable<System.String>.GetEnumerator () {
   foreach ( var l in _Links ) {
    yield return l.Value;
   }
  }

  IEnumerator<IGrouping<System.String, System.String>> IEnumerable<IGrouping<System.String, System.String>>.GetEnumerator () => _Links.ToLookup ( x => x.Relation, x => x.Uri ).GetEnumerator ();

  IEnumerator IEnumerable.GetEnumerator () => ( (IEnumerable<System.String>) this ).GetEnumerator ();

  // We may use this in the future to build a more
  // efficient version of the Lookup interface
  //
  //private class LinkGrouping : IGrouping<string, string>
  //{
  //    public string Relation
  //    { get; set; }
  //
  //    public IEnumerable<Link> Links
  //    { get; set; }
  //
  //    public string Key
  //    {
  //        get
  //        {
  //            return Relation;
  //        }
  //    }
  //
  //    public IEnumerator<string> GetEnumerator()
  //    {
  //        foreach (var l in Links)
  //            yield return l.Uri;
  //    }
  //
  //    IEnumerator IEnumerable.GetEnumerator()
  //    {
  //        foreach (var l in Links)
  //            yield return l.Uri;
  //    }
  //}
 }
}