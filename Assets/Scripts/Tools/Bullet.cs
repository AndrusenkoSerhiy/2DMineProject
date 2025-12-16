using Craft;
using Scriptables.Items;
using UnityEngine;
using Utils;
using World;

namespace Tools {
  public class Bullet : MonoBehaviour {
    private Vector3 bulletDirection;
    private float bulletSpeed;
    private bool active;
    private BulletsPool bulletsPool;
    private GameManager gameManager;
    private int cols;
    private int rows;
    private int visionOffsetX;
    private int visionOffsetY;
    private PlayerStats playerStats;
    private string id;
    private GameObject trail;
    private Coords pos;

    public string Id => id;

    public void Awake() {
      gameManager = GameManager.Instance;
      playerStats = gameManager.PlayerController.PlayerStats;
      cols = gameManager.GameConfig.ChunkSizeX;
      rows = gameManager.GameConfig.ChunkSizeY;
      visionOffsetX = gameManager.GameConfig.PlayerAreaWidth / 2;
      visionOffsetY = gameManager.GameConfig.PlayerAreaHeight / 2;
    }

    public void Launch(Vector3 direction, Tool tool, BulletsPool pool) {
      bulletDirection = direction;

      id = tool.ammo.Id;
      bulletSpeed = tool.AmmoSpeed;
      bulletsPool = pool;
      active = true;

      transform.right = bulletDirection;
      transform.SetParent(null);
      gameObject.SetActive(true);
      transform.localScale = Vector3.one;

      trail = GameManager.Instance.PoolEffects.SpawnFromPool("BulletTrailParticleEffect", transform.position, Quaternion.identity)
        .gameObject;
    }

    private void Update() {
      if (!active) {
        return;
      }

      transform.position += bulletDirection * bulletSpeed * Time.deltaTime;
      if(trail!=null && trail.activeSelf) trail.transform.position = transform.position;

      CheckArea();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
      if (!active) {
        return;
      }

      if (!collision.TryGetComponent(out IDamageable target)) {
        return;
      }

      if (target.DamageableType is DamageableType.Player or DamageableType.Robot) {
        return;
      }
      
      if (target.DamageableType is DamageableType.Door && !target.CanGetDamage) {
        return;
      }

      if (target.GetHealth() <= 0) {
        return;
      }

      var damage = target.DamageableType == DamageableType.Cell ? playerStats.BlockDamage : playerStats.EntityDamage;

      target.Damage(damage, true);
      if (target.GetHealth() <= 0) {
        target.DestroyObject();
      }

      Deactivate();
    }

    private void CheckArea() {
      var playerCoords = gameManager.PlayerController.PlayerCoords.GetCoords();

      var min_x = Mathf.Clamp(playerCoords.X - visionOffsetX, 0, cols - 1);
      var max_x = Mathf.Clamp(playerCoords.X + visionOffsetX, 0, cols - 1);
      var min_y = Mathf.Clamp(playerCoords.Y - visionOffsetY, 0, rows - 1);
      var max_y = Mathf.Clamp(playerCoords.Y + visionOffsetY, 0, rows - 1);

      CoordsTransformer.WorldToGrid(transform.position, ref pos);

      if (pos.X < min_x || pos.X > max_x || pos.Y < min_y || pos.Y > max_y) {
        Deactivate();
      }
    }

    private void Deactivate() {
      trail = null;
      GameManager.Instance.PoolEffects.SpawnFromPool("NailBulletDestroyParticleEffect", transform.position, Quaternion.identity);
      active = false;
      bulletsPool.ReturnBullet(this);
    }
  }
}