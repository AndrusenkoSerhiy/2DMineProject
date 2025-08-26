using System.Collections.Generic;
using Interaction;
using Scriptables.Craft;
using UnityEngine;
using World;

namespace Craft {
  public class Workbench : Crafter, IInteractable, IBaseCellHolder {
    [SerializeField] private string interactText;
    [SerializeField] private string holdInteractText;
    [SerializeField] private bool hasHoldInteraction = true;
    [SerializeField] private Recipe stationRecipe;
    [SerializeField] private Color destroyEffectColor = new(148, 198, 255, 255);

    private CellHolderHandler cellHandler;

    public string InteractionText => interactText;
    public bool HasHoldInteraction => hasHoldInteraction;
    public string HoldInteractionText => holdInteractText;

    protected override void Awake() {
      base.Awake();
      cellHandler = new CellHolderHandler(OnAllBaseCellsDestroyed, stationRecipe, transform.position);
    }

    public bool Interact(PlayerInteractor playerInteractor) {
      CheckInteract();

      return true;
    }

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      CheckHoldInteract();

      return true;
    }

    public void ClearBaseCells() {
      cellHandler.ClearBaseCells();
    }

    public void SetBaseCells(List<CellData> cells) {
      cellHandler.SetBaseCells(cells, transform.position);
    }

    private void OnAllBaseCellsDestroyed() {
      var workStation = GetWorkstation();
      workStation?.StopAndDropItems(transform.position);

      var psGo = GameManager.Instance.PoolEffects
        .SpawnFromPool("CellDestroyEffect", transform.position, Quaternion.identity)
        .gameObject;
      var ps = psGo.GetComponent<ParticleSystem>();

      if (ps != null) {
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(destroyEffectColor);
      }

      gameManager.PlaceCell.RemoveBuilding(buildObject, stationObject.InventoryItem);
      gameManager.MessagesManager.ShowSimpleMessage(stationObject.Title + " destroyed");
      gameManager.AudioController.PlayWorkstationDestroyed();
    }
  }
}