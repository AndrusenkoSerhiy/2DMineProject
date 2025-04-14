using UnityEngine;

namespace Scriptables {
  [CreateAssetMenu(menuName = "Buildings/Create Building", fileName = "Building")]
  public class Building : ScriptableObject {
    public int SizeX;
    public int SizeY;
  }
}