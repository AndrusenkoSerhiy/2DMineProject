using Inventory;
using Player;
using Scriptables;
using Settings;
using UnityEngine;
using Utils;
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
  [SerializeField] private int radius = 1;
  private Coords playerCoords;
  private PlayerControllerBase playerController;

  private void Start() {
    playerCoords = GameManager.Instance.PlayerController.PlayerCoords.GetCoords();
    playerController = GameManager.Instance.CurrPlayerController;
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
    playerController?.SetLockHighlight(true);
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
    playerController?.SetLockHighlight(false);
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
    var clampedPosition = grid;
    clampedPosition.X = Mathf.Clamp(grid.X, playerCoords.X - radius, playerCoords.X + radius);
    clampedPosition.Y = Mathf.Clamp(grid.Y, playerCoords.Y - radius, playerCoords.Y + radius);
    var world = CoordsTransformer.GridToWorld(clampedPosition.X, clampedPosition.Y);

    return new Vector3(world.x, world.y, 0f);
  }

  private bool ShouldUseBlockColor(Vector3 worldPosition) {
    var grid = CoordsTransformer.WorldToGrid(worldPosition);
    var hasCell = GameManager.Instance.ChunkController.GetCell(grid.X, grid.Y) != null;
    var isPlayerOnGrid = GameManager.Instance.PlayerController.PlayerCoords.GetCoords().Equals(grid);
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
    var coords = CoordsTransformer.WorldToGrid(GetSnappedWorldPosition());
    GameManager.Instance.ChunkController.ChunkData.ForceCellFill(resourceData, coords.X, coords.Y);
    GameManager.Instance.ChunkController.UpdateCellAround(coords.X, coords.Y);
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
    var mousePosition = GameManager.Instance.MainCamera.ScreenToWorldPoint(UserInput.instance.GetMousePosition());
    mousePosition.z = 0;
    return mousePosition;
  }
}