using UnityEngine;

namespace Pool {
  public class PoolObjectParticle : PoolObjectBase {
    [SerializeField] private ParticleSystem particleSystem;
    private bool animationFinished;

    private void Update() {
      if (particleSystem == null) {
        return;
      }
      
      if (!particleSystem.isPlaying) {
        ReturnToPool();
      }
    }
  }
}