using System.Collections.Generic;
using Siege;
using UnityEngine;

namespace Scriptables.Siege {
  [CreateAssetMenu(fileName = "SiegesSettings", menuName = "Siege/Sieges Settings")]
  public class SiegesSettings : ScriptableObject {
    public SiegeTemplate FirstSiege;
    public List<SiegeTemplate> RandomSiegeTemplates;
    public SiegeTemplate FinalSiege;
    public int SiegesCountMin;
    public int SiegesCountMax;
  }
}