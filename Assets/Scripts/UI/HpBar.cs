using System.Collections;
using Stats;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class HpBar : MonoBehaviour {
    [SerializeField] private Slider hpSlider;
    [SerializeField] private float initialBarWidth = 300f;
    [SerializeField] private float maxBarWidth = 600f;
    [SerializeField] private float barWidthUpdateDuration = 2f;

    private EntityStats entityStats;
    private RectTransform sliderRectTransform;
    private float widthCoefficient;

    private void Awake() {
      sliderRectTransform = hpSlider.GetComponent<RectTransform>();

      // MiningRobotTool.OnPlayerSitOnRobot += UpdateEntityStats;
      // MiningRobotTool.OnPlayerExitFromRobot += UpdateEntityStats;
    }

    private void Start() {
      UpdateEntityStats();
    }

    private void AddStatsListeners() {
      entityStats.Mediator.OnModifierAdded += OnMaxHealthChanged;
      entityStats.Mediator.OnModifierRemoved += OnMaxHealthChanged;
    }

    private void RemoveStatsListeners() {
      entityStats.Mediator.OnModifierAdded -= OnMaxHealthChanged;
      entityStats.Mediator.OnModifierRemoved -= OnMaxHealthChanged;
    }

    private void Update() {
      if (entityStats == null) {
        return;
      }

      UpdateValue();
    }

    private void UpdateEntityStats() {
      entityStats = GameManager.Instance.CurrPlayerController.EntityStats;
      RemoveStatsListeners();
      widthCoefficient = initialBarWidth / entityStats.BaseStatsObject.health;
      AddStatsListeners();
      UpdateUI();
    }

    private void OnMaxHealthChanged(StatModifier modifier) {
      if (!NeedHandleModifier(modifier)) {
        return;
      }

      UpdateUI();
    }

    private void UpdateUI() {
      StartCoroutine(UpdateBarWidth());
      UpdateMaxValue();
      UpdateValue();
    }

    private IEnumerator UpdateBarWidth() {
      var width = Mathf.Min(maxBarWidth, entityStats.MaxHealth * widthCoefficient);
      var startWidth = sliderRectTransform.sizeDelta.x;
      var elapsed = 0f;

      while (elapsed < barWidthUpdateDuration) {
        elapsed += Time.deltaTime;
        var newWidth = Mathf.Lerp(startWidth, width, elapsed / barWidthUpdateDuration);
        sliderRectTransform.sizeDelta = new Vector2(newWidth, sliderRectTransform.sizeDelta.y);
        yield return null;
      }

      sliderRectTransform.sizeDelta = new Vector2(width, sliderRectTransform.sizeDelta.y);
    }

    private bool NeedHandleModifier(StatModifier modifier) {
      return modifier.Type == StatType.MaxHealth;
    }

    private void UpdateValue() {
      hpSlider.value = entityStats.Health;
    }

    private void UpdateMaxValue() {
      hpSlider.maxValue = entityStats.MaxHealth;
    }
  }
}