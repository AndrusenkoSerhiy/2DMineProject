using System.Collections;
using TMPro;
using UnityEngine;
using System;
using JetBrains.Annotations;
using UnityEngine.UI;

namespace Messages {
  public class MessageUI : MonoBehaviour {
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI additionalMessageText;
    public Image iconImage;

    private Coroutine fadeCoroutine;
    private float lifetime;
    private Action<MessageUI> onHide;

    private string template;
    private string msgName;
    private string addMsgText;
    [CanBeNull] private string entityId;
    private int? currentAmount;
    [CanBeNull] private Sprite icon;

    public MessageUI SetEntityId([CanBeNull] string id) {
      entityId = id;

      return this;
    }

    public MessageUI SetTemplate(string template) {
      this.template = template;

      return this;
    }

    public MessageUI SetName(string name) {
      msgName = name;

      return this;
    }

    public MessageUI SetAdditionalMessage(string text = null) {
      addMsgText = text;

      return this;
    }

    public MessageUI SetAmount(int? amount) {
      currentAmount = amount;

      return this;
    }

    public MessageUI SetIcon([CanBeNull] Sprite icon) {
      this.icon = icon;

      return this;
    }

    public MessageUI SetDuration(float duration) {
      lifetime = duration;

      return this;
    }

    public MessageUI SetHideCallback(Action<MessageUI> hideCallback) {
      onHide = hideCallback;

      return this;
    }

    public void Setup() {
      messageText.text = ApplyTemplate();
      ApplyAdditionalMessage();
      SetupIcon();

      if (fadeCoroutine != null) {
        StopCoroutine(fadeCoroutine);
      }

      fadeCoroutine = StartCoroutine(ShowAndFade());
    }

    private void ApplyAdditionalMessage() {
      if (!additionalMessageText) {
        return;
      }

      additionalMessageText.text = string.IsNullOrEmpty(addMsgText) ? string.Empty : addMsgText;
    }

    private void SetupIcon() {
      if (icon) {
        iconImage.sprite = icon;
        iconImage.gameObject.SetActive(true);
      }
      else if (!iconImage.sprite) {
        iconImage.gameObject.SetActive(false);
      }
    }

    public bool TryUpdateAmount(int addAmount) {
      if (string.IsNullOrEmpty(entityId) || currentAmount == null) {
        return false;
      }

      currentAmount += addAmount;
      messageText.text = messageText.text.Replace($"x {currentAmount - addAmount}", $"x {currentAmount}");
      RestartFade();
      return true;
    }

    private void RestartFade() {
      if (fadeCoroutine != null) {
        StopCoroutine(fadeCoroutine);
      }

      fadeCoroutine = StartCoroutine(ShowAndFade());
    }

    private IEnumerator ShowAndFade() {
      canvasGroup.alpha = 0f;
      yield return FadeTo(1f, 0.3f);
      yield return new WaitForSeconds(lifetime);
      yield return FadeTo(0f, 0.5f);
      onHide?.Invoke(this);
    }

    private IEnumerator FadeTo(float targetAlpha, float duration) {
      var startAlpha = canvasGroup.alpha;
      var time = 0f;

      while (time < duration) {
        canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
        time += Time.deltaTime;
        yield return null;
      }

      canvasGroup.alpha = targetAlpha;
    }

    private string ApplyTemplate() {
      if (!string.IsNullOrEmpty(msgName)) {
        template = template.Replace($"{MessageTemplateVariables.name}", msgName);
      }

      if (currentAmount != null) {
        template = template.Replace($"{MessageTemplateVariables.amount}", currentAmount.ToString());
      }

      return template;
    }

    public bool MatchesResource(string id) {
      return entityId == id;
    }
  }
}