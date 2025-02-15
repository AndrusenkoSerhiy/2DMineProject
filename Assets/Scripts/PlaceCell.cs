using Inventory;
using Scriptables;
using Settings;
using UnityEngine;
using World;

public class PlaceCell : MonoBehaviour {
  [SerializeField] private ResourceData resourceData;
  [SerializeField] private GameObject prefab;
  private GameObject previewInstance;
  [SerializeField] private bool isPreviewing;
  [SerializeField] private Color previewColor;
  [SerializeField] private Color blockColor;
  private Color currPreviewColor;
  private InventorySlot currSlot;
  private SpriteRenderer renderer;

  private void Start() {
    //don't need subscribe because we call from quickslot
    //UserInput.instance.OnBuildClick += Input_OnBuildClick;
  }

  public void ActivateBuildMode(InventorySlot slot, ResourceData rData) {
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
    UpdatePreview();
  }

  private void StartPreview() {
    if (prefab == null)
      return;

    isPreviewing = true;
    previewInstance = Instantiate(prefab);
    UpdatePreview();

    SetPreviewColor(previewColor);
  }

  private void UpdatePreview() {
    renderer = previewInstance.GetComponent<SpriteRenderer>();
    renderer.sprite = resourceData.Sprite(0);
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
  }

  //enable building mode
  private void SetEnabled(bool value) {
    isPreviewing = value;
    BlockAction(value);
    if (value) StartPreview();
    else CancelPreview();
  }

  private void BlockAction(bool value) {
    var actionName = "Attack";
    var reason = "PlaceCell";
    if (value) {
      UserInput.instance.BlockAction(actionName, reason);
    }
    else UserInput.instance.UnblockAction(actionName, reason);
  }

  private void Update() {
    if (UserInput.instance.controls.UI.Click.WasPressedThisFrame()) {
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
    var world = CoordsTransformer.GridToWorld(grid.X, grid.Y);

    return new Vector3(world.x, world.y, 0f);
  }

  private bool ShouldUseBlockColor(Vector3 worldPosition) {
    var grid = CoordsTransformer.WorldToGrid(worldPosition);
    var hasCell = GameManager.instance.ChunkController.GetCell(grid.X, grid.Y) != null;
    var isPlayerOnGrid = GameManager.instance.PlayerController.PlayerCoords.GetCoords().Equals(grid);
    return hasCell || isPlayerOnGrid;
  }

  private void UIInput_OnUIClick() {
    if (!isPreviewing || !GetPreviewColor()) {
      return;
    }

    PlaceCellOnScene();
  }

  /*private void Input_OnBuildClick(object sender, EventArgs e) {
    SetEnabled(!_isPreviewing);
  }*/

  private void PlaceCellOnScene() {
    var coords = CoordsTransformer.WorldToGrid(GetMousePosition());
    GameManager.instance.ChunkController.SpawnCell(coords, resourceData);
    currSlot.AddAmount(-1);
    ClearSLot();
  }

  private void ClearSLot() {
    if (currSlot.amount > 0)
      return;

    currSlot.Unselect();
    currSlot.RemoveItem();
    SetEnabled(false);
    currSlot = null;
  }

  private Vector3 GetMousePosition() {
    return GameManager.instance.MainCamera.ScreenToWorldPoint(UserInput.instance.GetMousePosition());
  }
}