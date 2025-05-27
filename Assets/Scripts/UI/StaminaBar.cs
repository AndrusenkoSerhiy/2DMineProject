using System.Collections;
using Stats;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class StaminaBar : MonoBehaviour {
    [SerializeField] private Slider slider;
    [SerializeField] private float maxBarHeight = 600f;
    [SerializeField] private float barHeightUpdateDuration = 2f;

    private PlayerStats playerStats;
    private RectTransform sliderRectTransform;
    private float heightCoefficient;
    private bool started;

    private void Awake() {
      sliderRectTransform = slider.GetComponent<RectTransform>();

      MiningRobotTool.OnPlayerSitOnRobot += UpdateEntityStats;
      MiningRobotTool.OnPlayerExitFromRobot += UpdateEntityStats;
    }

    private void Start() {
      UpdateEntityStats();
      started = true;
    }

    private void OnEnable() {
      if (!started) {
        return;
      }

      UpdateEntityStats();
    }

    private void AddStatsListeners() {
      playerStats.Mediator.OnModifierAdded += OnMaxValueChanged;
      playerStats.Mediator.OnModifierRemoved += OnMaxValueChanged;
    }

    private void RemoveStatsListeners() {
      if (playerStats == null) {
        return;
      }

      playerStats.Mediator.OnModifierAdded -= OnMaxValueChanged;
      playerStats.Mediator.OnModifierRemoved -= OnMaxValueChanged;
    }

    private void Update() {
      if (playerStats == null) {
        return;
      }

      UpdateValue();
    }

    private void UpdateEntityStats() {
      RemoveStatsListeners();
      playerStats = GameManager.Instance.CurrPlayerController.PlayerStats;
      heightCoefficient = maxBarHeight / playerStats.StatsObject.staminaMaxPossibleValue;
      AddStatsListeners();
      UpdateUI();
    }

    private void OnMaxValueChanged(StatModifier modifier) {
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
      var height = Mathf.Min(maxBarHeight, playerStats.MaxStamina * heightCoefficient);
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
      return modifier.Type == StatType.MaxStamina;
    }

    private void UpdateValue() {
      slider.value = playerStats.Stamina;
    }

    private void UpdateMaxValue() {
      slider.maxValue = playerStats.MaxStamina;
    }
  }
}