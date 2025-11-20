using Audio;
using SaveSystem;
using Scriptables;
using UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Quests {
  public class QuestManager : MonoBehaviour, ISaveLoad {
    [SerializeField] private DialoguePanel dialoguePanel;
    [SerializeField] private GameObject catPrefab;
    [SerializeField] private AnimationCurve spawnCurve;
    [SerializeField] private AnimationCurve hideCurve;
    [SerializeField] private AudioData appearSound;
    [SerializeField] private AudioData loopedSound;
    
    private bool firstQuestCompleted;
    private bool secondQuestCompleted;
    private bool thirdQuestCompleted;
    private AudioController audioController;
    //while cat is active block interaction
    [SerializeField] private bool blockInteraction;

    public int Priority => LoadPriority.QUESTS;
    public bool BlockInteraction => blockInteraction;

    private void Awake() {
      SaveLoadSystem.Instance.Register(this);
    }
    private void Start() {
      audioController = GameManager.Instance.AudioController;
    }
    public void StartQuest(int index) {
      if (!CanStartQuest(index)) return;
      blockInteraction = true;
      GameManager.Instance.UserInput.EnableUIControls(false);
      GameManager.Instance.UserInput.controls.GamePlay.Interact.performed += StopQuest;
      var pos = GameManager.Instance.PlayerController.transform.position;
      pos.y += 2.55f;
      pos.x += 4f;
      catPrefab.transform.position = pos;
      audioController.PlayAudio(appearSound, pos);
      audioController.PlayAudio(loopedSound, pos);
      
      catPrefab.SetActive(true);
      //Tween for spawn effect
      var spriteRenderer = catPrefab.GetComponentInChildren<SpriteRenderer>();
      var material = spriteRenderer.material;
      material.SetFloat("_Scale", 0f);
      material.SetFloat("_Twirl", -1f);
      DOTween.To(() => material.GetFloat("_Scale"), x => material.SetFloat("_Scale", x), 1f, 1.2f);
      DOTween.To(() => material.GetFloat("_Twirl"), x => material.SetFloat("_Twirl", x), 0f, 1.2f).SetEase(spawnCurve);
      GameManager.Instance.PoolEffects.SpawnFromPool("CatSpawnParticleEffect", catPrefab.transform.position, Quaternion.identity);

      TriggerDialogue(index);
      SaveID(index);
    }

    public bool CanStartQuest(int index) {
      return index switch {
        0 => !firstQuestCompleted,
        1 => !secondQuestCompleted,
        2 => !thirdQuestCompleted,
        _ => false
      };
    }

    private void TriggerDialogue(int index) {
      dialoguePanel.ShowDialogue(index);
    }

    private void StopQuest(InputAction.CallbackContext callbackContext) {
      GameManager.Instance.UserInput.EnableUIControls(true);
      GameManager.Instance.UserInput.controls.GamePlay.Interact.performed -= StopQuest;
      //Tween for hide effect
      var spriteRenderer = catPrefab.GetComponentInChildren<SpriteRenderer>();
      var material = spriteRenderer.material;
      material.SetFloat("_Scale", 1f);
      material.SetFloat("_Twirl", 0f);
      DOTween.To(() => material.GetFloat("_Scale"), x => material.SetFloat("_Scale", x), 0f, 0.6f).SetEase(hideCurve);
      DOTween.To(() => material.GetFloat("_Twirl"), x => material.SetFloat("_Twirl", x), 1f, 0.6f).SetEase(hideCurve).OnComplete(() => catPrefab.SetActive(false));
      GameManager.Instance.PoolEffects.SpawnFromPool("CatSpawnParticleEffect", catPrefab.transform.position, Quaternion.identity);

      dialoguePanel.HideDialogue();
      audioController.StopAudio(loopedSound);
      
      blockInteraction = false;
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