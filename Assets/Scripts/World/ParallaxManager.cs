using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;
using Utils;

namespace World {
  public class ParallaxManager : MonoBehaviour {
    [SerializeField] private GameObject borderPrefab;
    [SerializeField] private float bufferZone = 5f;

    [System.Serializable]
    public class ParallaxLayer {
      public GameObject prefab;
      public float speedMultiplier;
      public float minColumnWidth;
      public float maxColumnWidth;
      public float minDistanceBetweenColumns;
      public float maxDistanceBetweenColumns;
    }

    private class ParallaxColumn {
      public GameObject gameObject;
      public List<GameObject> segments;
      public float columnWidth;
      public float segmentHeight;
    }

    public ParallaxLayer[] layers;
    public Camera mainCamera;

    private List<ParallaxColumn>[] activeColumns;
    private Vector2 lastCameraPosition;

    private float screenWidth;
    private float parallaxWidth;
    private float parallaxHeight;
    private float startX;
    private float endX;
    private float topY;
    private float bottomY;

    private Coords previousPlayerCoords;
    private SpriteRenderer borderRenderer;
    private float borderWidth;
    private readonly List<GameObject> borders = new();
    private bool borderLeft;
    private bool borderRight;
    private float leftBorderX;
    private float rightBorderX;
    private CinemachineCamera cinemachineCamera;
    private LockCameraXWhenBorderVisible lockCameraExtension;
    private bool cameraLocked;
    private float cameraLockLeftX;
    private float cameraLockRightX;

    private void Start() {
      lastCameraPosition = mainCamera.transform.position;
      activeColumns = new List<ParallaxColumn>[layers.Length];

      screenWidth = mainCamera.orthographicSize * 2f * Screen.width / Screen.height;
      parallaxWidth = screenWidth * 3f;
      parallaxHeight = mainCamera.orthographicSize * 2f * 3f;

      borderRenderer = borderPrefab.GetComponent<SpriteRenderer>();
      var box = borderPrefab.GetComponent<BoxCollider2D>();
      // borderWidth = borderRenderer.bounds.size.x - (borderRenderer.bounds.size.x - box.size.x) / 2;
      borderWidth = box.size.x;
      var sizeDiff = (GameManager.Instance.GameConfig.CellSizeX -
                      borderWidth) / 2;
      leftBorderX = CoordsTransformer.GridToWorld(-1, 0).x + sizeDiff;
      rightBorderX = CoordsTransformer.GridToWorld(GameManager.Instance.GameConfig.ChunkSizeX - 1, 0).x - sizeDiff;

      cinemachineCamera = GameManager.Instance.StartGameCameraController.cinemachineCamera;
      lockCameraExtension = cinemachineCamera.GetComponent<LockCameraXWhenBorderVisible>();
      cameraLockLeftX = leftBorderX + screenWidth / 2f - borderWidth / 2f;
      cameraLockRightX = rightBorderX - screenWidth / 2f + borderWidth / 2f;

      UpdateVisibleBounds();

      for (var i = 0; i < layers.Length; i++) {
        activeColumns[i] = new List<ParallaxColumn>();
        GenerateInitialColumns(i);
      }
    }

    private void Update() {
      UpdateColumns();
      Parallax();
      UpdateVisibleBounds();
      UpdateBorders();
      UpdateBorderVertical();
      CheckCameraLock();
      lastCameraPosition = mainCamera.transform.position;
    }

    #region Borders

    private void UpdateBorders() {
      var playerCoords = GameManager.Instance.PlayerController.PlayerCoords.GetCoords();

      if (previousPlayerCoords.X == playerCoords.X && previousPlayerCoords.Y == playerCoords.Y) {
        return;
      }

      previousPlayerCoords = playerCoords;

      var cols = GameManager.Instance.GameConfig.ChunkSizeX;
      var visionOffsetX = GameManager.Instance.GameConfig.PlayerAreaWidth / 2;
      var min_x = Mathf.Clamp(playerCoords.X - visionOffsetX, 0, cols - 1);
      var max_x = Mathf.Clamp(playerCoords.X + visionOffsetX, 0, cols - 1);

      if (min_x > 0 && max_x < cols - 1) {
        HideBorder();
        return;
      }

      if (min_x == 0) {
        ShowLeftBorder();
      }
      else {
        ShowRightBorder();
      }
    }

    private void UpdateBorderVertical() {
      if (!borderLeft && !borderRight) {
        return;
      }

      if (borders.Count == 0) {
        return;
      }

      UpdateSegmentsVerticalPositions(borders, borderRenderer.bounds.size.y);
    }

    private void CreateBorders() {
      if (borders.Count > 0) {
        return;
      }

      var borderHeight = borderRenderer.bounds.size.y;
      var fullHeight = parallaxHeight;
      var count = Mathf.CeilToInt(fullHeight / borderHeight) + 2;
      var segmentHeight = borderRenderer.bounds.size.y;

      for (var i = 0; i < count; i++) {
        var segment = Instantiate(borderPrefab, transform);

        var pos = segment.transform.position;
        var y = bottomY + segmentHeight * i;
        segment.transform.position = new Vector3(pos.x, y, pos.z);

        borders.Add(segment);
      }
    }

