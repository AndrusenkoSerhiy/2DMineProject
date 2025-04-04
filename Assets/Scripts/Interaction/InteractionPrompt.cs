using System;
using TMPro;
using UnityEngine;

namespace Interaction
{
  public class InteractionPrompt : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI promt;
    private bool isDisplayed;
    [Tooltip("if static we skip showPrompt on start")]
    [SerializeField] private bool isStatic;
    private void Start() {
      if (isStatic)
        return;
      
      ShowPrompt(false);
    }
    public void ShowPrompt(bool state, string str = "") {
      if (state.Equals(isDisplayed) && str.Equals(promt.text))
        return;

      promt.text = str;
      promt.gameObject.SetActive(state);
      isDisplayed = state;
    }

    public void UpdateSpriteAsset() {
      var index = (int)GameManager.Instance.UserInput.GetActiveGameDevice();
      promt.spriteAsset = GameManager.Instance.ListOfTmpSpriteAssets.SpriteAssets[index];
    }
  }
}
