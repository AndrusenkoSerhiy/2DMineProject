using System;
using System.Collections.Generic;
using System.Linq;
using SaveSystem;
using Scriptables.Items;
using Scriptables.Stats;
using UnityEngine;
using Modifier = Scriptables.Stats.StatModifier;

namespace Stats {
  public class StatsMediator {
    private readonly List<StatModifier> listModifiers = new();
    private readonly Dictionary<StatType, IEnumerable<StatModifier>> modifiersCache = new();
    private readonly IStatModifierApplicationOrder order = new NormalStatModifierOrder();
    private StatsBase stats;

    public event Action<StatModifier> OnModifierAdded = delegate { };
    public event Action<StatModifier> OnModifierRemoved = delegate { };
    public List<StatModifier> ListModifiers => listModifiers;

    public void SetStats(StatsBase playerStats) {
      stats = playerStats;
    }

    public float Calculate(StatType type, float value) {
      if (!modifiersCache.ContainsKey(type)) {
        modifiersCache[type] = listModifiers.Where(modifier => modifier.Type == type).ToList();
      }

      return order.Apply(modifiersCache[type], value);
    }

    private void InvalidateCache(StatType statType) {
      modifiersCache.Remove(statType);
    }

    public void Update(float deltaTime) {
      if (listModifiers.Count == 0) {
        return;
      }

      var toRemove = new List<StatModifier>();

      foreach (var modifier in listModifiers) {
        modifier.Update(deltaTime);

        if (modifier.MarkedForRemoval) {
          toRemove.Add(modifier);
        }
      }

      if (toRemove.Count == 0) {
        return;
      }

      foreach (var modifier in toRemove) {
        modifier.Dispose();
      }
    }

    public bool ApplyModifiers(ApplyType applyType, ItemObject itemObject) {
      if (!CanApplyModifier(applyType, itemObject)) {
        GameManager.Instance.MessagesManager.ShowSimpleMessage("Can't use this item now.");
        return false;
      }

      var statFactory = GameManager.Instance.StatModifierFactory;

      foreach (var statModifier in itemObject.statModifiers) {
        if (statModifier.applyType != applyType) {
          continue;
        }

        var modifier = statFactory.Create(statModifier, itemObject.Id);

        if (statModifier.permanent) {
          stats.UpdateClearValueByModifier(modifier);
          continue;
        }

        //remove previous modifiers
        RemovePrevious(statModifier);
        AddModifier(modifier);
      }

      return true;
    }

    public void Load(List<StatModifierData> data) {
      var statFactory = GameManager.Instance.StatModifierFactory;

      foreach (var statModifierData in data) {
        var modifierObject = statModifierData.ModifierDisplayObjectId != string.Empty
          ? GameManager.Instance.ModifiersDatabase.ItemsMap[statModifierData.ModifierDisplayObjectId]
          : null;
        var statModifier = new Modifier {
          type = statModifierData.Type,
          applyType = statModifierData.ApplyType,
          operatorType = statModifierData.OperatorType,
          value = statModifierData.Value,
          duration = statModifierData.Duration,
          timeLeft = statModifierData.TimeLeft,
          modifierDisplayObject = modifierObject
        };

        var modifier = statFactory.Create(statModifier, statModifierData.ItemId);
        AddModifier(modifier);
      }
    }

    private bool CanApplyModifier(ApplyType applyType, ItemObject itemObject) {
      if (itemObject == null || itemObject.statModifiers == null) {
        return false;
      }

      var statModifiersByApplyType = itemObject.statModifiers.FindAll(modifier => modifier.applyType == applyType);
      if (statModifiersByApplyType.Count == 0) {
        return false;
      }

      foreach (var modifier in statModifiersByApplyType) {
        //Value of current stat reached max
        if (stats.IsValueReachMax(modifier.type)) {
          return false;
        }

        if (modifier.permanent) {
          continue;
        }

        if (Mathf.Approximately(modifier.canApplyWhenPreviousAt, 100f)) {
          continue;
        }

        foreach (var appliedModifier in listModifiers) {
          //modifier without lifetime or ended
          if (appliedModifier.Duration <= 0 || appliedModifier.Progress <= 0) {
            continue;
          }

          //skip modifiers with different display object
          if (appliedModifier.modifierDisplayObject != modifier.modifierDisplayObject) {
            continue;
          }

          //skip modifiers with different type
          if (appliedModifier.Type != modifier.type) {
            continue;
          }

          //modifier progress is greater than canApplyWhenPreviousAt
          if (!(appliedModifier.Progress > modifier.canApplyWhenPreviousAt)) {
            continue;
          }

          return false;
        }
      }

      return true;
    }

