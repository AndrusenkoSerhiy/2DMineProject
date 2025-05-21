using UnityEngine;

namespace Scriptables {
  [CreateAssetMenu(menuName = "Buildings/Create Building", fileName = "Building")]
  public class Building : BaseScriptableObject {
    public int SizeX;
    public int SizeY;
    public bool IsAttackable;
  }
}