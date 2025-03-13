using Inventory;
using Scriptables.Items;
using UnityEngine;

namespace Scriptables.Craft {
  [CreateAssetMenu(menuName = "Crafting System/Workstation", fileName = "New Workstation")]
  public class WorkstationObject : BaseScriptableObject {
    public RecipeType RecipeType;
    public string ResourcePath;
    public InventoryType OutputInventoryType;
    public InventoryType FuelInventoryType;
    public RecipesDatabaseObject RecipeDB;
    public string Title;
    [TextArea(15, 20)] public string Description;
    public bool ShowSuccessCraftMessages;
    public int CraftSlotsCount = 5;

#if UNITY_EDITOR
    private void OnValidate() {
      ResourcePath = UnityEditor.AssetDatabase.GetAssetPath(this).Replace("Assets/", "").Replace(".asset", "");
    }
#endif
  }
}