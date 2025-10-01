using System;
using System.Collections.Generic;
using Player;
using Scriptables;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using World;
using DG.Tweening;

public class PlaceCellRobot : MonoBehaviour {
  [SerializeField] private GameObject attackCollider;
  [SerializeField] private PlayerCoords robotCoords;
  private GameManager gameManager;
  [SerializeField] private Coords robotCoordsOutOfBounds;
  [SerializeField] private Coords centerCoords;
  [SerializeField] private Coords leftUpCoords;
  [SerializeField] private GameObject previewContainer;
  [SerializeField] private List<SpriteRenderer> previewList = new();
  private Color blockColor;
  private Color previewColor;
  [SerializeField] private int curPreview;
  [SerializeField] private List<ResourceData> possibleResourceList;
  [SerializeField] private int activeBlockIndex;
  [SerializeField] private RobotPlaceCellInfo blockInfo;
  private ChunkController chunkController;
  
  private void Awake() {
    gameManager = GameManager.Instance;
    chunkController = gameManager.ChunkController;
  }

  private void Start() {
    SetSpriteFromData();
  }

  private void Update() {
    robotCoordsOutOfBounds = robotCoords.GetCoordsOutOfBounds();
    UpdatePreviewPosition();
    previewContainer.transform.localScale = transform.localScale;
  }

  private void SetSpriteFromData() {
    var newSprite = possibleResourceList[activeBlockIndex].Sprite(0);
    for (int i = 0; i < previewList.Count; i++) {
      previewList[i].sprite = newSprite;
    }
    previewColor = possibleResourceList[activeBlockIndex].PreviewColor;
    blockColor = possibleResourceList[activeBlockIndex].BlockColor;
  }

  public void UpdateBlockType() {
    if (activeBlockIndex < possibleResourceList.Count - 1) {
      activeBlockIndex++;
    }else activeBlockIndex = 0;
    //Debug.LogError($"activeBlockIndex {activeBlockIndex}");
    SetSpriteFromData();
    UpdatePreviewCount();
  }
  
  private void UpdatePreviewPosition() {
    var snappedPosition = GetSnappedWorldPosition();
    previewContainer.transform.position = snappedPosition;
    
    centerCoords = CoordsTransformer.MouseToGridPosition(attackCollider.transform.position);
    /*Debug.DrawRay(CoordsTransformer.OutOfGridToWorls(centerCoords.X, centerCoords.Y),
      Vector3.up, Color.green);*/
    FindLeftPoint();
    
    var itemCount = gameManager.PlayerInventory.GetInventory().GetTotalCount(possibleResourceList[activeBlockIndex].ItemData.Id);
    var ySize = Mathf.Min(2, itemCount - 3 <= 0 ? 1 : itemCount);
    var xSize = Mathf.Min(3, itemCount - ySize <= 0 ? 1 : itemCount);
    //Debug.LogError($"ySize {ySize} xSize {xSize}");
    curPreview = 0;
    for (int j = leftUpCoords.Y; j < leftUpCoords.Y + ySize; j++) {
      for (int i = leftUpCoords.X; i < leftUpCoords.X + xSize; i++) {
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
    foreach (var sprite in previewList) {
      if (sprite.gameObject.activeInHierarchy && sprite.color != blockColor) {
        var coords = CoordsTransformer.MouseToGridPosition(sprite.transform.position);
        PlaceBuildingBlock(coords.X, coords.Y);
      }
    }
    UpdatePreviewCount();
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
    
    var cell = chunkController.ChunkData.ForceCellFill(possibleResourceList[activeBlockIndex], x, y);
    chunkController.AfterCellChanged(cell);
    chunkController.UpdateCellAround(x, y);
    gameManager.PlayerInventory.InventoriesPool.RemoveFromInventoriesPool(possibleResourceList[activeBlockIndex].ItemData.Id, 1);
    
    gameManager.AudioController.PlayPlaceBuildingBlock();
    GameManager.Instance.ChunkController.CheckArea();

    //Tween for spawn effect
    var spawnedGo = gameManager.ChunkController.GetCell(x, y);
    var spriteRenderer = spawnedGo.GetComponentInChildren<SpriteRenderer>();

    var block = new MaterialPropertyBlock();
    spriteRenderer.GetPropertyBlock(block);
    block.SetFloat("_Dissolve", 0f);
    spriteRenderer.SetPropertyBlock(block);

    DOTween.To(() => block.GetFloat("_Dissolve"),
      x => {
        block.SetFloat("_Dissolve", x);
        spriteRenderer.SetPropertyBlock(block);
      },
      1f, 0.4f);

    GameManager.Instance.PoolEffects.SpawnFromPool("PlaceCellEffect", spawnedGo.transform.position,
      Quaternion.identity);
  }

  public void Activate() {
    enabled = true;
    gameManager.UserInput.controls.UI.Click.performed += TryPlaceCell;
    ShowPreview(true);
    blockInfo.Show();
    UpdateBlockInfo();
  }

  //update ui for current block
  private void UpdateBlockInfo() {
    var sprite = possibleResourceList[activeBlockIndex].Sprite(15);
    var itemCount = gameManager.PlayerInventory.GetInventory()
      .GetTotalCount(possibleResourceList[activeBlockIndex].ItemData.Id);
    
    blockInfo.SetImage(sprite);
    blockInfo.SetAmmo(itemCount);
  }

  private void ShowPreview(bool state) {
    previewContainer.SetActive(state);
    UpdatePreviewCount();
  }

  //how many block we have and need to show
  private void UpdatePreviewCount() {
    var itemCount = gameManager.PlayerInventory.GetInventory()
      .GetTotalCount(possibleResourceList[activeBlockIndex].ItemData.Id);

    var count = Mathf.Min(previewList.Count, itemCount);
    for (int i = 0; i < count; i++) {
      previewList[i].gameObject.SetActive(true);
    }

    for (int i = count; i < previewList.Count; i++) {
      previewList[i].gameObject.SetActive(false);
    }
    UpdateBlockInfo();
  }

  public void Deactivate() {
    enabled = false;
    ShowPreview(false);
    gameManager.UserInput.controls.UI.Click.performed -= TryPlaceCell;
    blockInfo.Hide();
  }
}