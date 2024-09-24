using UnityEngine;

namespace Pool {
  public class PoolObjectParticle : PoolObjectBase {
    [SerializeField] private ParticleSystem particleSys;

    private void Update() {
      if (particleSys == null) {
        return;
      }

      if (!particleSys.isPlaying) {
        ReturnToPool();
      }
    }
  }
}