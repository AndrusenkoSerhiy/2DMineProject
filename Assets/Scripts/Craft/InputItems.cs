using System;
using System.Collections.Generic;
using Scriptables.Craft;
using UnityEngine;

namespace Craft {
  public class InputItems {
    private readonly GameObject itemsContainer;
    private readonly GameObject itemPrefab;
    private readonly int itemsCount;
    private List<InputItem> items = new List<InputItem>();
    public List<InputItem> Items => items;
    private int inputInProgress = 0;
    private DateTime? lastEndTime;

    [SerializeField] private bool preventItemDrop;
    public bool PreventItemDrop => preventItemDrop;

    private Workstation station;

    public InputItems(Workstation station, GameObject itemsContainer, GameObject itemPrefab) {
      this.station = station;
      this.itemsCount = station.OutputSlotsAmount;
      this.itemsContainer = itemsContainer;
      this.itemPrefab = itemPrefab;

      PrintInputs();
    }

    private void PrintInputs() {
      for (int i = 0; i < itemsCount; i++) {
        var input = GameObject.Instantiate(itemPrefab, itemsContainer.transform);
        var rectTransform = input.GetComponent<RectTransform>();
        if (rectTransform != null) {
          var newX = input.transform.position.x - (i * rectTransform.rect.width);
          input.transform.position = new Vector3(newX, input.transform.position.y, input.transform.position.z);
        }

        var inputItem = input.GetComponent<InputItem>();

        items.Add(inputItem);
      }
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

    public void SetRecipe(int count, Recipe recipe) {
      var item = items[inputInProgress];
      item.Init(count, recipe, inputInProgress, lastEndTime);

      lastEndTime = item.Timer.GetEndTime();

      inputInProgress++;
    }
  }
}