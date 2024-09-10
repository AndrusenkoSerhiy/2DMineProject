using System;
using System.Collections.Generic;
using UnityEngine;

namespace World{
  public class ChunkGenerator : MonoBehaviour{
    public int SectorsStartRangeX = 10;
    public int SectorsStartRangeY = 10;
    private Dictionary<Tuple<int,int>, ChunkData> _chunkObjects;

    public void Init(){
      GenerateChunkDatas();
    }

    void GenerateChunkDatas(){
      _chunkObjects = new Dictionary<Tuple<int, int>, ChunkData>();
      for (int x = -SectorsStartRangeX; x <= SectorsStartRangeX; x++){
        for (int y = 0; y <= SectorsStartRangeY; y++) {
          GenerateChunk(y, x);
        }
      }
    }

    void GenerateChunk(int y, int x) {
      var key = Tuple.Create(x, y);
      _chunkObjects[key] = new ChunkData(key, x, y);
    }

    public ChunkData GetChunk(int y, int x){
      var key = Tuple.Create(y, x);
      return _chunkObjects.ContainsKey(key) ? _chunkObjects[key] : null;
    }
  }
}