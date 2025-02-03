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

    public InputItems(GameObject itemsContainer, GameObject itemPrefab, int count) {
      this.itemsContainer = itemsContainer;
      this.itemPrefab = itemPrefab;
      this.itemsCount = count;

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

        items.Add(input.GetComponent<InputItem>());
      }
    }

    public void SetRecipe(int count, Recipe recipe) {
      items[0].Init(count, recipe);
    }
  }
}