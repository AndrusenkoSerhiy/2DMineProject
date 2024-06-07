using System.Collections.Generic;
using UnityEngine;

namespace World{
  public class ChunkGenerator : MonoBehaviour{
    public int s_width = 10;
    public int s_height = 10;
    [SerializeField] private ChunkData[,] _chunkObjects;
    [SerializeField] private List<ChunkData> debugList = new();

    public void Init(){
      GenerateChunkDatas();
    }

    void GenerateChunkDatas(){
      _chunkObjects = new ChunkData[s_width, s_height];
      int id = 0;
      for (int x = 0; x < s_width; x++){
        for (int y = 0; y < s_height; y++){
          _chunkObjects[x, y] = new ChunkData(x, y, id);
          debugList.Add(_chunkObjects[x, y]);
          id++;
        }
      }
    }

    public ChunkData GetChunk(int x, int y){
      return _chunkObjects[x, y];
    }
  }
}