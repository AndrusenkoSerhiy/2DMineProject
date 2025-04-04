using System.Collections.Generic;
using Scriptables.Stats;
using Stats;
using UnityEngine;
using StatModifier = Stats.StatModifier;

public class FoodModifiersUI : MonoBehaviour {
  [SerializeField] private ModifierType modifierType;
  [SerializeField] private List<FoodModifierUI> modifiersItems;

  private Dictionary<string, FoodModifierUI> modifiersMap = new();
  private EntityStats playerStats;

  private void Start() {
    playerStats = GameManager.Instance.PlayerController.EntityStats;
    playerStats.Mediator.OnModifierAdded += OnModifierAddedHandler;
    playerStats.Mediator.OnModifierRemoved += OnModifierRemovedHandler;
    FillMap();
  }

  private void FillMap() {
    foreach (var modifierItem in modifiersItems) {
      modifiersMap.Add(modifierItem.Id, modifierItem);
    }
  }

  private void OnModifierAddedHandler(StatModifier modifier) {
    if (!NeedHandleModifier(modifier)) {
      return;
    }

    if (!modifiersMap.TryGetValue(modifier.modifierDisplayObject.Id, out var item)) {
      return;
    }

    item.Show(modifier);
  }

  private void OnModifierRemovedHandler(StatModifier modifier) {
    if (!NeedHandleModifier(modifier)) {
      return;
    }

    if (!modifiersMap.TryGetValue(modifier.modifierDisplayObject.Id, out var item)) {
      return;
    }

    item.Hide();
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