using System;
using System.Collections.Generic;
using Scriptables.Craft;
using UnityEngine;
using UnityServiceLocator;

namespace Craft {
  public class InputItems : MonoBehaviour, IInputItems {
    [SerializeField] private TimerInputItem craftInput;
    [SerializeField] private List<InputItem> items;

    private int inputInProgress;
    public int InputInProgress => inputInProgress;

    private Workstation station;

    public List<InputItem> Items => items;
    public TimerInputItem CraftInput => craftInput;

    public void Awake() {
      ServiceLocator.For(this).Register<IInputItems>(this);
      station = ServiceLocator.For(this).Get<Workstation>();

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
      for (var i = 0; i < items.Count; i++) {
        items[i].SetPosition(i);
      }
    }

    public void SetRecipe(int count, Recipe recipe) {
      var maxStack = recipe.Result.MaxStackSize;
      while (count > 0) {
        var addCount = count > maxStack ? maxStack : count;
        var item = items[inputInProgress];
        item.Init(addCount, recipe);

        count -= addCount;
        inputInProgress++;
      }
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
      for (var i = fromPosition; i < inputInProgress; i++) {
        var current = items[i];
        var currentCount = current.CountLeft;
        var currentRecipe = current.Recipe;

        current.ResetInput();
        if ((i + 1) >= inputInProgress) {
          break;
        }

        var next = items[i + 1];

        var nextCount = next.CountLeft;
        var nextRecipe = next.Recipe;

        current.Init(nextCount, nextRecipe);

        next.ResetInput();
        if (currentRecipe) {
          next.Init(currentCount, currentRecipe);
        }
      }
    }
  }
}