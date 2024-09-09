using Unity.VisualScripting;
using World;

public interface IDamageable {
  public bool HasTakenDamage { get; set; }

  public void Damage(float damage);

  public float GetHealth();

  public float GetMaxHealth();

  public void AfterDamageReceived();

  //TODO refactor
  public void DestroyObject(CellObjectsPool pool);
}
