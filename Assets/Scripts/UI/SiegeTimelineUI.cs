using System.Collections.Generic;
using Craft;
using Siege;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class SiegeTimelineUI : MonoBehaviour {
    [SerializeField] private RectTransform timelineContainer;
    [SerializeField] private RectTransform timePointer;
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private List<SiegeSegmentUI> preloadedSegments = new();
    [SerializeField] protected TooltipTrigger tooltipTrigger;

    [Header("Colors")] [SerializeField] private Color pauseColor = Color.gray;
    [SerializeField] private Color siegeColor = Color.red;
    [SerializeField] private Color activeSiegeColor = Color.yellow;

    private SiegeManager siegeManager;
    private float totalCycleTime;
    private int reusedSegmentIndex = 0;

    private readonly List<SegmentInfo> segments = new();

    private class SegmentInfo {
      public float startTime;
      public float duration;
      public RectTransform rect;
      public Image image;
      public bool isSiege;
    }

    private void Awake() {
      siegeManager = GameManager.Instance.SiegeManager;
    }

    public void SetupTimeline(List<ActiveSiegeTemplate> sieges) {
      foreach (var segment in preloadedSegments) {
        segment.gameObject.SetActive(false);
      }

      timePointer.gameObject.SetActive(true);

      segments.Clear();
      reusedSegmentIndex = 0;
      totalCycleTime = 0f;

      foreach (var siege in sieges) {
        var pauseSegment = GetOrCreateSegment();
        var pauseRect = pauseSegment.GetRectTransform();
        var pauseImage = pauseSegment.GetImage();
        pauseImage.color = pauseColor;

        segments.Add(new SegmentInfo {
          startTime = totalCycleTime,
          duration = siege.TimeBeforeSiege,
          rect = pauseRect,
          image = pauseImage,
          isSiege = false
        });
        totalCycleTime += siege.TimeBeforeSiege;

        var siegeSegment = GetOrCreateSegment();
        var siegeRect = siegeSegment.GetRectTransform();
        var siegeImage = siegeSegment.GetImage();
        siegeImage.color = siegeColor;

        segments.Add(new SegmentInfo {
          startTime = totalCycleTime,
          duration = siege.Duration,
          rect = siegeRect,
          image = siegeImage,
          isSiege = true
        });
        totalCycleTime += siege.Duration;
      }

      LayoutSegments();
    }

    public void Reset() {
      foreach (var segment in preloadedSegments) {
        segment.gameObject.SetActive(false);
      }

      timePointer.gameObject.SetActive(false);
      segments.Clear();
      reusedSegmentIndex = 0;
    }

    private SiegeSegmentUI GetOrCreateSegment() {
      SiegeSegmentUI segment;

      if (reusedSegmentIndex < preloadedSegments.Count) {
        segment = preloadedSegments[reusedSegmentIndex];
      }
      else {
        var segmentObj = Instantiate(segmentPrefab, timelineContainer);
        segment = segmentObj.GetComponent<SiegeSegmentUI>();
        preloadedSegments.Add(segment);
      }

      segment.gameObject.SetActive(true);
      segment.transform.SetParent(timelineContainer, false);
      reusedSegmentIndex++;
      return segment;
    }

    private void LayoutSegments() {
      var timelineWidth = timelineContainer.rect.width;

      foreach (var segment in segments) {
        var width = (segment.duration / totalCycleTime) * timelineWidth;

        var layoutElement = segment.rect.GetComponent<LayoutElement>();
        if (layoutElement != null) {
          layoutElement.preferredWidth = width;
        }
      }
    }

    private void Update() {
      if (!siegeManager || siegeManager.TotalCycleTime <= 0f) {
        return;
      }

      var elapsed = siegeManager.SiegeCycleElapsedTime;
      var total = siegeManager.TotalCycleTime;
      var normalized = Mathf.Clamp01(elapsed / total);

      var timelineWidth = timelineContainer.rect.width;
      timePointer.anchoredPosition = new Vector2(normalized * timelineWidth, timePointer.anchoredPosition.y);

      foreach (var segment in segments) {
        var isCurrent = elapsed >= segment.startTime &&
                        elapsed < segment.startTime + segment.duration;

        if (segment.isSiege) {
          segment.image.color = isCurrent ? activeSiegeColor : siegeColor;
        }
      }

      SetTooltipContent();
      tooltipTrigger.UpdateText();
    }

    private void SetTooltipContent() {
      tooltipTrigger.content = siegeManager.IsSiegeInProgress ? "Siege will end in:" : "Siege will start in:";
      tooltipTrigger.content += $" {Helper.SecondsToTimeString(siegeManager.TimeToNextSegment)}";
    }
  }
}