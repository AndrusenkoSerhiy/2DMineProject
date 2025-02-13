using System;
using TMPro;
using UnityEngine;

namespace Interaction
{
  public class InteractionPrompt : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI promt;
    private bool isDisplayed;

    private void Start() {
      ShowPrompt(false);
    }
    public void ShowPrompt(bool state, string str = "") {
      if (state.Equals(isDisplayed) && str.Equals(promt.text))
        return;
      
      promt.text = str;
      promt.gameObject.SetActive(state);
      isDisplayed = state;
    }
  }
}
