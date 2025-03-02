using System;
using UnityEngine;

namespace World {
  [Serializable]
  public class TileData {
    [SerializeField] private Sprite sprite;
    [SerializeField] private int offsetSorting;
    public Sprite Sprite => sprite;
    public int OffsetSorting => offsetSorting;
  }
}