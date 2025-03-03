using System;
using UnityEngine;

namespace Scriptables {
  public abstract class BaseScriptableObject : ScriptableObject {
    [SerializeField, HideInInspector] private string id;
    public string Id => id;

    public void RegenerateId() {
      id = Guid.NewGuid().ToString();
    }
  }
}