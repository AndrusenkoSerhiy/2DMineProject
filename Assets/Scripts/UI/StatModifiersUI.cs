using System.Collections.Generic;
using System.Linq;
using Scriptables.Stats;
using Stats;
using UnityEngine;
using StatModifier = Stats.StatModifier;

public class StatModifiersUI : MonoBehaviour {
  [SerializeField] private ModifierType modifierType;
  [SerializeField] private List<StatModifierUI> freeModifiersItems;
  [SerializeField] private GameObject modifierPrefab;

  private Dictionary<string, StatModifierUI> activeModifiers = new();
  private PlayerStats playerStats;

  private void Start() {
    playerStats = GameManager.Instance.PlayerController.PlayerStats;
    playerStats.Mediator.OnModifierAdded += OnModifierAddedHandler;
    playerStats.Mediator.OnModifierRemoved += OnModifierRemovedHandler;
  }

  private void OnModifierAddedHandler(StatModifier modifier) {
    if (!NeedHandleModifier(modifier)) {
      return;
    }

    var item = GetFreeModifierItem();
    activeModifiers.Add(modifier.Id, item);
    SetPosition(item);
    item.Show(modifier);
  }

  protected virtual void SetPosition(StatModifierUI item) {
    item.transform.SetSiblingIndex(activeModifiers.Count - 1);
  }

  private void OnModifierRemovedHandler(StatModifier modifier) {
    if (!NeedHandleModifier(modifier)) {
      return;
    }

    if (!activeModifiers.TryGetValue(modifier.Id, out var item)) {
      return;
    }

    item.Hide();
    freeModifiersItems.Add(item);
    activeModifiers.Remove(modifier.Id);
  }

  private StatModifierUI GetFreeModifierItem() {
    var item = freeModifiersItems.LastOrDefault();
    if (item) {
      freeModifiersItems.Remove(item);
      return item;
    }

    var modifierItem = Instantiate(modifierPrefab, transform).GetComponent<StatModifierUI>();
    return modifierItem;
  }

  private bool NeedHandleModifier(StatModifier modifier) {
    if (!modifier.modifierDisplayObject || modifier.modifierDisplayObject.modifierType != modifierType) {
      return false;
    }

    return modifier.modifierDisplayObject.display != null;
  }

  private void OnDisable() {
    playerStats.Mediator.OnModifierAdded -= OnModifierAddedHandler;
    playerStats.Mediator.OnModifierRemoved -= OnModifierRemovedHandler;
  }
}