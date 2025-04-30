using Siege;
using UnityEngine;

namespace Scriptables.Siege {
  [CreateAssetMenu(fileName = "SiegesSettings", menuName = "Siege/Sieges Settings")]
  public class SiegesSettings : ScriptableObject {
    public SiegeTemplate FirstSiege;
    public SiegeTemplate RandomSiegeTemplate;
    public SiegeTemplate FinalSiege;
    public Vector2 SiegesCount;
  }
}