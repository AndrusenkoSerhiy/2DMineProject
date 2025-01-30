using System.Collections.Generic;
using Craft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scriptables.Craft {
  [CreateAssetMenu(menuName = "Crafting System/Workstation", fileName = "New Workstation")]
  public class Workstation : ScriptableObject {
    public RecipeType RecipeType;
    public string Title;
    [TextArea(15, 20)] public string Description;
    public int OutputSlotsAmount;
    public List<Recipe> recipes = new List<Recipe>();
    // public GameObject recipesListContainerPrefab;
    // public RecipeDetail detail;
    // public Button recipesListItemPrefab;
    // public TMP_InputField countInput;
    // public Button craftButton;
    // public Button incrementButton;
    // public Button decrementButton;
  }
}