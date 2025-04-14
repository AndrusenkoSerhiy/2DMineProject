using UnityEngine;
using World;

namespace Scriptables {
  [CreateAssetMenu(menuName = "Buildings/Create BuildingDataLibrary", fileName = "BuildingDataLibrary")]
  public class BuildingDataLibrary : ScriptableObject  {
    [SerializeField] private BuildingData[] Buildings;
  }
}