    private void ShowBorder(bool showLeft) {
      if ((showLeft && borderLeft) || (!showLeft && borderRight)) {
        return;
      }

      CreateBorders();

      var targetX = showLeft ? leftBorderX : rightBorderX;

      foreach (var border in borders) {
        var pos = border.transform.position;
        border.transform.position = new Vector3(targetX, pos.y, pos.z);
        border.SetActive(true);
      }

      borderLeft = showLeft;
      borderRight = !showLeft;
    }

    private void ShowLeftBorder() => ShowBorder(true);

    private void ShowRightBorder() => ShowBorder(false);

    private void HideBorder() {
      if (!borderLeft && !borderRight) {
        return;
      }

      foreach (var border in borders) {
        border.SetActive(false);
      }

      borderLeft = false;
      borderRight = false;
    }

    private void CheckCameraLock() {
      if (cameraLocked) {
        var playerX = GameManager.Instance.PlayerController.transform.position.x;

        if (playerX <= cameraLockLeftX || playerX >= cameraLockRightX) {
          return;
        }

        lockCameraExtension.UnlockCameraX();
        cameraLocked = false;
      }
      else {
        var cameraX = cinemachineCamera.transform.position.x;

        if (cameraX > cameraLockLeftX && cameraX < cameraLockRightX) {
          return;
        }

        lockCameraExtension.LockCameraX(cinemachineCamera.transform.position.x);
        cameraLocked = true;
      }
    }

    #endregion

    private void UpdateVisibleBounds() {
      startX = lastCameraPosition.x - parallaxWidth / 2f;
      endX = lastCameraPosition.x + parallaxWidth / 2f;

      topY = lastCameraPosition.y + parallaxHeight / 2f;
      bottomY = lastCameraPosition.y - parallaxHeight / 2f;
    }

    private void Parallax() {
      var deltaMovement = (Vector2)mainCamera.transform.position - lastCameraPosition;

      if (deltaMovement != Vector2.zero) {
        for (var i = 0; i < layers.Length; i++) {
          MoveLayer(i, deltaMovement);
        }
      }
    }

    private void MoveLayer(int layerIndex, Vector2 deltaMovement) {
      if (layers[layerIndex].speedMultiplier <= 0) {
        return;
      }

      foreach (var column in activeColumns[layerIndex]) {
        column.gameObject.transform.position += new Vector3(deltaMovement.x * layers[layerIndex].speedMultiplier,
          deltaMovement.y * layers[layerIndex].speedMultiplier, 0);
      }
    }

    private void UpdateColumns() {
      for (var i = 0; i < layers.Length; i++) {
        if (activeColumns[i].Count <= 0) {
          return;
        }

        var firstColumn = activeColumns[i][0];
        var lastColumn = activeColumns[i][^1];

        // Видаляємо колони, які вийшли за межі ліворуч
        if ((firstColumn.gameObject.transform.position.x + firstColumn.columnWidth / 2f) < (startX - bufferZone)) {
          MoveFirstColumnRight(i);
          lastColumn = activeColumns[i][^1];
        }

        // Видаляємо колони, які вийшли за межі праворуч
        if ((lastColumn.gameObject.transform.position.x - lastColumn.columnWidth / 2f) > (endX + bufferZone)) {
          MoveLastColumnLeft(i);
        }

        //Вертикальні межі
        foreach (var column in activeColumns[i]) {
          UpdateSegmentsVerticalPositions(column.segments, column.segmentHeight);
        }
      }
    }

    private void UpdateSegmentsVerticalPositions(List<GameObject> segments, float segmentHeight) {
      var bottomSegment = segments[0];
      var topSegment = segments[^1];

      // Перевіряємо нижній сегмент
      if ((bottomSegment.transform.position.y + segmentHeight / 2f) < (bottomY - bufferZone)) {
        MoveSegmentToTop(bottomSegment, topSegment, segmentHeight);
        segments.RemoveAt(0);
        segments.Add(bottomSegment);

        bottomSegment = segments[0];
        topSegment = segments[^1];
      }

      // Перевіряємо верхній сегмент
      if (!((topSegment.transform.position.y - segmentHeight / 2f) > (topY + bufferZone))) {
        return;
      }

      MoveSegmentToBottom(topSegment, bottomSegment, segmentHeight);
      segments.RemoveAt(segments.Count - 1);
      segments.Insert(0, topSegment);
    }

    private void MoveSegmentToTop(GameObject segmentBottom, GameObject segmentTop, float segmentHeight) {
      var columnTop = segmentTop.transform.position.y + segmentHeight / 2f;
      var newY = columnTop + segmentHeight / 2f;

      segmentBottom.transform.position = new Vector3(
        segmentBottom.transform.position.x,
        newY,
        segmentBottom.transform.position.z
      );
    }

