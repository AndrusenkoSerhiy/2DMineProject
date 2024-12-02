using UnityEngine;
using Utils;

namespace World {
  public class ChunkGenerator : MonoBehaviour {
    private ChunkData[] _chunkObjects;

    public void Init() {
      GenerateChunkDatas();
    }

    void GenerateChunkDatas() {
      _chunkObjects = new ChunkData[1];
      GenerateChunk(0, 0);
    }

    void GenerateChunk(int x, int y) {
      var key = new Coords(x, y);
      _chunkObjects[0] = new ChunkData(key, x, y);
    }

    public ChunkData GetChunk(int x, int y) {
      var key = new Coords(x, y);
      for (int i = 0; i < _chunkObjects.Length; i++) {
        if (_chunkObjects[i].id.Equals(key)) return _chunkObjects[i];
      }

      return null;
    }
  }
}