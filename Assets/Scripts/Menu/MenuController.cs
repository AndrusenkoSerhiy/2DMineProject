using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Analytics;
using SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

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
    [SerializeField] private Menu activeMenu = Menu.None;
    private bool locked;
    private bool cancelEnabled;
    public static event Action OnExitToMainMenu;
    [Header("Cutscene")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject skipPrompt;
    [SerializeField] private bool isNewGame;
    public Menu ActiveMenu => activeMenu;

    private void Awake() {
      gameManager = GameManager.Instance;
      saveLoadSystem = SaveLoadSystem.Instance;

      SetupProfiles();
    }

    private void Start() {
      gameManager.UserInput.controls.UI.Cancel.performed += HandleEsc;
      // gameManager.AudioController.PreloadSiegeThemeAsync();
    }

    private void SetupProfiles() {
      var profilesData = saveLoadSystem.profilesData.Profiles;
      for (var i = 0; i < profiles.Count; i++) {
        var profile = profiles[i];
        var profileData = profilesData[i];
        profile.Setup(profileData.ProfileId, profileData.Name);
      }
    }

    private void HandleEsc(InputAction.CallbackContext ctx) {
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
      gameManager.LockOnNewGame();
      gameManager.UserInput.ShowCursor(false);
      videoPlayer.gameObject.SetActive(true);
      videoPlayer.loopPointReached += OnVideoEnd;
      HideMainMenu();
      HideSwitchProfiles();
      await Task.Yield();
      await Task.Delay(2000);
      ShowSkip();
      gameManager.StartGameCameraController.SetCameraTarget();
    }

    private void ShowSkip() {
      gameManager.UserInput.EnableInteractAction(true);
      gameManager.UserInput.controls.GamePlay.Interact.performed += StopCutscene;
      skipPrompt.SetActive(true);
    }
    
    private void HideSkip() {
      skipPrompt.SetActive(false);
    }

    private void StopCutscene(InputAction.CallbackContext obj) {
      videoPlayer.Stop();
      HideSkip();
      OnVideoEnd(videoPlayer);
    }

    //after cutscene continue to start game
    private void OnVideoEnd(VideoPlayer vp) {
      gameManager.UserInput.controls.GamePlay.Interact.performed -= StopCutscene;
      gameManager.UserInput.EnableInteractAction(false);
      videoPlayer.gameObject.SetActive(false);
      //ShowLoading();
      gameManager.StartGameCameraController.ResetBeforeNewGame();
      gameManager.StartGameCameraController.Play();
      saveLoadSystem.NewGame();
      isNewGame = true;
      Hide();
      gameManager.UserInput.ShowCursor(true);
      //HideLoading();
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
      isNewGame = false;
      HideLoading();
    }

    private async Task ExitGame() {
      await AnalyticsManager.Instance.SendBasicStatsAsync();
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
      OnExitToMainMenu?.Invoke();
    }

    public void Hide() {
      //gameManager.UserInput.ShowCursor(false);
      ShowAndLock(false);

      HideMainMenu();
      HideInGameMenu();
      HideProfiles();
      HideSwitchProfiles();

      activeMenu = Menu.None;
    }

    public void Show() {
      //gameManager.UserInput.ShowCursor(true);
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

      //use only one time when we start new game we don't need to unlock player 
      //before he grounded
      if (isNewGame && !state) {
        isNewGame = false;
      }
      else LockPlayer(state);

      bg.SetActive(state);
      locked = state;

      if (state) {
        gameManager.PauseGame();

        /*var cancelAction = gameManager.UserInput.controls.UI.Cancel;
        cancelEnabled = cancelAction.enabled;

        if (!cancelEnabled) {
          cancelAction.Enable();
          cancelAction.performed += ctx => HandleEsc();
        }*/

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

      mmNewGameButton.onClick.AddListener(StartNewGameClickHandler);
      mmExitButton.onClick.AddListener(ExitGameClickHandler);

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

      igmContinueGameButton.onClick.AddListener(HideClickHandler);
      igmExitToMainMenuButton.onClick.AddListener(ExitToMainMenuClickHandler);
      igmExitButton.onClick.AddListener(ExitGameClickHandler);

      HideMainMenu();
      HideProfiles();
      HideSwitchProfiles();

      activeMenu = Menu.InGameMenu;
      //GameManager.Instance.UserInput.ShowCursor(true);
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

      switchButton.onClick.AddListener(ShowProfilesClickHandler);
      profileNameText.text = saveLoadSystem.profilesData.CurrentProfile.Name;
    }

    private void HideSwitchProfiles() {
      switchProfileGameObject.SetActive(false);

      switchButton.onClick.RemoveAllListeners();
    }

    private void LockPlayer(bool state) {
      gameManager.EnableUIElements(!state);
      //gameManager.EnableAllInput(!state);
      gameManager.EnableGamePlayInput(!state);

      gameManager.CurrPlayerController.SetLockPlayer(state);
      gameManager.CurrPlayerController.SetLockHighlight(state);
      gameManager.UserInput.EnableGamePlayControls(!state);
    }

    private void ShowHideProfileBackButton() {
      var show = saveLoadSystem.IsProfileSet();

      backButton.gameObject.SetActive(show);
      backButton.onClick.RemoveAllListeners();

      if (show) {
        backButton.onClick.AddListener(ShowMainMenuClickHandler);
      }
    }

    private void ShowHideContinueButton() {
      var show = saveLoadSystem.CanContinueGame();

      mmContinueButton.gameObject.SetActive(show);
      mmContinueButton.onClick.RemoveAllListeners();

      if (show) {
        mmContinueButton.onClick.AddListener(ContinueGameClickHandler);
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

    private void StartNewGameClickHandler() {
      gameManager.AudioController.PlayUIClick();
      StartNewGame();
    }

    private void ExitGameClickHandler() {
      gameManager.AudioController.PlayUIClick();
      ExitGame();
    }

    private void HideClickHandler() {
      gameManager.AudioController.PlayUIClick();
      Hide();
    }

    private void ExitToMainMenuClickHandler() {
      gameManager.AudioController.PlayUIClick();
      ExitToMainMenu();
    }

    private void ShowProfilesClickHandler() {
      gameManager.AudioController.PlayUIClick();
      ShowProfiles();
    }

    private void ShowMainMenuClickHandler() {
      gameManager.AudioController.PlayUIClick();
      ShowMainMenu();
    }

    private void ContinueGameClickHandler() {
      gameManager.AudioController.PlayUIClick();
      ContinueGame();
    }
  }
}