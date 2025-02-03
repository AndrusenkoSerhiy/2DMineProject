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
  private Color _currPreviewColor;
  private InventorySlot currSlot;
  private void Start() {
    //don't need subscribe because we call from quickslot
    //UserInput.instance.OnBuildClick += Input_OnBuildClick;
  }
  
  public void ActivateBuildMode(InventorySlot slot, ResourceData rData) {
    //Debug.LogError("Use");
    if (currSlot == null) {
      //Debug.LogError("Enable"); 
      currSlot = slot;
      resourceData = rData;
      SetEnabled(true);
    }
    else if (currSlot.item == slot.item) {
      //Debug.LogError("Disable");
      SetEnabled(false);
      currSlot = null;
      resourceData = null;
    }
    else if (currSlot.item != slot.item) {
      //Debug.LogError("Change");
      currSlot = slot;
      resourceData = rData;
      UpdatePreview();
    }
    
    
    //Debug.LogError($"Activate build mode {slot} | {rData}");
    /*SetEnabled(!isPreviewing);
    currSlot = slot;
    resourceData = rData;*/
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
    var renderer = previewInstance.GetComponent<SpriteRenderer>();
    renderer.sprite = resourceData.Sprite(0);
  }

  private void SetPreviewColor(Color col) {
    var renderer = previewInstance.GetComponent<SpriteRenderer>();
    renderer.color = col;
    _currPreviewColor = col;
  }

  //use for condition if we can to place cell
  private bool GetPreviewColor() {
    return _currPreviewColor == previewColor;
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
    
    if (isPreviewing && previewInstance != null)
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

      previewInstance.transform.position = worldPosition;
      /*Debug.LogError($"player {GameManager.instance.PlayerController.PlayerCoords.GetCoords().X} {GameManager.instance.PlayerController.PlayerCoords.GetCoords().Y}");
      Debug.LogError($"grid   {grid.X} {grid.Y}");*/
      if (GameManager.instance.ChunkController.GetCell(grid.X, grid.Y) != null || GameManager.instance.PlayerController.PlayerCoords.GetCoords().Equals(grid)) {
        SetPreviewColor(blockColor);
      }
      else {
        SetPreviewColor(previewColor);
      }
      /*if (Input.GetMouseButtonDown(1))
      {
        CancelPreview();
      }*/
    }
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
    GameManager.instance.ChunkController.SpawnCell(coords,resourceData);
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