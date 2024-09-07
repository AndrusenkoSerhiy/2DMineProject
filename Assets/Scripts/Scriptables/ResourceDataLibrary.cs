using System;
using UnityEngine;

namespace Scriptables {
  [Serializable]
  public class ResourceDataObject {
    public ResourceData Data;
    public Vector2 PerlinRange;
  }

  [CreateAssetMenu(menuName = "Create ResourceDataLibrary", fileName = "ResourceDataLibrary", order = 0)]
  public class ResourceDataLibrary : ScriptableObject {
    [SerializeField] private ResourceDataObject[] Resources;

    public ResourceData GetData(float perlinValue) {
      for (int i = 0; i < Resources.Length; i++) {
        if (Resources[i].PerlinRange.x > perlinValue) continue;
        if (Resources[i].PerlinRange.y < perlinValue) continue;
        return Resources[i].Data;
      }

      return null;
    }
  }
}