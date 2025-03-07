using UnityEngine;
using UnityEngine.UI;

namespace Menu {
  public class InGameMenu : MonoBehaviour {
    [SerializeField] private Button continueGameButton, exitToMainMenuButton, exitButton;

    private void Update() {
      if (gameObject.activeSelf) {
        HandleEsc();
      }
    }

    private void OnEnable() {
      continueGameButton.onClick.AddListener(Hide);
      exitToMainMenuButton.onClick.AddListener(() => GameManager.Instance.ExitToMainMenu());
      exitButton.onClick.AddListener(() => GameManager.Instance.ExitGame());
    }

    private void OnDisable() {
      continueGameButton.onClick.RemoveAllListeners();
      exitToMainMenuButton.onClick.RemoveAllListeners();
      exitButton.onClick.RemoveAllListeners();
    }

    public void Hide() {
      GameManager.Instance.CurrPlayerController.SetLockPlayer(false);
      gameObject.SetActive(false);
    }

    public void Show() {
      GameManager.Instance.CurrPlayerController.SetLockPlayer(true);
      gameObject.SetActive(true);
    }

    private void HandleEsc() {
      if (GameManager.Instance.UserInput.controls.UI.Cancel.triggered) {
        Hide();
      }
    }
  }
}