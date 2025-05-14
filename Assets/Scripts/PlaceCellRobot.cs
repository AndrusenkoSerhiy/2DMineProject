using System;
using System.Collections.Generic;
using Player;
using Scriptables;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using World;

public class PlaceCellRobot : MonoBehaviour {
  [SerializeField] private GameObject attackCollider;
  [SerializeField] private PlayerCoords robotCoords;
  private GameManager gameManager;
  [SerializeField] private Coords robotCoordsOutOfBounds;
  [SerializeField] private Coords centerCoords;
  [SerializeField] private Coords leftUpCoords;
  [SerializeField] private ResourceData resourceData;
  [SerializeField] private GameObject previewContainer;
  [SerializeField] private List<SpriteRenderer> previewList = new();
  [SerializeField] private Color blockColor;
  [SerializeField] private Color previewColor;
  [SerializeField] private int curPreview;
  private ChunkController chunkController;
  private void Awake() {
    gameManager = GameManager.Instance;
    chunkController = gameManager.ChunkController;
  }

  private void Update() {
    robotCoordsOutOfBounds = robotCoords.GetCoordsOutOfBounds();
    UpdatePreviewPosition();
    previewContainer.transform.localScale = transform.localScale;
  }
  
  private void UpdatePreviewPosition() {
    var snappedPosition = GetSnappedWorldPosition();
    previewContainer.transform.position = snappedPosition;
    
    centerCoords = CoordsTransformer.MouseToGridPosition(attackCollider.transform.position);
    /*Debug.DrawRay(CoordsTransformer.OutOfGridToWorls(centerCoords.X, centerCoords.Y),
      Vector3.up, Color.green);*/
    FindLeftPoint();
    
    curPreview = 0;
    for (int i = leftUpCoords.X; i < leftUpCoords.X + 3; i++) {
      for (int j = leftUpCoords.Y; j < leftUpCoords.Y + 2; j++) {
        SetPreviewColor(i, j);
      }
    }
  }

  private void SetPreviewColor(int x, int y) {
    if (y < 0 || chunkController.ChunkData.GetCellFill(x, y) == 1 || !CheckRobotSize(x,y)) {
      previewList[curPreview].color = blockColor;
      
      //Debug.DrawRay(CoordsTransformer.OutOfGridToWorls(x,y), Vector3.up, Color.red);
    }
    else {
      previewList[curPreview].color = previewColor;
      //Debug.DrawRay(CoordsTransformer.OutOfGridToWorls(x,y), Vector3.up, Color.green);
    }
    //Debug.LogError($"{x},{y} | elem {previewList[curPreview].name}");
    curPreview++;
  }
  private Vector3 GetSnappedWorldPosition() {
    var grid = CoordsTransformer.MouseToGridPosition(attackCollider.transform.position);    var clampedPosition = grid;
    var world = CoordsTransformer.OutOfGridToWorls(grid.X, grid.Y);
    //Debug.DrawRay(CoordsTransformer.GridToWorld(coords.X, coords.Y), Vector3.up*10, Color.blue);
    return new Vector3(world.x, world.y, 0f);
  }

  private void TryPlaceCell(InputAction.CallbackContext obj) {
    FillCells();
  }

  private void FillCells() {
    for (int i = leftUpCoords.X; i < leftUpCoords.X + 3; i++) {
      for (int j = leftUpCoords.Y; j < leftUpCoords.Y + 2; j++) {
        if (CheckRobotSize(i, j)) {
          Debug.DrawRay(CoordsTransformer.OutOfGridToWorls(i, j),
            Vector3.up, Color.yellow, .5f);
          PlaceBuildingBlock(i, j);
        }
      }
    }
  }

  private bool CheckRobotSize(int x, int y) {
    var robotSizeX = 2;
    var robotSizeY = 2;
    for (int i = robotCoordsOutOfBounds.X; i < robotCoordsOutOfBounds.X + robotSizeX; i++) {
      for (int j = robotCoordsOutOfBounds.Y; j > robotCoordsOutOfBounds.Y - robotSizeY; j--) {
        /*var pos = CoordsTransformer.GridToWorld(i, j);
        Debug.DrawRay(pos, Vector3.up, Color.yellow);*/
        if (x == i && y == j) return false;
      }
    }
    return true;
  }

  private void FindLeftPoint() {
    leftUpCoords = centerCoords;
    leftUpCoords.X -= 1;
  }
  
  private void PlaceBuildingBlock(int x, int y) {
    //Debug.LogError($"Place {x} | {y}");
    if(y < 0 || chunkController.ChunkData.GetCellFill(x, y) == 1)
      return;
    
    var cell = chunkController.ChunkData.ForceCellFill(resourceData, x, y);
    chunkController.AfterCellChanged(cell);
    chunkController.UpdateCellAround(x, y);
  }

  public void Activate() {
    enabled = true;
    gameManager.UserInput.controls.UI.RightClick.performed += TryPlaceCell;
    ShowPreview(true);
  }

  private void ShowPreview(bool state) {
    previewContainer.SetActive(state);
  }

  public void Deactivate() {
    enabled = false;
    ShowPreview(false);
    gameManager.UserInput.controls.UI.RightClick.performed -= TryPlaceCell;
  }
}