using System;
using Scriptables;
using Settings;
using UnityEngine;
using UnityEngine.Serialization;
using World;

public class PlaceCell : MonoBehaviour {
  [SerializeField] private ResourceData _resourceData;
  [SerializeField] private GameObject _prefab;
  private GameObject _previewInstance;
  [SerializeField] private bool _isPreviewing;
  [SerializeField] private Color _previewColor;
  [SerializeField] private Color _blockColor;
  private Color _currPreviewColor;
  private void Start() {
    UserInput.instance.OnBuildClick += Input_OnBuildClick;
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
    //UserInput.instance.EnableGamePlayControls(!value);
    //TODO need to block only attack 
    UserInput.instance.EnableUIControls(value);

    if (value) StartPreview();
    else CancelPreview();
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
      if (GameManager.instance.ChunkController.GetCell(grid.X, grid.Y) != null || GameManager.instance.PlayerController.PlayerCoords.GetCoords().Equals(grid)) {
        SetPreviewColor(_blockColor);
      }
      else {
        SetPreviewColor(_previewColor);
      }
      if (Input.GetMouseButtonDown(1))
      {
        CancelPreview();
      }
    }
  }

  private void UIInput_OnUIClick() {
    if (!_isPreviewing || !GetPreviewColor()) {
      return; 
    }
    
    PlaceCellOnScene();
  }

  private void Input_OnBuildClick(object sender, EventArgs e) {
    SetEnabled(!_isPreviewing);
  }

  private void PlaceCellOnScene() {
    var coords = CoordsTransformer.WorldToGrid(GetMousePosition());
    GameManager.instance.ChunkController.SpawnCell(coords,_resourceData);
  }

  private Vector3 GetMousePosition() {
    return GameManager.instance.MainCamera.ScreenToWorldPoint(UserInput.instance.GetMousePosition());
  }
}