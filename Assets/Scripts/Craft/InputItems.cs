using System;
using System.Collections.Generic;
using Scriptables.Craft;
using UnityEngine;
using UnityServiceLocator;

namespace Craft {
  public class InputItems : MonoBehaviour, IInputItems {
    [SerializeField] private GameObject[] slots;

    private int itemsCount;
    private List<InputItem> items = new List<InputItem>();
    private int inputInProgress = 0;
    private DateTime? lastEndTime;
    private Workstation station;

    public List<InputItem> Items => items;

    public void Awake() {
      ServiceLocator.For(this).Register<IInputItems>(this);
      station = ServiceLocator.For(this).Get<Workstation>();
      itemsCount = station.OutputSlotsAmount;

      InitInputs();
    }

    private void InitInputs() {
      if (slots.Length < itemsCount) {
        Debug.LogError("InputItems: not enough slots in the interface");
        return;
      }

      for (var i = 0; i < slots.Length; i++) {
        if (i > itemsCount - 1) {
          continue;
        }

        var inputItem = slots[i].GetComponent<InputItem>();

        items.Add(inputItem);
      }
    }

    public void SetRecipe(int count, Recipe recipe) {
      var item = items[inputInProgress];
      item.Init(count, recipe, inputInProgress, lastEndTime);

      lastEndTime = item.Timer.GetEndTime();

      inputInProgress++;
    }

    public void UpdateWaitInputs(int fromPosition = 0) {
      if (inputInProgress == 1) {
        inputInProgress--;
        lastEndTime = null;
        return;
      }

      UpdateWaitChain(fromPosition);

      var firstInput = items[0];
      if (inputInProgress > 0 && !firstInput.Timer.IsStarted) {
        firstInput.StartCrafting();
      }
    }

    public void UpdateTimersStartTimes(InputItem inputItem) {
      if (inputItem.Position + 1 >= inputInProgress) {
        return;
      }

      var currentTime = DateTime.Now.ToUniversalTime();
      var currentInputStartTime = inputItem.Timer.GetStartTime();
      var newStartTime = currentTime > currentInputStartTime ? currentTime : currentInputStartTime;
      for (var i = inputItem.Position + 1; i < inputInProgress; i++) {
        var item = items[i];
        item.Timer.InitTimer(item.CountLeft, item.Recipe.CraftingTime, newStartTime);
        newStartTime = item.Timer.GetEndTime();
      }

      lastEndTime = newStartTime;
    }

    private void UpdateWaitChain(int fromPosition = 0) {
      for (var i = fromPosition; i < inputInProgress - 1; i++) {
        var nextPosition = i + 1;

        // Swap items
        (items[i], items[nextPosition]) = (items[nextPosition], items[i]);

        var currentItem = items[i];
        var nextItem = items[nextPosition];

        // Swap positions
        var tempTransformPosition = currentItem.GetTransformPosition();
        var tempPosition = currentItem.Position;
        currentItem.UpdateTransformPosition(nextItem.GetTransformPosition());
        currentItem.UpdatePosition(nextItem.Position);
        nextItem.UpdateTransformPosition(tempTransformPosition);
        nextItem.UpdatePosition(tempPosition);
      }

      inputInProgress--;
    }
  }
}