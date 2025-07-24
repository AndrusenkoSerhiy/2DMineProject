using Scriptables;
using UnityEngine;

public enum DamageableType {
  Default = 0,
  Player = 10,
  Robot = 20,
  Enemy = 30,
  Cell = 40,
  Door = 50,
};

public interface IDamageable {
  public DamageableType DamageableType { get; set; }
  public AudioData OnTakeDamageAudioData { get; set; }
  //public bool hasTakenDamage { get; set; }
  //use for nails and door
  public bool CanGetDamage { get; set; }
  public void Damage(float damage, bool isPlayer);

  public float GetHealth();

  public Vector3 GetPosition();
  public string GetName();
  public float GetMaxHealth();

  public void AfterDamageReceived();

  public void DestroyObject();
}