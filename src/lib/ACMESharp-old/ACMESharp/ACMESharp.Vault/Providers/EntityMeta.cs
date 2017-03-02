using System;

namespace ACMESharp.Vault.Providers {
 /// <summary>
 /// Basic wrapper around any entity that we save using this file-based
 /// provider in order to track common meta data about the entity.
 /// </summary>
 /// <typeparam name="T"></typeparam>
 public class EntityMeta<T> {
  public DateTime CreateDate { get; set; }

  public String CreateUser { get; set; }

  public String CreateHost { get; set; }

  public DateTime UpdateDate { get; set; }

  public String UpdateUser { get; set; }

  public T Entity { get; set; }
 }
}