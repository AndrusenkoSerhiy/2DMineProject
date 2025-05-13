using System;
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
  private ChunkController chunkController;
  private void Awake() {
    gameManager = GameManager.Instance;
    chunkController = gameManager.ChunkController;
  }

  private void Update() {
    robotCoordsOutOfBounds = robotCoords.GetCoordsOutOfBounds();
    if (Input.GetKeyDown(KeyCode.O)) {
      CheckRobotSize(1, 1);
    }
  }

  private void TryPlaceCell(InputAction.CallbackContext obj) {
    centerCoords = CoordsTransformer.MouseToGridPosition(attackCollider.transform.position);
    Debug.DrawRay(CoordsTransformer.OutOfGridToWorls(centerCoords.X, centerCoords.Y),
      Vector3.up, Color.green, 5f);
    //place under
    if (robotCoordsOutOfBounds.Y < centerCoords.Y) {
      PlaceUnder();
    }
    else if (robotCoordsOutOfBounds.Y < centerCoords.Y) {
      //Debug.LogError("TryPlaceCell Under");
      PlaceUnder();
    }
    else if (centerCoords.Y <= robotCoordsOutOfBounds.Y && Mathf.Abs(centerCoords.Y - robotCoordsOutOfBounds.Y) >= 2) {
      //Debug.LogError($"TryPlaceCell Above {Mathf.Abs(centerCoords.Y - robotCoordsOutOfBounds.Y)}");
      PlaceAbove();
    }
    else if (Mathf.Abs(centerCoords.Y - robotCoordsOutOfBounds.Y) < 3) {
      //Debug.LogError("TryPlaceCell OnTheSameLevel");
      PlaceTheSameLevel();
    }
    
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
        if (x == i && y == j) return false;
      }
    }
    return true;
  }

  private void PlaceUnder() {
    if (Mathf.Abs(robotCoordsOutOfBounds.Y - centerCoords.Y) == 1) {
      leftUpCoords = centerCoords;
      leftUpCoords.X -= 1;
    }
    if (Mathf.Abs(robotCoordsOutOfBounds.Y - centerCoords.Y) == 2) {
      leftUpCoords = centerCoords;
      leftUpCoords.X -= 1;
      leftUpCoords.Y -= 1;
    }
  }
  
  private void PlaceAbove() {
    if (Mathf.Abs(robotCoordsOutOfBounds.Y - centerCoords.Y) == 2) {
      leftUpCoords = centerCoords;
      leftUpCoords.X -= 1;
      leftUpCoords.Y -= 1;
    }
    if (Mathf.Abs(robotCoordsOutOfBounds.Y - centerCoords.Y) == 3) {
      leftUpCoords = centerCoords;
      leftUpCoords.X -= 1;
    }
  }

  private void PlaceTheSameLevel() {
    leftUpCoords = centerCoords;
    leftUpCoords.X -= 1;
    leftUpCoords.Y -= 1;
  }
  
  private void PlaceBuildingBlock(int x, int y) {
    if(y < 0 || chunkController.ChunkData.GetCellFill(x, y) == 1)
      return;
    
    var cell = chunkController.ChunkData.ForceCellFill(resourceData, x, y);
    chunkController.AfterCellChanged(cell);
    chunkController.UpdateCellAround(x, y);
  }

  public void Activate() {
    enabled = true;
    gameManager.UserInput.controls.UI.RightClick.performed += TryPlaceCell;
  }

  public void Deactivate() {
    enabled = false;
    gameManager.UserInput.controls.UI.RightClick.performed -= TryPlaceCell;
  }
}