using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class DialoguePanel : MonoBehaviour {
    [Header("Components")] [SerializeField]
    private Image bgImage;

    [SerializeField] private Image catImage;
    [SerializeField] private Image interactImage;
    [SerializeField] private TMP_Text textField;
    [SerializeField] private string firstText;
    [SerializeField] private string secondText;
    [SerializeField] private string thirdText;
    [SerializeField] private Color showColor;
    [SerializeField] private Color hideColor;

    [SerializeField] private bool isShowDialogue;

    public void ShowDialogue(int index) {
      textField.text = index switch {
        0 => firstText,
        1 => secondText,
        2 => thirdText,
        _ => textField.text
      };
      GameManager.Instance.UserInput.BlockAction("Movement", "Quest");
      GameManager.Instance.UserInput.BlockAction("Attack", "Quest");
      GameManager.Instance.CameraConfigManager.SetCameraLow();

      bgImage.gameObject.SetActive(true);
      catImage.gameObject.SetActive(true);
      interactImage.gameObject.SetActive(true);
      textField.gameObject.SetActive(true);

      bgImage.DOColor(showColor, 1f);
      catImage.DOColor(showColor, 1f);
      interactImage.DOColor(showColor, 1f);
      textField.DOColor(showColor, 1f);
      isShowDialogue = true;
    }

    public void HideDialogue() {
      GameManager.Instance.CameraConfigManager.SetCameraDefault();
      GameManager.Instance.UserInput.UnblockAction("Movement", "Quest");
      GameManager.Instance.UserInput.UnblockAction("Attack", "Quest");

      /*bgImage.DOColor(hideColor, 1f);
      catImage.DOColor(hideColor, 1f);
      interactImage.DOColor(hideColor, 1f);
      textField.DOColor(hideColor, 1f);*/

      var sequence = DOTween.Sequence();
      sequence.Join(bgImage.DOColor(hideColor, 1f));
      sequence.Join(catImage.DOColor(hideColor, 1f));
      sequence.Join(interactImage.DOColor(hideColor, 1f));
      sequence.Join(textField.DOColor(hideColor, 1f));

      sequence.OnComplete(() => {
        bgImage.gameObject.SetActive(false);
        catImage.gameObject.SetActive(false);
        interactImage.gameObject.SetActive(false);
        textField.gameObject.SetActive(false);
        isShowDialogue = false;
      });

      isShowDialogue = false;
    }
  }
}