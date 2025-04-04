using UnityEngine;

namespace Scriptables.Stats {
  [CreateAssetMenu(fileName = "Modifier", menuName = "Stats/Modifier")]
  public class ModifierDisplayObject : BaseScriptableObject {
    public ModifierType modifierType;
    public string modifierName;
    [TextArea(15, 20)] public string description;

    [Tooltip("Set if need to display the value in the UI")]
    public Sprite display;

    public Sprite border;
    public Color borderColor;
    public Color iconColor;
  }
}