using UnityEngine;
using UnityEngine.UI;

namespace Menu {
  public class MainMenu : MonoBehaviour {
    [SerializeField] private Button newGameButton, exitButton;
    
    private void OnEnable() {
      newGameButton.onClick.AddListener(() => GameManager.Instance.StartNewGame());
      exitButton.onClick.AddListener(() => GameManager.Instance.ExitGame());
    }

    private void OnDisable() {
      newGameButton.onClick.RemoveAllListeners();
      exitButton.onClick.RemoveAllListeners();
    }

    public void Hide() {
      GameManager.Instance.CurrPlayerController.SetLockPlayer(false);
      //GameManager.Instance.CurrPlayerController.SetLockHighlight(false);
      gameObject.SetActive(false);
    }

    public void Show() {
      GameManager.Instance.CurrPlayerController.SetLockPlayer(true);
      GameManager.Instance.CurrPlayerController.SetLockHighlight(true);
      gameObject.SetActive(true);
    }
  }
}