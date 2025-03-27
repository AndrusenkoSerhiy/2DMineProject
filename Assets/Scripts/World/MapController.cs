using System.IO;
using UnityEngine;

namespace World {
  public class MapController : MonoBehaviour {
    private Texture2D mapTexture;
    public Texture2D MapTexture => mapTexture;
    
    public void GenerateTexture() {
      var width = GameManager.Instance.GameConfig.ChunkSizeX;
      var height = GameManager.Instance.GameConfig.ChunkSizeY;
      var chunkData = GameManager.Instance.ChunkController.ChunkData;
      mapTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
      for (int i = 0; i < width; i++) {
        for (int j = 0; j < height; j++) {
          // Check the Perlin noise value
          var color =
            GameManager.Instance.ChunkController.ResourceDataLibrary.GetColor(chunkData.GetCellData(i, j).perlin);
          // Set the pixel color at (j, i)
          mapTexture.SetPixel(width - 1 - i, height - 1 - j, color);
        }
      }

      // Apply all SetPixel changes to the texture
      mapTexture.Apply();
    }


    [ContextMenu("Create TextureMap In Assets(PLAYMODE ONLY)")]
    public void SaveTexture() {
      byte[] pngData = mapTexture.EncodeToPNG();
      if (pngData != null) {
        // Write the PNG file to the specified path
        string path = Path.Combine(Application.persistentDataPath, "GeneratedTexture.png");
        File.WriteAllBytes(path, mapTexture.EncodeToPNG());
        Debug.Log("Texture saved : " + Application.persistentDataPath + "GeneratedTexture.png");
      }
      else {
        Debug.LogError("Failed to encode texture to PNG.");
      }
    }

    public void ModifyMapTexture(int x, int y, Color color) {
      mapTexture.SetPixel(x, y, color);
    }
  }
}