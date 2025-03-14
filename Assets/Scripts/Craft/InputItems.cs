using System.Collections.Generic;
using UnityEngine;
using UnityServiceLocator;

namespace Craft {
  public class InputItems : MonoBehaviour {
    [SerializeField] private List<InputItem> items;

    private Workstation station;

    private void Awake() {
      station = ServiceLocator.For(this).Get<Workstation>();
    }

    private void OnEnable() {
      InitInputs();
      station.OnAfterAddItemToInputs += SetRecipe;
      station.OnCraftCanceled += OnCraftCanceledHandler;
      station.OnInputAllCrafted += OnInputAllCraftedHandler;
      station.OnItemCrafted += OnItemCraftedHandler;
    }

    private void OnDisable() {
      station.OnAfterAddItemToInputs -= SetRecipe;
      station.OnCraftCanceled -= OnCraftCanceledHandler;
      station.OnInputAllCrafted -= OnInputAllCraftedHandler;
      station.OnItemCrafted -= OnItemCraftedHandler;
      ResetInputs();
    }

    private void OnItemCraftedHandler() {
      if (station.Inputs.Count == 0) {
        return;
      }

      var item = items[0];
      item.ResetInput();
      item.Init(station.Inputs[0], 0);
    }

    private void OnCraftCanceledHandler(Input input, int position) => UpdateInputs();
    private void OnInputAllCraftedHandler() => UpdateInputs();

    private void InitInputs() {
      for (var i = 0; i < items.Count; i++) {
        var item = items[i];
        
        if (i > station.Inputs.Count - 1) {
          break;
        }

        var inputData = station.Inputs[i];
        
        item.Init(inputData, i);
      }
    }

    private void ResetInputs() {
      foreach (var item in items) {
        item.ResetInput();
      }
    }

    private void UpdateInputs() {
      for (var i = 0; i < items.Count; i++) {
        var item = items[i];
        item.ResetInput();
        if (i >= station.Inputs.Count) {
          continue;
        }

        var inputData = station.Inputs[i];
        item.Init(inputData, i);
      }
    }

    private void SetRecipe(Input inputData) {
      var inputInProgress = station.Inputs.Count - 1;
      var item = items[inputInProgress];
      item.Init(inputData, inputInProgress);
    }
  }
}