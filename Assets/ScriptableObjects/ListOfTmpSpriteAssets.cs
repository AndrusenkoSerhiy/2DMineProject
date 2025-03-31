using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ScriptableObjects {
  [CreateAssetMenu(fileName = "List of Sprite Assets", menuName = "List of Sprite Assets")]
  public class ListOfTmpSpriteAssets : ScriptableObject {
    public List<TMP_SpriteAsset> SpriteAssets;
  }
}