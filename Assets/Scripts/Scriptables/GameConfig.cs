using UnityEngine;
using UnityEngine.Serialization;

namespace Scriptables {
  [CreateAssetMenu(fileName = "GameConfig", menuName = "GameConfig")]
  public class GameConfig : ScriptableObject {
    [Header("ChunkParameters")] [SerializeField]
    private int chunkSizeX = 1000;

    public int ChunkSizeX => chunkSizeX;
    [SerializeField] private int chunkSizeY = 1000;
    public int ChunkSizeY => chunkSizeY;

    [Header("Chunk Pivot")] [SerializeField]
    private int originCol = 500;

    public int OriginCol => originCol;
    [SerializeField] private int originRow = 0;
    public int OriginRow => originRow;

    [Header("Cell Parameters")] [SerializeField]
    private float cellSizeX = 1.32f;

    public float CellSizeX => cellSizeX;
    [SerializeField] private float cellSizeY = 1.3f;
    public float CellSizeY => cellSizeY;

    [Header("Generation Parameters")] [SerializeField]
    private float perlinScale = 35f;

    public float PerlinScale => perlinScale;

    [Header("Playable Area Parameters")] [SerializeField]
    private int playerAreaWidth = 80;

    public int PlayerAreaWidth => playerAreaWidth;
    [SerializeField] private int playerAreaHeight = 80;
    public int PlayerAreaHeight => playerAreaHeight;

    [SerializeField] private float checkAreaStep = 10f;
    public float CheckAreaStep => checkAreaStep;
  }
}