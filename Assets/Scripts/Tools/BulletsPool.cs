using System;
using System.Collections.Generic;
using Scriptables.Items;
using UnityEngine;

namespace Tools {
  [Serializable]
  class BulletPoolItem {
    public ItemObject bulletItemObject;
    public GameObject bulletPrefab;
  }

  public class BulletsPool : MonoBehaviour {
    [SerializeField] private List<BulletPoolItem> bulletPoolItem;

    private Dictionary<string, List<Bullet>> pools = new();

    public Bullet GetBullet(string id) {
      var pool = pools.TryGetValue(id, out var poolList) ? poolList : pools[id] = new List<Bullet>();

      if (pool.Count > 0) {
        var last = pool[^1];
        pool.RemoveAt(pool.Count - 1);
        return last;
      }

      var bulletPrefab = bulletPoolItem.Find(b => b.bulletItemObject.Id == id).bulletPrefab;

      var bullet = Instantiate(bulletPrefab, transform).GetComponent<Bullet>();
      bullet.gameObject.SetActive(false);
      return bullet;
    }

    public void ReturnBullet(Bullet bullet) {
      bullet.gameObject.SetActive(false);
      bullet.gameObject.transform.parent = transform;

      if (!pools.ContainsKey(bullet.Id)) {
        pools[bullet.Id] = new List<Bullet>();
      }

      pools[bullet.Id].Add(bullet);
    }
  }
}