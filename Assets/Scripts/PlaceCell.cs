using System;
using Craft;
using Inventory;
using Player;
using Scriptables;
using UnityEngine;
using Utils;
using World;

public class PlaceCell : MonoBehaviour {
  [SerializeField] private ResourceData resourceData;
  [SerializeField] private GameObject prefab;
  [SerializeField] private GameObject previewInstance;
  [SerializeField] private bool isPreviewing;
  [SerializeField] private Color previewColor;
  [SerializeField] private Color blockColor;
  private Color currPreviewColor;
  [SerializeField] private InventorySlot currSlot;
  private SpriteRenderer renderer;

  [SerializeField] private int radius = 1;
  public static event Action OnSlotReset;
  
  private PlayerControllerBase playerController;
  private GameObject spawnPrefab;
  private ChunkController chunkController;

  private void Start() {
    playerController = GameManager.Instance.CurrPlayerController;
    chunkController = GameManager.Instance.ChunkController;
  }

  private Coords GetPlayerCoords() {
    return GameManager.Instance.PlayerController.PlayerCoords.GetCoords();
  }

  public void ActivateBuildMode(InventorySlot slot, ResourceData rData, GameObject sPrefab) {
    spawnPrefab = sPrefab;
    if (currSlot == null) {
      EnableBuildMode(slot, rData);
    }
    else if (currSlot.Item == slot.Item) {
      DisableBuildMode();
    }
    else if (currSlot.Item != slot.Item) {
      ChangeBuildMode(slot, rData);
    }
  }

  private void EnableBuildMode(InventorySlot slot, ResourceData rData) {
    currSlot = slot;
    resourceData = rData;
    SetEnabled(true);
  }

  private void DisableBuildMode() {
    SetEnabled(false);
    currSlot = null;
    resourceData = null;
  }

  private void ChangeBuildMode(InventorySlot slot, ResourceData rData) {
    currSlot = slot;
    resourceData = rData;
    SetEnabled(true);
  }

  private void StartPreview() {
    if (prefab == null)
      return;

    isPreviewing = true;
    previewInstance = Instantiate(spawnPrefab);
    renderer = previewInstance.GetComponentInChildren<SpriteRenderer>();

    SetPreviewColor(previewColor);
    playerController?.SetLockHighlight(true);
  }

  private void SetPreviewColor(Color col) {
    renderer.color = col;
    currPreviewColor = col;
  }

  private bool GetPreviewColor() {
    return currPreviewColor == previewColor;
  }

  private void CancelPreview() {
    isPreviewing = false;
    if (previewInstance != null) {
      Destroy(previewInstance);
    }

    playerController?.SetLockHighlight(false);
  }

  //enable building mode
  private void SetEnabled(bool value) {
    //Debug.LogError($"SetEnabled {value}");
    isPreviewing = value;
    BlockAction(value);
    if (value) StartPreview();
    else CancelPreview();
  }

  private void BlockAction(bool value) {
    var actionName = "Attack";
    var reason = "PlaceCell";
    if (value) {
      GameManager.Instance.UserInput.BlockAction(actionName, reason);
    }
    else GameManager.Instance.UserInput.UnblockAction(actionName, reason);
  }

  private void Update() {
    if (GameManager.Instance.UserInput.controls.UI.Click.WasPressedThisFrame()) {
      UIInput_OnUIClick();
    }

    UpdatePreviewPosition();
  }

  private void UpdatePreviewPosition() {
    if (!isPreviewing || previewInstance == null)
      return;

    var snappedPosition = GetSnappedWorldPosition();

    previewInstance.transform.position = snappedPosition;

    SetPreviewColor(ShouldUseBlockColor(snappedPosition) ? blockColor : previewColor);
  }

