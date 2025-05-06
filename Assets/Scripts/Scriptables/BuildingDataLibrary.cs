using Scriptables.Items;
using UnityEngine;

namespace Scriptables {
  [CreateAssetMenu(menuName = "Buildings/Create BuildingDataLibrary", fileName = "BuildingDataLibrary")]
  public class BuildingDataLibrary : Database<Building> {
    // [SerializeField] private BuildingData[] Buildings;
  }
}