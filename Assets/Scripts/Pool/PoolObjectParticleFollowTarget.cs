using UnityEngine;

namespace Pool {
  public class PoolObjectParticleFollowTarget : PoolObjectBase {
    [SerializeField] private ParticleSystem particleSys;

    private void Update() {
      if (particleSys == null) {
        return;
      }

      if (target != null) {
        transform.position = target.transform.position;
      }

      if (!particleSys.isPlaying) {
        ReturnToPool();
      }
    }
  }
}