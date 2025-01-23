using Scriptables.Craft;
using UnityEngine;

namespace Scriptables.Craft {
  [CreateAssetMenu(menuName = "Crafting System/Workstation", fileName = "New Workstation")]
  public class Workstation : ScriptableObject {
    public RecipeType RecipeType;
    public string Title;
    [TextArea(15, 20)] public string Description;
    public int OutputSlotsAmount;
  }
}