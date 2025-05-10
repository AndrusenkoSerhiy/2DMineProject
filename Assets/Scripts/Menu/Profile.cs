using System;
using System.Text.RegularExpressions;
using SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu {
  public class Profile : MonoBehaviour {
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject outline;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button selectButton;
    [SerializeField] private int maxCharacters = 20;
    [SerializeField] private int minCharacters = 3;

    public event Action<int> OnProfileSelected;
    public event Action OnProfileDeleted;

    private static readonly Regex allowedCharacters = new(@"[^a-zA-Z0-9 _\-]");
    private int profileId = 0;
    private string profileName = string.Empty;
    private bool active;
    private SaveLoadSystem saveLoadSystem;

    public int ProfileId => profileId;

    private void Awake() {
      saveLoadSystem = SaveLoadSystem.Instance;
    }

    private void OnEnable() {
      inputField.characterLimit = maxCharacters;
      inputField.text = profileName;
      inputField.onValueChanged.AddListener(OnInputChanged);
      inputField.onEndEdit.AddListener(OnInputEndEdit);
      saveButton.onClick.AddListener(SaveHandler);
      deleteButton.onClick.AddListener(DeleteHandler);
      selectButton.onClick.AddListener(SelectHandler);

      ShowHideDeleteButton();
      ShowHideSelectButton();
      ShowHideSaveButton(profileName);

      if (saveLoadSystem.profilesData.CurrentProfile.ProfileId == profileId) {
        Activate();
      }
    }

    private void OnDisable() {
      inputField.onValueChanged.RemoveAllListeners();
      inputField.onEndEdit.RemoveAllListeners();
      saveButton.onClick.RemoveAllListeners();
      deleteButton.onClick.RemoveAllListeners();
      selectButton.onClick.RemoveAllListeners();
    }

    public void Setup(int id, string profile = "") {
      profileId = id;
      profileName = profile;
    }

    public void Activate() {
      active = true;
      outline.SetActive(true);

      ShowHideSelectButton();
    }

    public void Deactivate() {
      active = false;
      outline.SetActive(false);

      ShowHideSelectButton();
    }

    private void ShowHideDeleteButton() {
      deleteButton.gameObject.SetActive(profileName != string.Empty);
    }

    private void ShowHideSaveButton(string newValue) {
      var isLongEnough = newValue.Length >= minCharacters;
      saveButton.gameObject.SetActive(newValue != profileName && isLongEnough);
    }

    private void ShowHideSelectButton() {
      if (!active && profileName != string.Empty) {
        selectButton.gameObject.SetActive(true);
      }
      else {
        selectButton.gameObject.SetActive(false);
      }
    }

    private void OnInputChanged(string value) {
      var cleaned = allowedCharacters.Replace(value, "");

      ShowHideSaveButton(cleaned);

      if (cleaned == value) {
        return;
      }

      var caretPosition = inputField.caretPosition - (value.Length - cleaned.Length);
      inputField.SetTextWithoutNotify(cleaned);
      inputField.caretPosition = Mathf.Clamp(caretPosition, 0, cleaned.Length);
    }

    private void OnInputEndEdit(string value) {
      if (value == profileName || value.Length > 0) {
        return;
      }

      inputField.SetTextWithoutNotify(profileName);
      ShowHideSaveButton(profileName);
    }

    private void DeleteHandler() {
      if (profileId == 0) {
        deleteButton.gameObject.SetActive(false);
        return;
      }

      saveLoadSystem.DeleteGame(profileId);

      profileName = string.Empty;
      inputField.SetTextWithoutNotify(profileName);

      ShowHideDeleteButton();
      Deactivate();
      OnProfileDeleted?.Invoke();
    }

    private void SaveHandler() {
      var newName = inputField.text;
      if (profileId <= 0 || string.IsNullOrEmpty(newName) || newName.Length < minCharacters) {
        return;
      }

      saveLoadSystem.UpdateProfileName(profileId, inputField.text);
      profileName = inputField.text;
      saveButton.gameObject.SetActive(false);

      ShowHideSelectButton();
      ShowHideDeleteButton();
    }

    private void SelectHandler() {
      if (profileId <= 0 || string.IsNullOrEmpty(profileName)) {
        return;
      }

      saveLoadSystem.SetCurrentProfile(profileId, profileName);
      OnProfileSelected?.Invoke(profileId);
    }
  }
}