using Audio;
using SaveSystem;
using Scriptables;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Quests {
  public class QuestManager : MonoBehaviour, ISaveLoad {
    [SerializeField] private DialoguePanel dialoguePanel;
    [SerializeField] private GameObject catPrefab;
    [SerializeField] private AudioData appearSound;
    [SerializeField] private AudioData loopedSound;
    
    private bool firstQuestCompleted;
    private bool secondQuestCompleted;
    private bool thirdQuestCompleted;
    private AudioController audioController;

    public int Priority => LoadPriority.QUESTS;

    private void Awake() {
      SaveLoadSystem.Instance.Register(this);
    }
    private void Start() {
      audioController = GameManager.Instance.AudioController;
    }
    public void StartQuest(int index) {
      if (!CanStartQuest(index)) return;
      GameManager.Instance.UserInput.EnableUIControls(false);
      GameManager.Instance.UserInput.controls.GamePlay.Interact.performed += StopQuest;
      var pos = GameManager.Instance.PlayerController.transform.position;
      pos.y += 2.55f;
      pos.x += 4f;
      catPrefab.transform.position = pos;
      audioController.PlayAudio(appearSound, pos);
      audioController.PlayAudio(loopedSound, pos);
      
      catPrefab.SetActive(true);
      TriggerDialogue(index);
      SaveID(index);
    }

    public bool CanStartQuest(int index) {
      return index switch {
        0 => !firstQuestCompleted,
        1 => !secondQuestCompleted,
        2 => !thirdQuestCompleted,
        _ => true
      };
    }

    private void TriggerDialogue(int index) {
      dialoguePanel.ShowDialogue(index);
    }

    private void StopQuest(InputAction.CallbackContext callbackContext) {
      GameManager.Instance.UserInput.EnableUIControls(true);
      GameManager.Instance.UserInput.controls.GamePlay.Interact.performed -= StopQuest;
      catPrefab.SetActive(false);
      dialoguePanel.HideDialogue();
      audioController.StopAudio(loopedSound);
    }

    private void OnDestroy() {
      if (!GameManager.HasInstance) {
        return;
      }

      GameManager.Instance.UserInput.controls.GamePlay.Interact.performed -= StopQuest;
    }

    private void SaveID(int index) {
      switch (index) {
        case 0: firstQuestCompleted = true; break;
        case 1: secondQuestCompleted = true; break;
        case 2: thirdQuestCompleted = true; break;
      }

      Save();
    }

    public void Save() {
      SaveLoadSystem.Instance.gameData.QuestData.FirstCompleted = firstQuestCompleted;
      SaveLoadSystem.Instance.gameData.QuestData.SecondCompleted = secondQuestCompleted;
      SaveLoadSystem.Instance.gameData.QuestData.ThirdCompleted = thirdQuestCompleted;
    }

    public void Load() {
      firstQuestCompleted = SaveLoadSystem.Instance.gameData.QuestData.FirstCompleted;
      secondQuestCompleted = SaveLoadSystem.Instance.gameData.QuestData.SecondCompleted;
      thirdQuestCompleted = SaveLoadSystem.Instance.gameData.QuestData.ThirdCompleted;
    }

    public void Clear() {
      firstQuestCompleted = false;
      secondQuestCompleted = false;
      thirdQuestCompleted = false;
    }
  }
}