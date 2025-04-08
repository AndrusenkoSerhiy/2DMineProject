using System.Collections;
using Stats;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class HpBar : MonoBehaviour {
    [SerializeField] private Slider hpSlider;
    [SerializeField] private float maxBarHeight = 600f;
    [SerializeField] private float barHeightUpdateDuration = 2f;

    private PlayerStats playerStats;
    private RectTransform sliderRectTransform;
    private float heightCoefficient;

    private void Awake() {
      sliderRectTransform = hpSlider.GetComponent<RectTransform>();

      MiningRobotTool.OnPlayerSitOnRobot += UpdateEntityStats;
      MiningRobotTool.OnPlayerExitFromRobot += UpdateEntityStats;
    }

    private void Start() {
      UpdateEntityStats();
    }

    private void AddStatsListeners() {
      playerStats.Mediator.OnModifierAdded += OnMaxHealthChanged;
      playerStats.Mediator.OnModifierRemoved += OnMaxHealthChanged;
    }

    private void RemoveStatsListeners() {
      playerStats.Mediator.OnModifierAdded -= OnMaxHealthChanged;
      playerStats.Mediator.OnModifierRemoved -= OnMaxHealthChanged;
    }

    private void Update() {
      if (playerStats == null) {
        return;
      }

      UpdateValue();
    }

    private void UpdateEntityStats() {
      playerStats = GameManager.Instance.CurrPlayerController.PlayerStats;
      RemoveStatsListeners();
      heightCoefficient = maxBarHeight / playerStats.StatsObject.healthMaxPossibleValue;
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
      StartCoroutine(UpdateBarHeight());
      UpdateMaxValue();
      UpdateValue();
    }

    private IEnumerator UpdateBarHeight() {
      var height = Mathf.Min(maxBarHeight, playerStats.MaxHealth * heightCoefficient);
      var startHeight = sliderRectTransform.sizeDelta.y;
      var elapsed = 0f;

      while (elapsed < barHeightUpdateDuration) {
        elapsed += Time.deltaTime;
        var newHeight = Mathf.Lerp(startHeight, height, elapsed / barHeightUpdateDuration);
        sliderRectTransform.sizeDelta = new Vector2(sliderRectTransform.sizeDelta.x, newHeight);
        yield return null;
      }

      sliderRectTransform.sizeDelta = new Vector2(sliderRectTransform.sizeDelta.x, height);
    }

    private bool NeedHandleModifier(StatModifier modifier) {
      return modifier.Type == StatType.MaxHealth;
    }

    private void UpdateValue() {
      hpSlider.value = playerStats.Health;
    }

    private void UpdateMaxValue() {
      hpSlider.maxValue = playerStats.MaxHealth;
    }
  }
}