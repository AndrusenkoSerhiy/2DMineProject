using System.Collections.Generic;
using Inventory;
using Scriptables.Items;
using UnityEngine;

namespace Scriptables.Craft {
  [CreateAssetMenu(menuName = "Crafting System/Workstation", fileName = "New Workstation")]
  public class WorkstationObject : BaseScriptableObject {
    public RecipeType RecipeType;
    public ItemObject InventoryItem;
    public InventoryType OutputInventoryType;
    public InventoryType FuelInventoryType;
    public RecipesDatabaseObject RecipeDB;
    public string Title;
    [TextArea(15, 20)] public string Description;
    public bool ShowSuccessCraftMessages;
    public int CraftSlotsCount = 5;

    public List<AudioData> MusicAudioDatas;
    public Vector2 SecondsBetweenMusic = new Vector2(15, 30);
  }
}