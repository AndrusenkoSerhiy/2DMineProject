using System;
using System.Collections.Generic;
using Windows;
using Analytics;
using Audio;
using Craft;
using DG.Tweening;
using Inventory;
using Player;
using Scriptables;
using Scriptables.Items;
using UnityEngine;
using Utils;
using World;

public class PlaceCell : MonoBehaviour {
  [SerializeField] private Building buildingData;
  [SerializeField] private ResourceData resourceData;
  [SerializeField] private GameObject prefab;
  [SerializeField] private GameObject previewInstance;
  [SerializeField] private bool isPreviewing;
  private Color previewColor;
  private Color blockColor;
  private Color currPreviewColor;
  [SerializeField] private AnimationCurve spawnScaleCurve;
  private SpriteRenderer renderer;
  [SerializeField] private ResourceData emptyResourceData;

  [SerializeField] private int radius = 1;
  public static event Action OnSlotReset;

  private PlayerControllerBase playerController;
  private GameObject spawnPrefab;
  private ChunkController chunkController;
  private BuildingsDataController buildingDataController;
  private WindowsController windowsController;
  private AudioController audioController;
  private BuildingBlock currentBuildingBlock;

  private void Start() {
    playerController = GameManager.Instance.CurrPlayerController;
    chunkController = GameManager.Instance.ChunkController;
    buildingDataController = GameManager.Instance.BuildingsDataController;
    windowsController = GameManager.Instance.WindowsController;
    audioController = GameManager.Instance.AudioController;
  }

  // public void ActivateBuildMode(Building bData, ResourceData rData, GameObject sPrefab) {
  public void ActivateBuildMode(BuildingBlock buildingBlock, GameObject sPrefab) {
    spawnPrefab = sPrefab;
    currentBuildingBlock = buildingBlock;
    var bData = buildingBlock.BuildingData;
    var rData = buildingBlock.ResourceData;

    if (!isPreviewing) {
      EnableBuildMode(bData, rData);
      if (rData != null) {
        previewColor = rData.PreviewColor;
        blockColor = rData.BlockColor;
      }

      if (bData != null) {
        previewColor = bData.PreviewColor;
        blockColor = bData.BlockColor;
      }
    }
    else {
      DisableBuildMode();
    }
  }

  private InventorySlot GetSelectedSlot() {
    return GameManager.Instance.QuickSlotListener.GetSelectedSlot();
  }

  private void EnableBuildMode(Building bData, ResourceData rData) {
    buildingData = bData;
    resourceData = rData;
    SetEnabled(true);
  }

