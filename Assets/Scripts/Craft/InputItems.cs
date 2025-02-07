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
        inputItem.onInputAllCrafted += OnInputAllCraftedHandler;
        inputItem.onItemCrafted += OnItemCraftedHandler;

        items.Add(inputItem);
      }
    }

    private void OnItemCraftedHandler(Recipe recipe, int count) {
      station.RemoveCountFromCraftTotal(recipe.Result, count);
    }

    private void OnInputAllCraftedHandler() {
      station.RemoveFromCraftInputsItemsIds();

      if (inputInProgress == 1) {
        inputInProgress--;
        lastEndTime = null;
        return;
      }

      for (var i = 0; i < inputInProgress - 1; i++) {
        var nextPosition = i + 1;

        // Swap items
        (items[i], items[nextPosition]) = (items[nextPosition], items[i]);

        // Swap positions
        var tempPosition = items[i].GetTransformPosition();
        items[i].UpdateTransformPosition(items[nextPosition].GetTransformPosition());
        items[nextPosition].UpdateTransformPosition(tempPosition);
      }

      inputInProgress--;

      if (inputInProgress > 0) {
        items[0].StartCrafting();
      }
    }

    public void SetRecipe(int count, Recipe recipe) {
      var item = items[inputInProgress];
      item.Init(count, recipe, inputInProgress, lastEndTime);

      lastEndTime = item.Timer.GetEndTime();

      inputInProgress++;

      station.AddItemToCraftTotal(recipe.Result, count);
      station.AddToCraftInputsItemsIds(recipe.Result.data.Id);
    }
  }
}