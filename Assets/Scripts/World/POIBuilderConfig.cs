using System;
using System.Collections.Generic;
using Scriptables;
using UnityEngine;

namespace World {
  [Serializable]
  public class POIBuilderData {
    public ResourceData resourceDataRef;
    public Color colorRef;
    public Sprite spriteRef;
  }
  [CreateAssetMenu(menuName = "Create POIBuilderConfig", fileName = "POIBuilderConfig", order = 0)]
  public class POIBuilderConfig: ScriptableObject {
    public CellObject prefab;
    public float stepX;
    public float stepY;
    public List<POIBuilderData> POIBuilderDataList;
  }
}