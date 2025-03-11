using System;
using System.Collections.Generic;
using Scriptables.Items;
using UnityEngine;

namespace Scriptables.Craft {
  [CreateAssetMenu(menuName = "Crafting System/Recipe", fileName = "New Recipe")]
  public class Recipe : BaseScriptableObject {
    public string RecipeName;
    public RecipeType RecipeType;
    public List<CraftingMaterial> RequiredMaterials;
    public CraftingMaterial Fuel;
    public ItemObject Result;
    [Tooltip("In seconds")] public int CraftingTime;
    public Sprite detailImg;

    [Serializable]
    public class CraftingMaterial {
      public ItemObject Material;
      public int Amount;
    }
  }
}