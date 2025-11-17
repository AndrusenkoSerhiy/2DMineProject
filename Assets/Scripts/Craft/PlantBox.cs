using System;
using System.Collections.Generic;
using Farm;
using Interaction;
using SaveSystem;
using Scriptables.Craft;
using Scriptables.Items;
using UnityEngine;
using World;

namespace Craft {
  public class PlantBox : MonoBehaviour, IInteractable, IBaseCellHolder {
    protected GameManager gameManager;
    private CellHolderHandler cellHandler;

    [SerializeField] private bool hasHoldInteraction;
    [SerializeField] private ItemObject dirt;
    [SerializeField] protected ItemObject buildingData;
    [SerializeField] private BuildingDataObject buildObject;
    [SerializeField] private Recipe stationRecipe;
    [SerializeField] private Color destroyEffectColor = new(148, 198, 255, 255);
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer groundIcon;
    
    [SerializeField] private bool hasGround;
    [SerializeField] private bool hasSeeds;
    [SerializeField] private bool startGrowing;
    [SerializeField] private bool hasRipened;

    [SerializeField] private Seeds currSeed;
    [SerializeField] private ItemObject currHarvest;
    [SerializeField] private int timeToGrowth;
    [SerializeField] private float currTime;
    
    [SerializeField] private List<SpriteRenderer> grownSprites;

    [SerializeField] private string AddGroundStr;
    [SerializeField] private string AddSeedStr;
    [SerializeField] private string GrowingStr;
    [SerializeField] private string CollectStr;
    [SerializeField] private bool canDestroyCellsBelow = true;
    
    public bool HasGround => hasGround;
    public bool HasSeeds => hasSeeds;
    public bool StartGrowing => startGrowing;
    public bool HasRipened => hasRipened;

    public Seeds CurrSeed => currSeed;
    public ItemObject CurrHarvest => currHarvest;
    public int TimeToGrowth => timeToGrowth;
    public float CurrTime => currTime;
    public string InteractionText => GetInteractionText();
    public bool HasHoldInteraction => hasHoldInteraction;
    public string HoldInteractionText => "Collect seed";
    public string HoldProcessText { get; }
    private void Awake() {
      gameManager = GameManager.Instance;
      cellHandler = new CellHolderHandler(OnAllBaseCellsDestroyed, stationRecipe, transform.position);
    }
    
    public void SetParamFromManager(ProcessingPlantBox data, bool load = false) {
      hasGround = data.HasGround;
      hasSeeds = data.HasSeeds;
      startGrowing = data.StartGrowing;
      hasRipened = data.HasRipened;
      currSeed = data.CurrSeed;
      currHarvest = data.CurrHarvest;
      timeToGrowth = data.TimeToGrowth;
      currTime = data.CurrTime;
      
      if (currTime > 0) {
        if (!load) {
          var now = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
          var timePassed = now - data.LastUpdateTime;
          currTime += (float)timePassed; 
        }
      }
      else {
        ResetGrownSprites();
      }
      UpdateSprites();
    }
    
    private void ResetGrownSprites() {
      foreach (var sprite in grownSprites) {
        sprite.enabled = false;
      }
      groundIcon.enabled = false;
    }

    private void UpdateSprites() {
      if (hasGround) {
        groundIcon.enabled = true;
      }
      SetInfoFromSeedData();
      if (hasSeeds) {
        SetGrownSprites();
      }
    }
    public bool Interact(PlayerInteractor playerInteractor) {
      var equipedItem = gameManager.PlayerEquipment.EquippedItem;

      if (hasRipened) {
        AddToInventory(currHarvest);
        ClearPlantBox();
      }
      
      if (!hasGround) {
        if (equipedItem != null && equipedItem.info == dirt) {
          groundIcon.enabled = true;
          hasGround = true;
          gameManager.QuickSlotListener.GetSelectedSlot().RemoveAmount(1);
        }
      }
      else {
        if (equipedItem != null && !hasSeeds && equipedItem.info.Type == ItemType.Seeds) {
          gameManager.QuickSlotListener.GetSelectedSlot().RemoveAmount(1);
          currSeed = (Seeds)equipedItem.info;
          SetInfoFromSeedData();
          ActivateGrownSprites(0);
          hasHoldInteraction = true;
          hasSeeds = true;
          startGrowing = true;
          //Debug.LogError($"growing time {((Seeds)currSeed).TimeToGrowth}");
        }
      }
      
      return true;
    }

