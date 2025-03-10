using System;
using Scriptables.Craft;
using UnityEngine;

namespace Craft {
  public struct ItemCraftedEventData {
    public Recipe Recipe;
    public int Count;
  }

  public class TimerInputItem : InputItem {
    [SerializeField] private Timer timer;

    public event Action<ItemCraftedEventData> OnItemCrafted;
    public event Action OnInputAllCrafted;

    public Timer Timer => timer;

    public override void Init(int count, Recipe recipe) {
      base.Init(count, recipe);

      timer.enabled = true;
      timer.onTimerStop += OnTimerStopHandler;
      timer.onItemTimerEnd += OnItemTimerEndHandler;
      timer.InitTimer(count, recipe);

      if (position == 0) {
        StartCrafting();
      }
    }

    public void StartCrafting() {
      timer.StartTimer();
    }

    private void OnTimerStopHandler() {
      ResetInput();
      OnInputAllCrafted?.Invoke();
    }

    private void OnItemTimerEndHandler(int count) {
      OnItemCrafted?.Invoke(new ItemCraftedEventData {
        Recipe = recipe,
        Count = count,
      });

      countLeft -= count;
      PrintCount();
    }

    public override void ResetInput() {
      base.ResetInput();

      timer.onTimerStop -= OnTimerStopHandler;
      timer.onItemTimerEnd -= OnItemTimerEndHandler;
      timer.enabled = false;
      timer.Reset();
    }
  }
}