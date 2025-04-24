#if UNITY_EDITOR
using Actors;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StatsBase), true)]
public class BaseStatsEditor : Editor {
  private int[] values = new[] { 20, 50, 100 };
  private StatsBase stats;
  private ActorBase actor;
  public override void OnInspectorGUI() {
    DrawDefaultInspector();
    
    if (!Application.isPlaying) {
      return;
    }
    
    stats = (StatsBase)target;
    actor = target.GetComponent<ActorBase>();
    
    foreach (var val in values) {
      if (GUILayout.Button($"Remove {val} hp")) {
        stats.AddHealth(-val);
      }
    }

    foreach (var val in values) {
      if (GUILayout.Button($"Add {val} hp")) {
        stats.AddHealth(val);
      }
    }

    if (actor == null) {
      return;
    }

    foreach (var val in values) {
      if (GUILayout.Button($"Actor take {val} damage")) {
        actor.Damage(val);
      }
    }
  }
}
#endif