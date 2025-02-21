using System.Collections.Generic;
using Inventory;
using UnityEngine;

namespace Windows {
  public abstract class InventoryWindowBase : WindowBase {
    [SerializeField] private List<UserInterface> interfaces;

    public override void Hide() {
      base.Hide();
      if (interfaces is not { Count: > 0 }) {
        return;
      }

      foreach (var userInterface in interfaces) {
        userInterface.Save();
      }
    }
  }
}