using System.Collections.Generic;
using UnityEngine;

namespace Tools {
  public class BulletsPool : MonoBehaviour {
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private List<Bullet> pool = new();

    public Bullet GetBullet() {
      if (pool.Count > 0) {
        var last = pool[^1];
        pool.RemoveAt(pool.Count - 1);
        return last;
      }

      var bullet = Instantiate(bulletPrefab, transform).GetComponent<Bullet>();
      bullet.gameObject.SetActive(false);
      return bullet;
    }

    public void ReturnBullet(Bullet bullet) {
      bullet.gameObject.SetActive(false);
      bullet.gameObject.transform.parent = transform;
      pool.Add(bullet);
    }
  }
}