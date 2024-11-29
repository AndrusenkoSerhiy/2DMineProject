using System;
using System.Collections.Generic;
using UnityEngine;

namespace World{
  public class ChunkGenerator : MonoBehaviour{
    private Dictionary<Tuple<int,int>, ChunkData> _chunkObjects;

    public void Init(){
      GenerateChunkDatas();
    }

    void GenerateChunkDatas(){
      _chunkObjects = new Dictionary<Tuple<int, int>, ChunkData>(); 
      GenerateChunk(0, 0);
    }

    void GenerateChunk(int y, int x) {
      var key = Tuple.Create(x, y);
      _chunkObjects[key] = new ChunkData(key, x, y);
    }

    public ChunkData GetChunk(Tuple<int, int> chunk) => _chunkObjects[chunk];

    public ChunkData GetChunk(int y, int x){
      var key = Tuple.Create(y, x);
      return _chunkObjects.ContainsKey(key) ? _chunkObjects[key] : null;
    }
  }
}