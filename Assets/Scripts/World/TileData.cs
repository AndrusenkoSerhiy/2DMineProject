using System;
using UnityEngine;

namespace World {
  [Serializable]
  public class TileData {
    [SerializeField] private Sprite sprite;
    [SerializeField] private int offsetSorting;
    [SerializeField] private Vector3 offsetPosition;
    [SerializeField] private Vector3 offsetCollider;

    public Sprite Sprite => sprite;
    public int OffsetSorting => offsetSorting;
    public Vector3 OffsetPosition => offsetPosition;
    public Vector3 OffsetCollider => offsetCollider;
  }
}