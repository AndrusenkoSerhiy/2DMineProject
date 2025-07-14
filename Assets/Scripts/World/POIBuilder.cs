using Scriptables.POI;
using UnityEngine;

namespace World {
  public class POIBuilder : MonoBehaviour {
    [Header("References")] public POIBuilderConfig config;
    public Texture2D texture;
    public POISaver saver;

    [ContextMenu("Build POIs From Texture")]
    public void BuildPOIs() {
      if (config == null || texture == null) {
        Debug.LogError("Missing config, texture, or prefab.");
        return;
      }

      saver.Name = texture.name;
      saver.SizeX = texture.width;
      saver.SizeY = texture.height;
      foreach (var oldcell in saver.targetCellObjects) {
        DestroyImmediate(oldcell.gameObject);
      }

      saver.targetCellObjects.Clear();
      // Go through each pixel
      for (int y = 0; y < texture.height; y++) {
        for (int x = 0; x < texture.width; x++) {
          Color pixelColor = texture.GetPixel(x, y);

          // Skip white or fully transparent pixels
          if (pixelColor == Color.white)
            continue;

          // Try to find a matching data entry
          POIBuilderData matchedData = config.POIBuilderDataList.Find(data =>
            ColorsApproximatelyEqual(data.colorRef, pixelColor)
          );

          if (matchedData != null) {
            Vector3 spawnPosition = new Vector3(
              x * config.stepX,
              y * config.stepY,
              0f
            );

            // Instantiate the prefab
            CellObject instance = Instantiate(config.prefab, spawnPosition, Quaternion.identity, this.transform);

            // Apply resource and sprite (custom logic based on your CellObject)
            instance.resourceData = matchedData.resourceDataRef;
            instance.sprite.sprite = matchedData.spriteRef;
            saver.targetCellObjects.Add(instance.GetComponent<POICellObject>());
          }
        }
      }
      saver.SavePOIData();
      Debug.Log("POIs generated from texture.");
    }

    private bool ColorsApproximatelyEqual(Color a, Color b, float tolerance = 0.01f) {
      return Mathf.Abs(a.r - b.r) < tolerance &&
             Mathf.Abs(a.g - b.g) < tolerance &&
             Mathf.Abs(a.b - b.b) < tolerance;
    }
  }
}