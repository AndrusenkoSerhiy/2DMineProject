using Scriptables.Stats;
using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(fileName = "New Modifier Database", menuName = "Stats/ModifierDatabase")]
  public class ModifiersDatabaseObject : Database<ModifierDisplayObject> {
  }
}