  private Vector3 GetSnappedWorldPosition() {
    var worldPosition = GetMousePosition();
    var grid = CoordsTransformer.WorldToGrid(worldPosition);
    var clampedPosition = grid;
    var coords = GetPlayerCoords();
    clampedPosition.X = Mathf.Clamp(grid.X, coords.X - radius, coords.X + radius);
    clampedPosition.Y = Mathf.Clamp(grid.Y, coords.Y - radius, coords.Y + radius);
    var world = CoordsTransformer.GridToWorld(clampedPosition.X, clampedPosition.Y);

    return new Vector3(world.x, world.y, 0f);
  }

  private bool ShouldUseBlockColor(Vector3 worldPosition) {
    var grid = CoordsTransformer.WorldToGrid(worldPosition);
    var canPlace = CanPlaceObject(grid.X, grid.Y, (int)resourceData.CellSize.x, (int)resourceData.CellSize.y);
    var isPlayerOnGrid = GetPlayerCoords().Equals(grid);
    //if we place building we just need to know the all cells is empty
    if (resourceData.IsBuilding) {
      return !canPlace;
    }

    return !canPlace || isPlayerOnGrid;
  }

  private bool CanPlaceObject(int startX, int startY, int objectSizeX, int objectSizeY) {
    for (var x = 0; x < objectSizeX; x++) {
      for (var y = 0; y < objectSizeY; y++) {
        var checkX = startX + x;
        var checkY = startY - y;

        if (chunkController.ChunkData.GetCellFill(checkX, checkY) == 1) {
          return false;
        }
      }
    }

    return true;
  }

  private void UIInput_OnUIClick() {
    if (!isPreviewing || !GetPreviewColor()) {
      return;
    }

    PlaceCellOnScene();
  }

  private void PlaceCellOnScene() {
    //for forge, stoneCutter etc
    if (resourceData.IsBuilding) {
      PlaceBuilding();
    }
    else {
      PlaceBuildingBlock();
    }

    currSlot.RemoveAmount(1);
    ClearSLot();
  }

  private void PlaceBuilding() {
    var coords = CoordsTransformer.WorldToGrid(GetSnappedWorldPosition());
    chunkController.ChunkData.ForceCellFill(resourceData, coords.X, coords.Y);
    FillBuildingCells(coords.X, coords.Y, (int)resourceData.CellSize.x, (int)resourceData.CellSize.y);
    var cell = chunkController.SpawnBuild(coords, resourceData);
    cell.BoxCollider2D.enabled = true;
    AfterPlaceCellActions(cell);
  }

  private void PlaceBuildingBlock() {
    var coords = CoordsTransformer.WorldToGrid(GetSnappedWorldPosition());
    chunkController.ChunkData.ForceCellFill(resourceData, coords.X, coords.Y);
    chunkController.UpdateCellAround(coords.X, coords.Y);
  }

  private void FillBuildingCells(int startX, int startY, int objectSizeX, int objectSizeY) {
    for (var x = 0; x < objectSizeX; x++) {
      for (var y = 0; y < objectSizeY; y++) {
        var coordX = startX + x; 
        var coordY = startY - y;
        chunkController.ChunkData.SetCellFill(coordX, coordY);
      }
    }
  }

  private void AfterPlaceCellActions(CellObject cell) {
    cell.TryGetComponent<Workbench>(out var worckbench);
    if (!worckbench) {
      return;
    }

    var station = worckbench.StationObject;
    if (station == null) {
      return;
    }

    GameManager.Instance.RecipesManager.UnlockStation(station.RecipeType);
  }

  private void ClearSLot() {
    if (currSlot.amount > 0)
      return;

    OnSlotReset?.Invoke();
    SetEnabled(false);
    currSlot = null;
    resourceData = null;
  }

  private Vector3 GetMousePosition() {
    var mousePosition =
      GameManager.Instance.MainCamera.ScreenToWorldPoint(GameManager.Instance.UserInput.GetMousePosition());
    mousePosition.z = 0;
    return mousePosition;
  }
}