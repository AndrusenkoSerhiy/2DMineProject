using UnityEngine;
using System.Collections.Generic;

namespace World {
  public class ParallaxManager : MonoBehaviour {
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

    float screenWidth;
    float parallaxWidth;
    float parallaxHeight;
    float startX;
    float endX;
    float topY;
    float bottomY;

    void Start() {
      //layers = new ParallaxLayer[1] { layers[0] };
      lastCameraPosition = mainCamera.transform.position;
      activeColumns = new List<ParallaxColumn>[layers.Length];

      screenWidth = mainCamera.orthographicSize * 2f * Screen.width / Screen.height;
      parallaxWidth = screenWidth * 3f;
      parallaxHeight = mainCamera.orthographicSize * 2f * 3f;

      UpdateVisibleBounds();

      // GameObject startXObj = new GameObject($"startX");
      // startXObj.transform.position = new Vector3(startX, mainCamera.transform.position.y, 0);
      // GameObject endXObj = new GameObject($"endX");
      // endXObj.transform.position = new Vector3(endX, mainCamera.transform.position.y, 0);
      // GameObject topYObj = new GameObject($"topY");
      // topYObj.transform.position = new Vector3(mainCamera.transform.position.x, topY, 0);
      // GameObject bottomYObj = new GameObject($"bottomY");
      // bottomYObj.transform.position = new Vector3(mainCamera.transform.position.x, bottomY, 0);

      for (int i = 0; i < layers.Length; i++) {
        activeColumns[i] = new List<ParallaxColumn>();
        GenerateInitialColumns(i);
      }
    }

    void Update() {
      UpdateColumns();
      Parallax();
      UpdateVisibleBounds();
      lastCameraPosition = mainCamera.transform.position;
    }

    private void UpdateVisibleBounds() {
      startX = lastCameraPosition.x - parallaxWidth / 2f;
      endX = lastCameraPosition.x + parallaxWidth / 2f;

      topY = lastCameraPosition.y + parallaxHeight / 2f;
      bottomY = lastCameraPosition.y - parallaxHeight / 2f;
    }

    private void Parallax() {
      Vector2 deltaMovement = (Vector2)mainCamera.transform.position - lastCameraPosition;

      if (deltaMovement != Vector2.zero) {
        for (int i = 0; i < layers.Length; i++) {
          MoveLayer(i, deltaMovement);
        }
      }
    }

    private void MoveLayer(int layerIndex, Vector2 deltaMovement) {
      foreach (ParallaxColumn column in activeColumns[layerIndex]) {
        column.gameObject.transform.position += new Vector3(deltaMovement.x * layers[layerIndex].speedMultiplier, deltaMovement.y * layers[layerIndex].speedMultiplier, 0);
      }
    }

    private void UpdateColumns() {
      float bufferZone = 5f;
      for (int i = 0; i < layers.Length; i++) {
        if (activeColumns[i].Count <= 0) return;

        ParallaxColumn firstColumn = activeColumns[i][0];
        ParallaxColumn lastColumn = activeColumns[i][^1];

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
        foreach (ParallaxColumn column in activeColumns[i]) {
          // Перевіряємо нижній сегмент
          GameObject bottomSegment = column.segments[0];
          GameObject topSegment = column.segments[^1];

          // Debug.Log("bottomSegment " + (bottomSegment.transform.position.y + column.segmentHeight / 2f));
          // Debug.Log("bottomY - bufferZone " + (bottomY - bufferZone));

          if ((bottomSegment.transform.position.y + column.segmentHeight / 2f) < (bottomY - bufferZone)) {
            MoveSegmentToTop(bottomSegment, topSegment, column);
            column.segments.RemoveAt(0);
            column.segments.Add(bottomSegment);

            bottomSegment = column.segments[0];
            topSegment = column.segments[^1];
          }

          // Debug.Log("topSegment " + (topSegment.transform.position.y - column.segmentHeight / 2f));
          // Debug.Log("topY + bufferZone " + (topY + bufferZone));
          // Перевіряємо верхній сегмент
          if ((topSegment.transform.position.y - column.segmentHeight / 2f) > (topY + bufferZone)) {
            MoveSegmentToBottom(topSegment, bottomSegment, column);
            column.segments.RemoveAt(column.segments.Count - 1);
            column.segments.Insert(0, topSegment);
          }
        }
      }
    }

    private void MoveSegmentToTop(GameObject segmentBottom, GameObject segmentTop, ParallaxColumn column) {
      float columnTop = segmentTop.transform.position.y + column.segmentHeight / 2f;
      float newY = columnTop + column.segmentHeight / 2f;

      segmentBottom.transform.position = new Vector3(
          segmentBottom.transform.position.x,
          newY,
          segmentBottom.transform.position.z
      );

      // Debug.Log($"MoveSegmentToTop: New Y = {newY}");
    }

    private void MoveSegmentToBottom(GameObject segmentTop, GameObject segmentBottom, ParallaxColumn column) {
      float columnBottom = segmentBottom.transform.position.y - column.segmentHeight / 2f;
      float newY = columnBottom - column.segmentHeight / 2f;

      segmentTop.transform.position = new Vector3(
          segmentTop.transform.position.x,
          newY,
          segmentTop.transform.position.z
      );

      // Debug.Log($"MoveSegmentToBottom: New Y = {newY}");
    }

