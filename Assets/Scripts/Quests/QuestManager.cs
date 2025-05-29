using System;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Quests {
  public class QuestManager : MonoBehaviour {
    [SerializeField] private DialoguePanel dialoguePanel;
    [SerializeField] private GameObject catPrefab;

    public void StartQuest(int index) {
      GameManager.Instance.UserInput.controls.GamePlay.Interact.performed += StopQuest;
      var pos = GameManager.Instance.PlayerController.transform.position;
      pos.y += 2.55f;
      pos.x += -4f;
      catPrefab.transform.position = pos;
      catPrefab.SetActive(true);
      TriggerDialogue(index);
    }

    private void TriggerDialogue(int index) {
      dialoguePanel.ShowDialogue(index);
    }

    private void StopQuest(InputAction.CallbackContext callbackContext) {
      GameManager.Instance.UserInput.controls.GamePlay.Interact.performed -= StopQuest;
      catPrefab.SetActive(false);
      dialoguePanel.HideDialogue();
    }
    
    private void OnDestroy() {
      if (!GameManager.HasInstance) {
        return;
      }

      GameManager.Instance.UserInput.controls.GamePlay.Interact.performed -= StopQuest;
    }
  }
}