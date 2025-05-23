using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class RobotPlaceCellInfo : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image image;

    public void Show() {
      gameObject.SetActive(true);
    }

    public void Hide() {
      gameObject.SetActive(false);
    }
    public void SetAmmo(int ammo) {
      ammoText.text = ammo.ToString();
    }

    public void SetImage(Sprite sprite) {
      image.sprite = sprite;
    }
  }
}