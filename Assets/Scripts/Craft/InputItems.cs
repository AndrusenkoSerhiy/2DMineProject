using System.Collections.Generic;
using Scriptables.Craft;
using UnityEngine;

namespace Craft {
  public class InputItems {
    private readonly GameObject inputItemsContainer;
    private readonly GameObject inputItemPrefab;
    private readonly int count;
    private List<InputItem> inputs = new List<InputItem>();

    public InputItems(GameObject inputItemsContainer, GameObject inputItemPrefab, int count) {
      this.inputItemsContainer = inputItemsContainer;
      this.inputItemPrefab = inputItemPrefab;
      this.count = count;

      PrintInputs();
    }

    private void PrintInputs() {
      for (int i = 0; i < count; i++) {
        var input = GameObject.Instantiate(inputItemPrefab, inputItemsContainer.transform);
        var rectTransform = input.GetComponent<RectTransform>();
        if (rectTransform != null) {
          var newX = input.transform.position.x - (i * rectTransform.rect.width);
          input.transform.position = new Vector3(newX, input.transform.position.y, input.transform.position.z);
        }

        inputs.Add(input.GetComponent<InputItem>());
      }
    }

    public void SetRecipe(int count, Recipe recipe) {
      inputs[0].Init(count, recipe);
    }
  }
}