using System;
using UnityEngine;

namespace Scriptables {
  public abstract class BaseScriptableObject : ScriptableObject {
    [SerializeField, HideInInspector] private string id = Guid.NewGuid().ToString();
    public string Id => id;

    public void RegenerateId() {
      id = Guid.NewGuid().ToString();
    }

    private void OnValidate() {
      if (string.IsNullOrEmpty(id)) {
        id = Guid.NewGuid().ToString();
      }
    }
  }
}