    private void SetInfoFromSeedData() {
      if (currSeed != null) {
        timeToGrowth = currSeed.TimeToGrowth;
        currHarvest = currSeed.Harvest;
        var spritesFromSeed = currSeed.GrownSprites;
        for (int i = 0; i < grownSprites.Count; i++) {
          grownSprites[i].sprite = spritesFromSeed[i];
        }
      }
    }

    private void ActivateGrownSprites(int index) {
      foreach (var sprite in grownSprites) {
        sprite.enabled = false;
      }
      grownSprites[index].enabled = true;
    }

    private void Update() {
      if (startGrowing) {
        currTime += Time.deltaTime;

        SetGrownSprites();
      }
    }

    private void SetGrownSprites() {
      if (currTime < timeToGrowth * .5f) {
        ActivateGrownSprites(0);
      }
      
      if (currTime >= timeToGrowth * .5f) {
        ActivateGrownSprites(1);
      }
        
      if (currTime >= timeToGrowth) {
        ActivateGrownSprites(2);
        startGrowing = false;
        hasRipened = true;
        hasHoldInteraction = false;
      }
    }
    private string GetInteractionText() {
      if (hasRipened) {
        return CollectStr;
      }
      
      if (!hasGround) {
        return AddGroundStr;
      }

      if (!hasSeeds) {
        return AddSeedStr;
      }

      if (startGrowing) {
        return GrowingStr;
      }
      
      return string.Empty;
    }

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      if (!hasHoldInteraction || hasRipened) {
        return false;
      }
      
      AddToInventory(currSeed);
      hasHoldInteraction = false;
      ClearPlantBox();
      return true;
    }

    private void AddToInventory(ItemObject itemObject) {
      if (!gameManager.PlayerInventory.CanAddItemToInventory(itemObject)) {
        gameManager.MessagesManager.ShowSimpleMessage("Inventory is full");
        return;
      }

      var addedAmount =
        gameManager.PlayerInventory.AddItemToInventoryWithOverflowDrop(new Item(itemObject, 0), 1);
      gameManager.MessagesManager.ShowPickupResourceMessage(itemObject, addedAmount);
    }

    public Bounds GetBounds() {
      return spriteRenderer ? spriteRenderer.bounds : new Bounds(transform.position, Vector3.zero);
    }
    
    private void OnAllBaseCellsDestroyed() {
      var psGo = GameManager.Instance.PoolEffects
        .SpawnFromPool("CellDestroyEffect", transform.position, Quaternion.identity)
        .gameObject;
      var ps = psGo.GetComponent<ParticleSystem>();

      if (ps != null) {
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(destroyEffectColor);
      }

      gameManager.PlaceCell.RemoveBuilding(buildObject, buildingData);
      gameManager.MessagesManager.ShowSimpleMessage($"{buildingData.Name} destroyed");
      gameManager.AudioController.PlayWorkstationDestroyed();
    }

    private void ClearPlantBox() {
      startGrowing = false;
      currTime = 0;
      currSeed = null;
      currHarvest = null;
      groundIcon.enabled = false;

      foreach (var plant in grownSprites) {
        plant.enabled = false;
      }
      hasGround = false;
      hasSeeds = false;
      hasRipened = false;
    }

    public bool CanDestroyCellsBelow { get; set; }

    public void SetBaseCells(List<CellData> cells) {
      CanDestroyCellsBelow = canDestroyCellsBelow;
      cellHandler.SetBaseCells(cells, transform.position, CanDestroyCellsBelow);
    }

    public void ClearBaseCells() {
      cellHandler.ClearBaseCells();
    }
  }
}