using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class DialoguePanel : MonoBehaviour {
    [Header("Components")] 
    [SerializeField] private Image bgImage;
    [SerializeField] private Image catImage;
    [SerializeField] private Image interactImage;
    [SerializeField] private TMP_Text textField;
    [SerializeField] private string firstText;
    [SerializeField] private string secondText;
    [SerializeField] private string thirdText;
    [SerializeField] private Color showColor;
    [SerializeField] private Color hideColor;
    
    [SerializeField] private bool isShowDialogue;

    public void ShowDialogue(int index) {
      textField.text = index switch {
        0 => firstText,
        1 => secondText,
        2 => thirdText,
        _ => textField.text
      };
      
      bgImage.DOColor(showColor,1f);
      catImage.DOColor(showColor,1f);
      interactImage.DOColor(showColor,1f);
      textField.DOColor(showColor,1f);
      isShowDialogue = true;
    }
    
    public void HideDialogue() {
      bgImage.DOColor(hideColor,1f);
      catImage.DOColor(hideColor,1f);
      interactImage.DOColor(hideColor,1f);
      textField.DOColor(hideColor,1f);
      isShowDialogue = false;
    }
  }
}