    private void RemovePrevious(Modifier statModifier) {
      if (statModifier.removePrevious == RemovePreviousModifiersType.None) {
        return;
      }

      switch (statModifier.removePrevious) {
        case RemovePreviousModifiersType.All:
          RemoveAllAppliedModifiers();
          break;
        case RemovePreviousModifiersType.AllWithSameType:
          RemoveAllAppliedModifiersWithSameType(statModifier.type);
          break;
        case RemovePreviousModifiersType.LastWithSameType:
          RemoveLastAppliedModifierWithSameType(statModifier.type);
          break;
        case RemovePreviousModifiersType.AllWithSameDisplayObject:
          RemoveAllAppliedModifiersWithSameModifierObject(statModifier.modifierDisplayObject);
          break;
        case RemovePreviousModifiersType.LastWithSameDisplayObject:
          RemoveLastAppliedModifierWithSameModifierObject(statModifier.modifierDisplayObject);
          break;
      }
    }

    public void RemoveModifiersByItemId(string id) {
      var appliedModifiers = listModifiers
        .Where(modifier => modifier.ItemId == id)
        .ToList();

      foreach (var appliedModifier in appliedModifiers) {
        appliedModifier.Dispose();
      }
    }

    private void RemoveAllAppliedModifiers() {
      foreach (var appliedModifier in listModifiers) {
        appliedModifier.Dispose();
      }
    }

    private void RemoveAllAppliedModifiersWithSameType(StatType statModifierType) {
      var appliedModifiers = listModifiers
        .Where(modifier => modifier.Type == statModifierType)
        .ToList();

      foreach (var appliedModifier in appliedModifiers) {
        appliedModifier.Dispose();
      }
    }

    private void RemoveLastAppliedModifierWithSameType(StatType statModifierType) {
      var appliedModifier = listModifiers.LastOrDefault(modifier => modifier.Type == statModifierType);

      appliedModifier?.Dispose();
    }

    private void RemoveAllAppliedModifiersWithSameModifierObject(ModifierDisplayObject modifierDisplayObject) {
      if (modifierDisplayObject == null) {
        return;
      }

      var appliedModifiers = listModifiers
        .Where(modifier => modifier.HasDisplay() && modifier.modifierDisplayObject.Id == modifierDisplayObject.Id)
        .ToList();

      foreach (var appliedModifier in appliedModifiers) {
        appliedModifier.Dispose();
      }
    }

    private void RemoveLastAppliedModifierWithSameModifierObject(ModifierDisplayObject modifierDisplayObject) {
      if (modifierDisplayObject == null) {
        return;
      }

      var appliedModifier =
        listModifiers.LastOrDefault(modifier =>
          modifier.HasDisplay() && modifier.modifierDisplayObject.Id == modifierDisplayObject.Id);

      appliedModifier?.Dispose();
    }

    private void AddModifier(StatModifier modifier) {
      listModifiers.Add(modifier);
      InvalidateCache(modifier.Type);
      modifier.ResetMarkForRemoval();

      stats.InvalidateStatCache(modifier.Type);

      modifier.OnDispose += _ => OnDispose(modifier);
      OnModifierAdded.Invoke(modifier);
    }

    private void OnDispose(StatModifier modifier) {
      InvalidateCache(modifier.Type);
      listModifiers.Remove(modifier);
      stats.InvalidateStatCache(modifier.Type);
      OnModifierRemoved.Invoke(modifier);
    }
  }
}