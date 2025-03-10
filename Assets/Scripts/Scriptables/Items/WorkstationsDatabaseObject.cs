using Scriptables.Craft;
using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(fileName = "New Workstations Database", menuName = "Workstations/WorkstationsDatabase")]
  public class WorkstationsDatabaseObject : Database<Workstation> {
  }
}