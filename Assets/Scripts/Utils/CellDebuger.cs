using UnityEngine;

namespace Utils {
  public class CellDebuger : MonoBehaviour {
    public int X;
    public int Y;
    public float Perlin;

    
    [ContextMenu("Get Perlin")]
    public void CheckCell() {
      Perlin = GameManager.Instance.ChunkController.ChunkData.GetCellData(X,Y).perlin;
    }
  }
}