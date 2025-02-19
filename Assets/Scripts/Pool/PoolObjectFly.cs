using Scriptables;
using UnityEngine;

namespace Pool {
  public class PoolObjectFly : PoolObjectBase {
    [SerializeField] private WavyMove wavyMove;
    [SerializeField] private ResourceData resourceData;

    public void SetSprite(Sprite sprite) {
      wavyMove.UpdateSprite(sprite);
    }

    public void SetPosition(Vector3 a, Transform b) {
      wavyMove.SetPositions(a, b);
    }
    private void Update() {
      if (wavyMove.IsMoveComplete) ReturnToPool();
    }
    
  }
}