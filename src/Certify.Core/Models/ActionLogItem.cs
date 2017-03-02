using System;

namespace Certify.Models {
 public class ActionLogItem {
  public DateTime DateTime { get; set; }

  public String Command { get; set; }

  public String Result { get; set; }

  public override String ToString () => "[" + DateTime.ToShortTimeString () + "] " + Command + ( Result == null ? String.Empty : " : " + Result );
 }
}