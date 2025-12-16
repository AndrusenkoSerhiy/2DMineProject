using System;
using System.Collections.Generic;
using SaveSystem;
using UnityEngine;

namespace UI {
  public class LocatorTarget {
    public Vector3 target;
    public Point point;
  }

  public class Point {
    public LocatorPoint locatorPoint;
    public LocatorDistance locatorDistance;
  }

  [Serializable]
  public class AllowPoint {
    public string id;
    public Sprite sprite;
    public Color color;
  }

  public class Locator : MonoBehaviour, ISaveLoad {
    [Header("References")] [SerializeField]
    private Transform locatorPoints;

    [SerializeField] private Transform locatorDistances;
    [SerializeField] private GameObject locatorPointPrefab;
    [SerializeField] private GameObject locatorDistancePrefab;
    [SerializeField] private GameObject pointerUI;
    [SerializeField] private Camera cam;

    [Header("Data")] [SerializeField] private List<LocatorPoint> points = new();
    [SerializeField] private List<LocatorDistance> distances = new();
    [SerializeField] private List<AllowPoint> allowed = new();
    [SerializeField] private float sortInterval = 1f;

    private Queue<Point> pointsPool = new();
    private Dictionary<string, LocatorTarget> locatorTargets = new();
    private Dictionary<string, AllowPoint> allowedPoints = new();

    private GameManager gameManager;
    private SaveLoadSystem saveLoadSystem;

    private bool hidden;
    private bool paused;
    private float lastSortTime;

    private Transform playerTransform;
    private float cellSizeX;

    private readonly List<VisiblePoint> visiblePoints = new(16);

    private struct VisiblePoint {
      public float distance;
      public LocatorPoint point;
    }

    #region Save / Load

    public int Priority => LoadPriority.LOCATOR;

    public void Save() {
      saveLoadSystem.gameData.LocatorPointsData.Clear();

      foreach (var kvp in locatorTargets) {
        saveLoadSystem.gameData.LocatorPointsData.Add(
          new LocatorPointData {
            Id = kvp.Key,
            Position = kvp.Value.target,
            Color = allowedPoints[kvp.Key].color
          }
        );
      }
    }

    public void Load() {
      if (saveLoadSystem.IsNewGame() ||
          saveLoadSystem.gameData.LocatorPointsData.Count == 0) {
        return;
      }

      foreach (var target in saveLoadSystem.gameData.LocatorPointsData) {
        SetTarget(target.Position, target.Id);
      }
    }

    public void Clear() {
      foreach (var kvp in locatorTargets) {
        ResetPoint(kvp.Value.point);
        pointsPool.Enqueue(kvp.Value.point);
      }

      locatorTargets.Clear();
    }

    #endregion

    private void Awake() {
      if (points.Count == 0 || points.Count != distances.Count) {
        Debug.LogError("Locator: Points and distances count mismatch or empty list.");
        return;
      }

      gameManager = GameManager.Instance;
      saveLoadSystem = SaveLoadSystem.Instance;
      saveLoadSystem.Register(this);

      playerTransform = gameManager.PlayerController.transform; // NEW
      cellSizeX = gameManager.GameConfig.CellSizeX; // NEW

      for (var i = 0; i < points.Count; i++) {
        var point = points[i];
        var distance = distances[i];

        point.gameObject.SetActive(false);
        distance.gameObject.SetActive(false);

        pointsPool.Enqueue(new Point {
          locatorPoint = point,
          locatorDistance = distance
        });
      }

      foreach (var allow in allowed) {
        allowedPoints.Add(allow.id, allow);
      }

      gameManager.OnGamePaused += OnGamePausedHandler;
      gameManager.OnGameResumed += OnGameResumedHandler;
    }

    private void Update() => CheckArea();

    public void ShowHide(bool state) {
      hidden = !state;

      if (hidden) {
        pointerUI.SetActive(false);
      }
    }

    public void SetTarget(Vector3 position, string id) {
      // CHANGED: TryGetValue instead of ContainsKey
      if (!allowedPoints.TryGetValue(id, out var allow)) {
        return;
      }

      if (locatorTargets.ContainsKey(id)) {
        return;
      }

      var point = pointsPool.Count > 0 ? pointsPool.Dequeue() : CreateNewPoint();

      point.locatorPoint.SetPoint(allow.sprite, allow.color);
      point.locatorDistance.SetPoint(allow.sprite);

      locatorTargets[id] = new LocatorTarget {
        target = position,
        point = point
      };
    }

    private Point CreateNewPoint() {
      var pointObject = Instantiate(locatorPointPrefab, locatorPoints);
      var distanceObject = Instantiate(locatorDistancePrefab, locatorDistances);

      var locatorPoint = pointObject.GetComponent<LocatorPoint>();
      var locatorDistance = distanceObject.GetComponent<LocatorDistance>();

      locatorPoint.gameObject.SetActive(false);
      locatorDistance.gameObject.SetActive(false);

      return new Point {
        locatorPoint = locatorPoint,
        locatorDistance = locatorDistance
      };
    }

    public void RemoveTarget(string id) {
      if (!locatorTargets.TryGetValue(id, out var target))
        return;

      ResetPoint(target.point);
      pointsPool.Enqueue(target.point);

      locatorTargets.Remove(id);
    }

    private void ResetPoint(Point p) {
      p.locatorPoint.Hide();
      p.locatorDistance.Hide();
    }

    private void OnGamePausedHandler() {
      paused = true;
      pointerUI.SetActive(false);
    }

    private void OnGameResumedHandler() {
      paused = false;
    }

    private void CheckArea() {
      if (locatorTargets.Count == 0 || hidden || paused)
        return;

      visiblePoints.Clear();
      var showLocator = false;

      var playerPos = playerTransform.position;

      foreach (var kvp in locatorTargets) {
        var target = kvp.Value;

        var vp = cam.WorldToViewportPoint(target.target);

        var onScreen =
          vp.z > 0 &&
          vp.x > 0 && vp.x < 1 &&
          vp.y > 0 && vp.y < 1;

        if (onScreen) {
          ResetPoint(target.point);
          continue;
        }

        var dir = target.target - playerPos;
        var distance = dir.magnitude / cellSizeX;

        target.point.locatorPoint.UpdateArrow(dir.normalized);
        target.point.locatorDistance.UpdateDistance(distance);

        target.point.locatorPoint.Show();
        target.point.locatorDistance.Show();

        visiblePoints.Add(new VisiblePoint {
          distance = distance,
          point = target.point.locatorPoint
        });

        showLocator = true;
      }

      if (Time.time - lastSortTime > sortInterval && visiblePoints.Count > 1) {
        lastSortTime = Time.time;

        visiblePoints.Sort((a, b) => b.distance.CompareTo(a.distance));

        for (var i = 0; i < visiblePoints.Count; i++) {
          visiblePoints[i].point.transform.SetAsLastSibling();
        }
      }

      pointerUI.SetActive(showLocator);
    }
  }
}