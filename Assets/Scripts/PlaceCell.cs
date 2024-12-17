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
    UserInput.instance.OnUIClick += UIInput_OnUIClick;
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
    UserInput.instance.EnableGamePlayControls(!value);
    UserInput.instance.EnableUIControls(value);
    Debug.LogError($"ui controls {value}");

    if (value) StartPreview();
    else CancelPreview();
  }
  
  private void Update() {
    if (Input.GetKeyDown(KeyCode.PageUp)) {
      SetEnabled(true);
    }

    if (Input.GetKeyDown(KeyCode.PageDown)) {
      SetEnabled(false);
    }
    
    if (_isPreviewing && _previewInstance != null)
    {
      // Update the position of the preview to follow the mouse position
      Vector3 mousePosition = UserInput.instance.GetMousePosition();
      mousePosition.z = 0f; 
      Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

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

  private void UIInput_OnUIClick(object sender, EventArgs e) {
    if (!_isPreviewing || !GetPreviewColor()) {
      return; 
    }
    
    PlaceCellOnScene();
  }

  private void PlaceCellOnScene() {
    var coords = CoordsTransformer.WorldToGrid(Camera.main.ScreenToWorldPoint(UserInput.instance.GetMousePosition()));
    GameManager.instance.ChunkController.SpawnCell(coords,_resourceData);
  }

  private void OnDestroy() {
    UserInput.instance.OnUIClick -= UIInput_OnUIClick;
  }
}