using UnityEngine;

namespace World {
  public class ParallaxBackground : MonoBehaviour {

    [SerializeField] private Vector2 parallaxEffectMultiplier;
    [SerializeField] private bool infiniteHorizontal;
    [SerializeField] private bool infiniteVertical;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    private float textureUnitSizeX;
    private float textureUnitSizeY;


    private void Start() {
      cameraTransform = Camera.main.transform;
      lastCameraPosition = cameraTransform.position;
      Sprite sprite = GetComponent<SpriteRenderer>().sprite;
      Texture2D texture = sprite.texture;
      textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
      textureUnitSizeY = texture.height / sprite.pixelsPerUnit;
    }

    private void LateUpdate() {
      Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
      transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier.x, deltaMovement.y * parallaxEffectMultiplier.y);
      lastCameraPosition = cameraTransform.position;

      Vector3 newPosition = transform.position;

      if (infiniteHorizontal) {
        if (Mathf.Abs(cameraTransform.position.x - transform.position.x) >= textureUnitSizeX) {
          float offsetPositionX = (cameraTransform.position.x - transform.position.x) % textureUnitSizeX;
          newPosition.x = cameraTransform.position.x + offsetPositionX;
        }
      }

      if (infiniteVertical) {
        if (Mathf.Abs(cameraTransform.position.y - transform.position.y) >= textureUnitSizeY) {
          float offsetPositionY = (cameraTransform.position.y - transform.position.y) % textureUnitSizeY;
          newPosition.y = cameraTransform.position.y + offsetPositionY;
        }
      }

      transform.position = newPosition;
    }
  }
}
