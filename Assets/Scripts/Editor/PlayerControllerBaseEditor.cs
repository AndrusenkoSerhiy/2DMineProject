#if UNITY_EDITOR
using Player;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerControllerBase), true)]
public class PlayerControllerBaseEditor : Editor {
  private PlayerControllerBase playerController;

  public override void OnInspectorGUI() {
    DrawDefaultInspector();

    if (!Application.isPlaying) {
      return;
    }

    playerController = (PlayerControllerBase)target;

    // Display the stats in the inspector
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
    EditorGUILayout.LabelField($"Health: {playerController.EntityStats.Health}", EditorStyles.label);
    EditorGUILayout.LabelField($"Max Health: {playerController.EntityStats.MaxHealth}", EditorStyles.label);
    EditorGUILayout.LabelField($"Health Regen: {playerController.EntityStats.HealthRegen}", EditorStyles.label);
    EditorGUILayout.LabelField($"Stamina: {playerController.EntityStats.Stamina}", EditorStyles.label);
    EditorGUILayout.LabelField($"Max Stamina: {playerController.EntityStats.MaxStamina}", EditorStyles.label);
    EditorGUILayout.LabelField($"Stamina Drain: {playerController.EntityStats.StaminaDrain}", EditorStyles.label);
    EditorGUILayout.LabelField($"Stamina Recovery: {playerController.EntityStats.StaminaRecovery}",
      EditorStyles.label);
    EditorGUILayout.LabelField($"Attack Range: {playerController.EntityStats.AttackRange}", EditorStyles.label);
    EditorGUILayout.LabelField($"Block Damage: {playerController.EntityStats.BlockDamage}", EditorStyles.label);
    EditorGUILayout.LabelField($"Entity Damage: {playerController.EntityStats.EntityDamage}", EditorStyles.label);
    EditorGUILayout.LabelField($"Time Between Attacks: {playerController.EntityStats.TimeBtwAttacks}",
      EditorStyles.label);
    EditorGUILayout.LabelField($"Attack Stamina Usage: {playerController.EntityStats.AttackStaminaUsage}",
      EditorStyles.label);
    EditorGUILayout.LabelField($"Armor: {playerController.EntityStats.Armor}", EditorStyles.label);

    // Add a button to simulate taking damage
    if (GUILayout.Button("Take Damage")) {
      TakeDamage();
    }

    // Add a button to simulate healing
    if (GUILayout.Button("Heal")) {
      Heal();
    }
  }

  // Button click simulation method for taking damage
  private void TakeDamage() {
    playerController.EntityStats.AddHealth(-20f);
  }

  // Button click simulation method for healing
  private void Heal() {
    playerController.EntityStats.AddHealth(20f);
  }
}
#endif