using Inventory;
using Scriptables;
using Settings;
using UnityEngine;
using World;

public class PlaceCell : MonoBehaviour {
  [SerializeField] private ResourceData _resourceData;
  [SerializeField] private GameObject _prefab;
  private GameObject _previewInstance;
  [SerializeField] private bool _isPreviewing;
  [SerializeField] private Color _previewColor;
  [SerializeField] private Color _blockColor;
  private Color _currPreviewColor;
  private InventorySlot currSlot;
  private void Start() {
    //don't need subscribe because we call from quickslot
    //UserInput.instance.OnBuildClick += Input_OnBuildClick;
  }
  
  public void ActivateBuildMode(InventorySlot slot) {
    SetEnabled(!_isPreviewing);
    currSlot = slot;
  }
  
  private void StartPreview() {
    if (_prefab == null)
      return;

    _isPreviewing = true;
    _previewInstance = Instantiate(_prefab);

    SetPreviewColor(_previewColor);
  }

  private void SetPreviewColor(Color col) {
    var renderer = _previewInstance.GetComponent<SpriteRenderer>();
    renderer.color = col;
    _currPreviewColor = col;
  }

  //use for condition if we can to place cell
  private bool GetPreviewColor() {
    return _currPreviewColor == _previewColor;
  }

  private void CancelPreview() {
    _isPreviewing = false;
    if (_previewInstance != null) {
      Destroy(_previewInstance);
    }
  }
  
  //enable building mode
  private void SetEnabled(bool value) {
    _isPreviewing = value;
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
    
    if (_isPreviewing && _previewInstance != null)
    {
      // Update the position of the preview to follow the mouse position
      Vector3 mousePosition = UserInput.instance.GetMousePosition();
      mousePosition.z = 0f; 
      Vector3 worldPosition = GetMousePosition();

      var grid = CoordsTransformer.WorldToGrid(worldPosition);
      var world = CoordsTransformer.GridToWorld(grid.X, grid.Y);
      
      worldPosition.x = world.x;
      worldPosition.y = world.y;
      worldPosition.z = 0;

      _previewInstance.transform.position = worldPosition;
      /*Debug.LogError($"player {GameManager.instance.PlayerController.PlayerCoords.GetCoords().X} {GameManager.instance.PlayerController.PlayerCoords.GetCoords().Y}");
      Debug.LogError($"grid   {grid.X} {grid.Y}");*/
      if (GameManager.instance.ChunkController.GetCell(grid.X, grid.Y) != null || GameManager.instance.PlayerController.PlayerCoords.GetCoords().Equals(grid)) {
        SetPreviewColor(_blockColor);
      }
      else {
        SetPreviewColor(_previewColor);
      }
      /*if (Input.GetMouseButtonDown(1))
      {
        CancelPreview();
      }*/
    }
  }

  private void UIInput_OnUIClick() {
    if (!_isPreviewing || !GetPreviewColor()) {
      return; 
    }
    
    PlaceCellOnScene();
  }

  /*private void Input_OnBuildClick(object sender, EventArgs e) {
    SetEnabled(!_isPreviewing);
  }*/

  private void PlaceCellOnScene() {
    var coords = CoordsTransformer.WorldToGrid(GetMousePosition());
    GameManager.instance.ChunkController.SpawnCell(coords,_resourceData);
    currSlot.AddAmount(/*currSlot.amount*/-1, 20);
    //TODO
    if (currSlot.amount <=0) {
      currSlot.Unselect();
      currSlot.RemoveItem();
      SetEnabled(false);
      currSlot = null;
    }
  }

  private Vector3 GetMousePosition() {
    return GameManager.instance.MainCamera.ScreenToWorldPoint(UserInput.instance.GetMousePosition());
  }
}