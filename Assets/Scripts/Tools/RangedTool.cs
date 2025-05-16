using Inventory;
using Scriptables.Items;
using UI;
using UnityEngine;

namespace Tools {
  public class RangedTool : HandItem {
    [SerializeField] private BulletsPool bulletsPool;
    [SerializeField] private Transform firePoint;

    private PlayerEquipment playerEquipment;
    private DynamicCrosshair crosshair;
    private Tool tool;

    private void Awake() {
      playerEquipment = GameManager.Instance.PlayerEquipment;
      crosshair = GameManager.Instance.DynamicCrosshair;
      tool = (Tool)Item;
    }

    public override void StartUse() {
      if (!playerEquipment.EquippedItem.CanShoot()) {
        return;
      }

      var firePos = firePoint.position;
      var bullet = bulletsPool.GetBullet();
      bullet.transform.position = firePos;

      var shootDirection = (crosshair.GetCenter() - firePos).normalized;

      bullet.Launch(shootDirection, tool.AmmoSpeed, bulletsPool);
      playerEquipment.ConsumeAmmo();
    }
  }
}