    private void MoveFirstColumnRight(int layerIndex) {
      if (activeColumns[layerIndex].Count == 0) {
        return;
      }

      ParallaxLayer layer = layers[layerIndex];
      ParallaxColumn firstColumn = activeColumns[layerIndex][0];
      ParallaxColumn lastColumn = activeColumns[layerIndex][^1];
      GameObject columnObj = firstColumn.gameObject;
      activeColumns[layerIndex].RemoveAt(0);

      float width = firstColumn.columnWidth;

      // Calculate the new X position
      float newX = lastColumn.gameObject.transform.position.x + lastColumn.columnWidth / 2f
        + Random.Range(layer.minDistanceBetweenColumns, layer.maxDistanceBetweenColumns) + width / 2f;

      // Update the column's position and resize its segments
      UpdateColumnPositionAndResizeSegments(columnObj, newX);

      // Add the updated column back to the active list
      activeColumns[layerIndex].Add(firstColumn);
      // Debug.Log("MoveFirstColumnRight");
    }

    private void MoveLastColumnLeft(int layerIndex) {
      if (activeColumns[layerIndex].Count == 0) {
        return;
      }

      ParallaxLayer layer = layers[layerIndex];
      ParallaxColumn firstColumn = activeColumns[layerIndex][0];
      ParallaxColumn lastColumn = activeColumns[layerIndex][^1];
      activeColumns[layerIndex].RemoveAt(activeColumns[layerIndex].Count - 1);

      float width = lastColumn.columnWidth;

      // Calculate the new X position
      float newX = firstColumn.gameObject.transform.position.x - firstColumn.columnWidth / 2f
        - Random.Range(layer.minDistanceBetweenColumns, layer.maxDistanceBetweenColumns) - width / 2f;

      // Update the column's position and resize its segments
      UpdateColumnPositionAndResizeSegments(lastColumn.gameObject, newX);

      // Add the updated column back to the active list
      activeColumns[layerIndex].Insert(0, lastColumn);
      // Debug.Log("MoveLastColumnLeft");
    }

    private void UpdateColumnPositionAndResizeSegments(GameObject column, float newX) {
      // Update the column's position
      column.transform.position = new Vector3(newX, column.transform.position.y, 0);
    }

    private void GenerateInitialColumns(int i) {
      float currentX = startX;
      while (currentX < endX) {
        currentX += AddColumn(i, currentX);
      }
    }

    private float AddColumn(int layerIndex, float xPosition) {
      ParallaxLayer layer = layers[layerIndex];

      float screenCenterY = mainCamera.transform.position.y; // Центр екрана по вертикалі

      // Створення нової колони
      GameObject columnObj = new GameObject($"Column_{layerIndex}");
      columnObj.transform.position = new Vector3(xPosition, screenCenterY, 0);
      columnObj.transform.parent = transform;

      float columnWidth = Random.Range(layer.minColumnWidth, layer.maxColumnWidth);

      // Висота одного сегмента
      SpriteRenderer spriteRenderer = layer.prefab.GetComponent<SpriteRenderer>();
      if (spriteRenderer == null) {
        Debug.LogError("Prefab does not have a SpriteRenderer!");
        return 0;
      }
      float segmentHeight = spriteRenderer.bounds.size.y * columnWidth / spriteRenderer.bounds.size.x;

      // Розрахунок початкової позиції
      float startY = screenCenterY - parallaxHeight / 2f - segmentHeight / 2f;

      List<GameObject> segments = new List<GameObject>();
      // Заповнення колони сегментами
      float currentY = startY;
      while (currentY < screenCenterY + parallaxHeight / 2f + segmentHeight / 2f) {
        GameObject segment = CreateSegment(layer.prefab, columnObj, xPosition, currentY, columnWidth);
        segments.Add(segment);
        currentY += segmentHeight;
      }

      ParallaxColumn column = new ParallaxColumn { gameObject = columnObj, segments = segments, columnWidth = columnWidth, segmentHeight = segmentHeight };

      // Додавання до активних і зайнятих позицій
      activeColumns[layerIndex].Add(column);

      float randomDistance = Random.Range(layer.minDistanceBetweenColumns, layer.maxDistanceBetweenColumns);
      return randomDistance + columnWidth;
    }

    private GameObject CreateSegment(GameObject prefab, GameObject parent, float xPosition, float yPosition, float targetWidth) {
      GameObject segment = Instantiate(prefab, new Vector3(xPosition, yPosition, 0), Quaternion.identity, parent.transform);

      // Масштабування сегменту
      SpriteRenderer spriteRenderer = segment.GetComponent<SpriteRenderer>();
      if (spriteRenderer != null) {
        Vector3 scale = segment.transform.localScale;
        float spriteWidth = spriteRenderer.bounds.size.x;

        // Масштаб пропорційний до заданої ширини
        float widthScale = targetWidth / spriteWidth;
        scale.x = widthScale;
        scale.y = widthScale;

        segment.transform.localScale = scale;
      }

      return segment;
    }
  }
}