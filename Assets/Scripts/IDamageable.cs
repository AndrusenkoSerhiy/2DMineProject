using Unity.VisualScripting;

public interface IDamageable {
  public bool HasTakenDamage { get; set; }

  public void Damage(float damage);

  public float GetHealth();

  public float GetMaxHealth();

  public void AfterDamageReceived();
}
