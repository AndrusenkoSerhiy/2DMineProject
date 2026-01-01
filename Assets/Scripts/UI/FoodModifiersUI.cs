using System.Collections.Generic;
using Scriptables.Stats;
using UI.Objectives;
using UnityEngine;
using StatModifier = Stats.StatModifier;

public class FoodModifiersUI : MonoBehaviour {
  [SerializeField] private ModifierType modifierType;
  [SerializeField] private List<FoodModifierUI> modifiersItems;

  private Dictionary<string, FoodModifierUI> modifiersMap = new();
  private PlayerStats playerStats;
  private bool started;

  private void Start() {
    playerStats = GameManager.Instance.PlayerController.PlayerStats;
    FillMap();
    AddModifiersListeners();
    CheckActiveModifiers();
    started = true;
  }

  private void OnEnable() {
    if (!started) {
      return;
    }

    AddModifiersListeners();
    CheckActiveModifiers();
  }

  private void OnDisable() {
    RemoveModifiersListeners();
    HideAll();
  }

  private void AddModifiersListeners() {
    playerStats.Mediator.OnModifierAdded += OnModifierAddedHandler;
    playerStats.Mediator.OnModifierRemoved += OnModifierRemovedHandler;
  }

  private void RemoveModifiersListeners() {
    if(playerStats == null)
      return;
    
    playerStats.Mediator.OnModifierAdded -= OnModifierAddedHandler;
    playerStats.Mediator.OnModifierRemoved -= OnModifierRemovedHandler;
  }

  private void CheckActiveModifiers() {
    foreach (var modifier in playerStats.Mediator.ListModifiers) {
      if (!NeedHandleModifier(modifier)) {
        continue;
      }

      if (!modifiersMap.TryGetValue(modifier.modifierDisplayObject.Id, out var item)) {
        return;
      }

      item.Show(modifier);
    }
  }

  private void HideAll() {
    foreach (var modifierUI in modifiersMap) {
      modifierUI.Value.Hide();
    }
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
    JournalManager.Instance.UnlockEntry(5);
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
}