    private void MoveSegmentToBottom(GameObject segmentTop, GameObject segmentBottom, float segmentHeight) {
      var columnBottom = segmentBottom.transform.position.y - segmentHeight / 2f;
      var newY = columnBottom - segmentHeight / 2f;

      segmentTop.transform.position = new Vector3(
        segmentTop.transform.position.x,
        newY,
        segmentTop.transform.position.z
      );
    }

    private void MoveFirstColumnRight(int layerIndex) {
      if (activeColumns[layerIndex].Count == 0) {
        return;
      }

      var layer = layers[layerIndex];
      var firstColumn = activeColumns[layerIndex][0];
      var lastColumn = activeColumns[layerIndex][^1];
      var columnObj = firstColumn.gameObject;
      activeColumns[layerIndex].RemoveAt(0);

      var width = firstColumn.columnWidth;

      // Calculate the new X position
      var newX = lastColumn.gameObject.transform.position.x + lastColumn.columnWidth / 2f
                                                            + Random.Range(layer.minDistanceBetweenColumns,
                                                              layer.maxDistanceBetweenColumns) + width / 2f;

      // Update the column's position and resize its segments
      UpdateColumnPositionAndResizeSegments(columnObj, newX);

      // Add the updated column back to the active list
      activeColumns[layerIndex].Add(firstColumn);
    }

    private void MoveLastColumnLeft(int layerIndex) {
      if (activeColumns[layerIndex].Count == 0) {
        return;
      }

      var layer = layers[layerIndex];
      var firstColumn = activeColumns[layerIndex][0];
      var lastColumn = activeColumns[layerIndex][^1];
      activeColumns[layerIndex].RemoveAt(activeColumns[layerIndex].Count - 1);

      var width = lastColumn.columnWidth;

      // Calculate the new X position
      var newX = firstColumn.gameObject.transform.position.x - firstColumn.columnWidth / 2f
                                                             - Random.Range(layer.minDistanceBetweenColumns,
                                                               layer.maxDistanceBetweenColumns) - width / 2f;

      // Update the column's position and resize its segments
      UpdateColumnPositionAndResizeSegments(lastColumn.gameObject, newX);

      // Add the updated column back to the active list
      activeColumns[layerIndex].Insert(0, lastColumn);
    }

    private void UpdateColumnPositionAndResizeSegments(GameObject column, float newX) {
      // Update the column's position
      column.transform.position = new Vector3(newX, column.transform.position.y, 0);
    }

    private void GenerateInitialColumns(int i) {
      var currentX = startX;
      while (currentX < endX) {
        currentX += AddColumn(i, currentX);
      }
    }

    private float AddColumn(int layerIndex, float xPosition) {
      var layer = layers[layerIndex];

      var screenCenterY = mainCamera.transform.position.y; // Центр екрана по вертикалі

      // Створення нової колони
      var columnObj = new GameObject($"Column_{layerIndex}");
      columnObj.transform.position = new Vector3(xPosition, screenCenterY, 0);
      columnObj.transform.parent = transform;

      var columnWidth = Random.Range(layer.minColumnWidth, layer.maxColumnWidth);

      // Висота одного сегмента
      var spriteRenderer = layer.prefab.GetComponent<SpriteRenderer>();
      if (spriteRenderer == null) {
        Debug.LogError("Prefab does not have a SpriteRenderer!");
        return 0;
      }

      var segmentHeight = spriteRenderer.bounds.size.y * columnWidth / spriteRenderer.bounds.size.x;

      // Розрахунок початкової позиції
      var startY = screenCenterY - parallaxHeight / 2f - segmentHeight / 2f;

      var segments = new List<GameObject>();
      // Заповнення колони сегментами
      var currentY = startY;
      while (currentY < screenCenterY + parallaxHeight / 2f + segmentHeight / 2f) {
        var segment = CreateSegment(layer.prefab, columnObj, xPosition, currentY, columnWidth);
        segments.Add(segment);
        currentY += segmentHeight;
      }

      var column = new ParallaxColumn
        { gameObject = columnObj, segments = segments, columnWidth = columnWidth, segmentHeight = segmentHeight };

      // Додавання до активних і зайнятих позицій
      activeColumns[layerIndex].Add(column);

      var randomDistance = Random.Range(layer.minDistanceBetweenColumns, layer.maxDistanceBetweenColumns);
      return randomDistance + columnWidth;
    }

    private GameObject CreateSegment(GameObject prefab, GameObject parent, float xPosition, float yPosition,
      float targetWidth) {
      var segment = Instantiate(prefab, new Vector3(xPosition, yPosition, 0), Quaternion.identity, parent.transform);

      // Масштабування сегменту
      var spriteRenderer = segment.GetComponent<SpriteRenderer>();
      if (spriteRenderer != null) {
        var scale = segment.transform.localScale;
        var spriteWidth = spriteRenderer.bounds.size.x;

        // Масштаб пропорційний до заданої ширини
        var widthScale = targetWidth / spriteWidth;
        scale.x = widthScale;
        scale.y = widthScale;

        segment.transform.localScale = scale;
      }

      return segment;
    }
  }
}