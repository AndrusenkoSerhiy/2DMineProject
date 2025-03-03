using System.Collections.Generic;
using Scriptables.Craft;
using UnityEngine;
using UnityServiceLocator;

namespace Craft {
  public class InputItems : MonoBehaviour, IInputItems {
    [SerializeField] private TimerInputItem craftInput;
    [SerializeField] private List<InputItem> items;

    private int itemsCount;

    private int inputInProgress;
    public int InputInProgress => inputInProgress;

    private Workstation station;

    public List<InputItem> Items => items;
    public TimerInputItem CraftInput => craftInput;

    public void Awake() {
      ServiceLocator.For(this).Register<IInputItems>(this);
      station = ServiceLocator.For(this).Get<Workstation>();
      itemsCount = station.OutputSlotsAmount;

      InitInputs();
    }

    public void InitComponent() {
      return;
    }

    public void ClearComponent() {
      inputInProgress = 0;
      foreach (var item in items) {
        item.ResetInput();
      }
    }

    private void InitInputs() {
      if (items.Count < itemsCount) {
        Debug.LogError("InputItems: not enough slots in the interface");
        return;
      }

      for (var i = 0; i < items.Count - 1; i++) {
        items[i].SetPosition(i);
      }
    }

    public void SetRecipe(int count, Recipe recipe) {
      var item = items[inputInProgress];
      item.Init(count, recipe);

      inputInProgress++;
    }

    public void UpdateWaitInputs(int fromPosition = 0) {
      if (inputInProgress == 1) {
        inputInProgress--;
        return;
      }

      UpdateWaitChain(fromPosition);
      inputInProgress--;

      if (inputInProgress > 0 && !craftInput.Timer.IsStarted) {
        craftInput.StartCrafting();
      }
    }

    private void UpdateWaitChain(int fromPosition = 0) {
      for (var i = fromPosition; i < inputInProgress - 1; i++) {
        var currentCount = items[i].CountLeft;
        var currentRecipe = items[i].Recipe;
        var nextCount = items[i + 1].CountLeft;
        var nextRecipe = items[i + 1].Recipe;

        items[i].ResetInput();
        if (nextRecipe) {
          items[i].Init(nextCount, nextRecipe);
        }

        items[i + 1].ResetInput();
        if (currentRecipe) {
          items[i + 1].Init(currentCount, currentRecipe);
        }
      }
    }
  }
}