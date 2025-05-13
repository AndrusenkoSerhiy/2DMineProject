using System.Collections.Generic;
using System.Threading.Tasks;
using SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu {
  public enum Menu {
    None = 0,
    MainMenu = 1,
    InGameMenu = 2,
    Profiles = 3,
  }

  public class MenuController : MonoBehaviour {
    [SerializeField] private GameObject mainMenuGameObject;
    [SerializeField] private GameObject inGameMenuGameObject;
    [SerializeField] private GameObject profilesGameObject;
    [SerializeField] private GameObject switchProfileGameObject;
    [SerializeField] private GameObject loadingGameObject;
    [SerializeField] private GameObject bg;

    [Header("Main menu buttons")] [SerializeField]
    private Button mmNewGameButton, mmContinueButton, mmExitButton;

    [Header("In game menu buttons")] [SerializeField]
    private Button igmContinueGameButton, igmExitToMainMenuButton, igmExitButton;

    [Header("Profiles")] [SerializeField] private Button backButton;
    [SerializeField] private List<Profile> profiles;

    [Header("Profiles")] [SerializeField] private TextMeshProUGUI profileNameText;
    [SerializeField] private Button switchButton;

    private GameManager gameManager;
    private SaveLoadSystem saveLoadSystem;
    private Menu activeMenu = Menu.None;
    private bool locked;
    private bool cancelEnabled;

    private void Awake() {
      gameManager = GameManager.Instance;
      saveLoadSystem = SaveLoadSystem.Instance;

      SetupProfiles();
    }

    private void SetupProfiles() {
      var profilesData = saveLoadSystem.profilesData.Profiles;
      for (var i = 0; i < profiles.Count; i++) {
        var profile = profiles[i];
        var profileData = profilesData[i];
        profile.Setup(profileData.ProfileId, profileData.Name);
      }
    }

    private void HandleEsc() {
      if (!saveLoadSystem.IsProfileSet()) {
        return;
      }

      switch (activeMenu) {
        case Menu.Profiles:
          ShowMainMenu();
          return;
      }
    }

    private async void StartNewGame() {
      ShowLoading();
      await Task.Yield();
      await Task.Delay(100);

      gameManager.StartGameCameraController.Play();
      saveLoadSystem.NewGame();

      HideLoading();
    }

    private async void ContinueGame() {
      ShowLoading();
      await Task.Yield();
      await Task.Delay(100);

      if (saveLoadSystem.IsNewGame()) {
        gameManager.StartGameCameraController.Play();
      }
      else {
        gameManager.StartGameCameraController.ResetPlayer();
        gameManager.StartGameCameraController.Play();
      }

      saveLoadSystem.LoadGame();

      HideLoading();
    }

    private void ExitGame() {
      if (Application.isEditor) {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
      }
      else {
        Application.Quit();
      }
    }

    private void ExitToMainMenu() {
      saveLoadSystem.Save();
      gameManager.StartGameCameraController.Init();
      ShowMainMenu();
    }

    public void Hide() {
      ShowAndLock(false);

      HideMainMenu();
      HideInGameMenu();
      HideProfiles();
      HideSwitchProfiles();

      activeMenu = Menu.None;
    }

    public void Show() {
      ShowAndLock(true);
      gameManager.StartGameCameraController.Init();

      if (saveLoadSystem.IsProfileSet()) {
        ShowMainMenu();
      }
      else {
        ShowProfiles();
      }
    }

    private void ShowAndLock(bool state) {
      gameManager.SetGameStage(state ? GameStage.MainMenu : GameStage.Game);

      if (locked == state) {
        return;
      }

      LockPlayer(state);
      bg.SetActive(state);
      locked = state;

      if (state) {
        gameManager.PauseGame();

        var cancelAction = gameManager.UserInput.controls.UI.Cancel;
        cancelEnabled = cancelAction.enabled;

        if (!cancelEnabled) {
          cancelAction.Enable();
          cancelAction.performed += ctx => HandleEsc();
        }

        gameManager.SetGameStage(GameStage.MainMenu);
      }
      else {
        gameManager.ResumeGame();

        gameManager.SetGameStage(GameStage.Game);
      }
    }

    private void ShowMainMenu() {
      if (!saveLoadSystem.IsProfileSet()) {
        return;
      }

      /*gameManager.CurrPlayerController.SetLockPlayer(true);
      gameManager.CurrPlayerController.SetLockHighlight(true);*/

      mainMenuGameObject.SetActive(true);

      mmNewGameButton.onClick.AddListener(StartNewGame);
      mmExitButton.onClick.AddListener(ExitGame);

      ShowHideContinueButton();

      HideInGameMenu();
      HideProfiles();
      ShowSwitchProfiles();

      activeMenu = Menu.MainMenu;
    }

    private void HideMainMenu() {
      // gameManager.CurrPlayerController.SetLockPlayer(false);

      mainMenuGameObject.SetActive(false);

      mmNewGameButton.onClick.RemoveAllListeners();
      mmContinueButton.onClick.RemoveAllListeners();
      mmExitButton.onClick.RemoveAllListeners();
    }

    public void ShowInGameMenu() {
      if (activeMenu == Menu.InGameMenu) {
        Hide();
        return;
      }

      if (gameManager.GameStage != GameStage.Game) {
        return;
      }

      ShowAndLock(true);
      // LockPlayer(true);

      inGameMenuGameObject.SetActive(true);

      igmContinueGameButton.onClick.AddListener(Hide);
      igmExitToMainMenuButton.onClick.AddListener(ExitToMainMenu);
      igmExitButton.onClick.AddListener(ExitGame);

      HideMainMenu();
      HideProfiles();
      HideSwitchProfiles();

      activeMenu = Menu.InGameMenu;
    }

    private void HideInGameMenu() {
      // LockPlayer(false);

      igmContinueGameButton.onClick.RemoveAllListeners();
      igmExitToMainMenuButton.onClick.RemoveAllListeners();
      igmExitButton.onClick.RemoveAllListeners();

      inGameMenuGameObject.SetActive(false);
    }

    private void ShowProfiles() {
      profilesGameObject.SetActive(true);

      foreach (var profile in profiles) {
        profile.OnProfileSelected += OnProfileSelectedHandler;
        profile.OnProfileDeleted += OnProfileDeletedHandler;
      }

      ShowHideProfileBackButton();

      HideMainMenu();
      HideInGameMenu();
      HideSwitchProfiles();

      activeMenu = Menu.Profiles;
    }

    private void HideProfiles() {
      profilesGameObject.SetActive(false);

      foreach (var profile in profiles) {
        profile.OnProfileSelected -= OnProfileSelectedHandler;
        profile.OnProfileDeleted -= OnProfileDeletedHandler;
      }

      backButton.onClick.RemoveAllListeners();
    }

    private void OnProfileDeletedHandler() {
      ShowHideProfileBackButton();
    }

    private void OnProfileSelectedHandler(int profileId) {
      foreach (var profile in profiles) {
        if (profile.ProfileId == profileId) {
          profile.Activate();
        }
        else {
          profile.Deactivate();
        }
      }

      ShowHideProfileBackButton();
    }

    private void ShowSwitchProfiles() {
      switchProfileGameObject.SetActive(true);

      switchButton.onClick.AddListener(ShowProfiles);
      profileNameText.text = saveLoadSystem.profilesData.CurrentProfile.Name;
    }

    private void HideSwitchProfiles() {
      switchProfileGameObject.SetActive(false);

      switchButton.onClick.RemoveAllListeners();
    }

    private void LockPlayer(bool state) {
      gameManager.EnableUIElements(!state);
      gameManager.EnableInput(!state);

      gameManager.CurrPlayerController.SetLockPlayer(state);
      gameManager.CurrPlayerController.SetLockHighlight(state);
      gameManager.UserInput.EnableGamePlayControls(!state);
    }

    private void ShowHideProfileBackButton() {
      var show = saveLoadSystem.IsProfileSet();

      backButton.gameObject.SetActive(show);
      backButton.onClick.RemoveAllListeners();

      if (show) {
        backButton.onClick.AddListener(ShowMainMenu);
      }
    }

    private void ShowHideContinueButton() {
      var show = saveLoadSystem.CanContinueGame();

      mmContinueButton.gameObject.SetActive(show);
      mmContinueButton.onClick.RemoveAllListeners();

      if (show) {
        mmContinueButton.onClick.AddListener(ContinueGame);
      }
    }

    private void ShowLoading() {
      loadingGameObject.SetActive(true);
      HideMainMenu();
      HideSwitchProfiles();
    }

    private void HideLoading() {
      loadingGameObject.SetActive(false);
      Hide();
    }
  }
}