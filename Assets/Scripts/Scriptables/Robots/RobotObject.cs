using System.Collections.Generic;
using Scriptables.Items;
using UnityEngine;

namespace Scriptables.Repair {
  [CreateAssetMenu(menuName = "Robots", fileName = "New robot")]
  public class RobotObject : BaseScriptableObject, IRepairable {
    [Tooltip("Count of repair kits")] [SerializeField]
    private int repairCost;

    public int RepairCost => repairCost;

    public AudioData jumpAudioData;
    public AudioData jumpLandingAudioData;
    public AudioData leftStepAudioData;
    public AudioData rightStepAudioData;
    public List<AudioData> damagedAudioData;
  }
}