  public void DisableBuildMode() {
    SetEnabled(false);
    buildingData = null;
    resourceData = null;
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
    if (GameManager.Instance.UserInput.controls.UI.Click.WasPressedThisFrame() &&
        !windowsController.IsAnyWindowOpen) {
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
    var grid = CoordsTransformer.MouseToGridPosition(worldPosition); //WorldToGrid
    var clampedPosition = grid;
    var coords = GetPlayerCoords();
    clampedPosition.X = Mathf.Clamp(grid.X, coords.X - radius, coords.X + radius);
    clampedPosition.Y = Mathf.Clamp(grid.Y, coords.Y - radius, coords.Y + radius);
    var world = CoordsTransformer.GridToWorld(clampedPosition.X, clampedPosition.Y); //GridToWorld
    //Debug.DrawRay(CoordsTransformer.GridToWorld(coords.X, coords.Y), Vector3.up*10, Color.blue);
    return new Vector3(world.x, world.y, 0f);
  }

  private Coords GetPlayerCoords() {
    return GameManager.Instance.PlayerController.PlayerCoords.GetCoordsOutOfBounds();
  }

  private bool ShouldUseBlockColor(Vector3 worldPosition) {
    var grid = CoordsTransformer.MouseToGridPosition(worldPosition);
    var sizeX = buildingData ? buildingData.SizeX : 1;
    var sizeY = buildingData ? buildingData.SizeY : 1;

    //block building block upper start point
    if (resourceData != null && grid.Y < 0)
      return true;

    var canPlace = CanPlaceObject(grid.X, grid.Y, sizeX, sizeY);
    var isPlayerOnGrid = GetPlayerCoords().Equals(grid);
    //Debug.LogError($"canplace {grid.X} | {grid.Y}");
    var hasGround = HasGround(grid.X, grid.Y, sizeX);
    //if we place building we just need to know the all cells is empty
    if (buildingData) {
      return !canPlace || !hasGround;
    }

    return !canPlace || isPlayerOnGrid;
  }

  private bool CanPlaceObject(int startX, int startY, int objectSizeX, int objectSizeY) {
    for (var x = 0; x < objectSizeX; x++) {
      for (var y = 0; y < objectSizeY; y++) {
        var checkX = startX + x;
        var checkY = startY - y;
        /*Debug.LogError($"GetCellFill {chunkController.ChunkData.GetCellFill(checkX, checkY)} |" +
                       $"GetBuildDataConverted {buildingDataController.GetBuildDataConverted(checkX, checkY)} |" +
                       $"GetCellFill buid {buildingDataController.GetCellFill(checkX, checkY)}");
        */
        if (chunkController.ChunkData.GetCellFill(checkX, checkY) == 1 ||
            buildingDataController.GetBuildDataConverted(checkX, checkY) != null ||
            buildingDataController.GetCellFill(checkX, checkY) == 1) {
          return false;
        }
      }
    }

    return true;
  }

  private bool HasGround(int startX, int startY, int objectSizeX) {
    for (var x = 0; x < objectSizeX; x++) {
      var checkX = startX + x;
      var checkY = startY + 1;
      //var cellObj = chunkController.GetCell(checkX, checkY);

      if (chunkController.ChunkData.GetCellFill(checkX, checkY) == 0 /* ||
          cellObj.resourceData.Equals(emptyResourceData)*/ /*|| cellObj.resourceData.IsBuilding*/) {
        return false;
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
    if (buildingData) {
      PlaceBuilding();
    }
    else {
      PlaceBuildingBlock();
    }

    GameManager.Instance.ObjectivesSystem.ReportBuild(currentBuildingBlock, 1);

    GetSelectedSlot().RemoveAmount(1);
    ClearSLot();
  }

  private void PlaceBuilding() {
    var pos = GetSnappedWorldPosition();
    var coords = CoordsTransformer.WorldToGridBuildings(pos);
    var build = chunkController.SpawnBuild(coords, buildingData);
    //Debug.LogError($"spawn build {coords.X}, {coords.Y}!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    var posCoords = CoordsTransformer.MouseToGridPosition(pos);

    AfterPlaceCellActions(build, posCoords);
    //SetCellsUndamegable(test.X, test.Y, build.Building.SizeX);

    var item = GetSelectedSlot().Item;
    GameManager.Instance.Locator.SetTarget(pos, item.id);

    PlayBuildingBlockPlaceSound();

    //Tween for spawn effect
    var spriteRenderer = build.GetComponentInChildren<SpriteRenderer>();
    var childObject = spriteRenderer.gameObject;
    var material = spriteRenderer.material;
    material.SetFloat("_Dissolve", 0f);
    DOTween.To(() => material.GetFloat("_Dissolve"), x => material.SetFloat("_Dissolve", x), 1f, 0.4f);
    childObject.transform.localScale = new Vector3(0f, 0f, 0f);
    childObject.transform.DOScale(Vector3.one, 0.5f).SetEase(spawnScaleCurve);
    GameManager.Instance.PoolEffects.SpawnFromPool("PlaceCellEffect", childObject.transform.position,
      Quaternion.identity);

    //AnalyticsManager.Instance.LogStationPlaced(item.name);
  }

  public bool RemoveBuilding(BuildingDataObject buildObject, ItemObject itemObject) {
    // var coords = CoordsTransformer.MouseToGridPosition(buildObject.transform.position);
    // var worldCoords = CoordsTransformer.WorldToGridBuildings(buildObject.transform.position);
    chunkController.RemoveBuild(buildObject);
    //SetCellsUndamegable(coords.X, coords.Y, buildObject.Building.SizeX, true);
    GameManager.Instance.Locator.RemoveTarget(itemObject.Id);
    audioController.PlayTakeBuilding();

    AfterBuildingRemoved(buildObject);

    //AnalyticsManager.Instance.LogStationRemoved(itemObject.name);

    return true;
  }

  private void PlaceBuildingBlock() {
    var coords = CoordsTransformer.WorldToGrid(GetSnappedWorldPosition());
    var cell = chunkController.ChunkData.ForceCellFill(resourceData, coords.X, coords.Y);
    chunkController.AfterCellChanged(cell);
    chunkController.UpdateCellAround(coords.X, coords.Y);
    chunkController.AddCellToActives(coords.X, coords.Y, resourceData);

    PlayBuildingPlaceSound();

    //Tween for spawn effect
    var spawnedGo = GameManager.Instance.ChunkController.GetCell(coords.X, coords.Y);
    var spriteRenderer = spawnedGo.GetComponentInChildren<SpriteRenderer>();
    var childObject = spriteRenderer.gameObject;
    var material = spriteRenderer.material;
    material.SetFloat("_Dissolve", 0f);
    DOTween.To(() => material.GetFloat("_Dissolve"), x => material.SetFloat("_Dissolve", x), 1f, 0.4f);
    childObject.transform.localScale = new Vector3(0f, 0f, 0f);
    childObject.transform.DOScale(Vector3.one, 0.5f).SetEase(spawnScaleCurve);
    GameManager.Instance.PoolEffects.SpawnFromPool("PlaceCellEffect", childObject.transform.position,
      Quaternion.identity);
  }

  /*private void SetCellsUndamegable(int startX, int startY, int objectSizeX, bool isDamageable = false) {
    var coordY = startY + 1;
    for (var x = 0; x < objectSizeX; x++) {
      var coordX = startX + x;
      chunkController.GetCell(coordX, coordY).CanGetDamage = isDamageable;
      //Debug.DrawRay(CoordsTransformer.GridToWorld(coordX, coordY), Vector3.up, Color.green, 100f);
    }
  }*/

  public void AfterPlaceCellActions(BuildingDataObject build, Coords coords) {
    if (build.TryGetComponent<Workbench>(out var workbench)) {
      var station = workbench.StationObject;
      if (!station) {
        return;
      }

      GameManager.Instance.RecipesManager.UnlockStation(station.RecipeType);
      AfterPlaceCellWithBaseCells(workbench, coords, build.Building.SizeX);
    }
    else if (build.TryGetComponent<Storage>(out var storage)) {
      AfterPlaceCellWithBaseCells(storage, coords, build.Building.SizeX);
    }
  }

  private void AfterPlaceCellWithBaseCells(IBaseCellHolder baseCellHolder, Coords coords, int sizeX) {
    SetBaseCells(baseCellHolder, chunkController, coords, sizeX);
  }

  public static void SetBaseCells(IBaseCellHolder baseCellHolder, ChunkController chunkController, Coords coords,
    int sizeX) {
    var cells = new List<CellData>();
    var coordY = coords.Y + 1;
    for (var x = 0; x < sizeX; x++) {
      var coordX = coords.X + x;
      var cell = chunkController.ChunkData.GetCellData(coordX, coordY);
      cells.Add(cell);
    }

    baseCellHolder.SetBaseCells(cells);
  }

  public void AfterBuildingRemoved(BuildingDataObject build) {
    if (build.TryGetComponent<IBaseCellHolder>(out var cellHolder)) {
      cellHolder.ClearBaseCells();
    }
  }

  private void PlayBuildingPlaceSound() {
    if (currentBuildingBlock && currentBuildingBlock.ConsumeSound) {
      audioController.PlayAudio(currentBuildingBlock.ConsumeSound);
    }
    else {
      audioController.PlayPlaceBuilding();
    }
  }

  private void PlayBuildingBlockPlaceSound() {
    if (currentBuildingBlock && currentBuildingBlock.ConsumeSound) {
      audioController.PlayAudio(currentBuildingBlock.ConsumeSound);
    }
    else {
      audioController.PlayPlaceBuildingBlock();
    }
  }

  private void ClearSLot() {
    if (GetSelectedSlot().amount > 0)
      return;

    OnSlotReset?.Invoke();
    SetEnabled(false);
    buildingData = null;
  }

  private Vector3 GetMousePosition() {
    var mousePosition =
      GameManager.Instance.MainCamera.ScreenToWorldPoint(GameManager.Instance.UserInput.GetMousePosition());
    mousePosition.z = 0;
    return mousePosition;
  }
}