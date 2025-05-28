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
    [SerializeField] private Transform locatorPoints;
    [SerializeField] private Transform locatorDistances;
    [SerializeField] private GameObject locatorPointPrefab;
    [SerializeField] private GameObject locatorDistancePrefab;
    [SerializeField] private GameObject pointerUI;
    [SerializeField] private Camera cam;
    [SerializeField] private List<LocatorPoint> points = new();
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

    #region Save/Load

    public int Priority => LoadPriority.LOCATOR;

    public void Save() {
      saveLoadSystem.gameData.LocatorPointsData.Clear();

      foreach (var (id, target) in locatorTargets) {
        saveLoadSystem.gameData.LocatorPointsData.Add(new LocatorPointData
          { Id = id, Position = target.target, Color = allowedPoints[id].color });
      }
    }

    public void Load() {
      if (saveLoadSystem.IsNewGame() ||
          saveLoadSystem.gameData.LocatorPointsData.Count <= 0) {
        return;
      }

      foreach (var target in saveLoadSystem.gameData.LocatorPointsData) {
        SetTarget(target.Position, target.Id);
      }
    }

    public void Clear() {
      foreach (var (id, target) in locatorTargets) {
        target.point.locatorPoint.Hide();
        target.point.locatorDistance.Hide();
        pointsPool.Enqueue(target.point);
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

      for (var i = 0; i < points.Count; i++) {
        var point = points[i];
        var distance = distances[i];
        point.gameObject.SetActive(false);
        distance.gameObject.SetActive(false);
        pointsPool.Enqueue(new Point { locatorPoint = point, locatorDistance = distance });
      }

      foreach (var point in allowed) {
        allowedPoints.Add(point.id, point);
      }

      gameManager.OnGamePaused += OnGamePausedHandler;
      gameManager.OnGameResumed += OnGameResumedHandler;
    }

    private void Update() => CheckArea();

    public void ShowHide(bool state) {
      if (state) {
        hidden = false;
      }
      else {
        hidden = true;
        pointerUI.gameObject.SetActive(false);
      }
    }

    public void SetTarget(Vector3 position, string id) {
      if (!allowedPoints.ContainsKey(id)) {
        return;
      }

      if (locatorTargets.ContainsKey(id)) {
        return;
      }

      var point = pointsPool.Count > 0 ? pointsPool.Dequeue() : CreateNewPoint();
      var icon = allowedPoints[id].sprite;
      var color = allowedPoints[id].color;

      point.locatorPoint.SetPoint(icon, color);
      point.locatorDistance.SetPoint(icon);

      locatorTargets[id] = new LocatorTarget { target = position, point = point };
    }

    private Point CreateNewPoint() {
      var pointObject = Instantiate(locatorPointPrefab, locatorPoints);
      var distanceObject = Instantiate(locatorDistancePrefab, locatorDistances);
      var locatorPoint = pointObject.GetComponent<LocatorPoint>();
      var locatorDistance = distanceObject.GetComponent<LocatorDistance>();

      locatorPoint.gameObject.SetActive(false);
      locatorDistance.gameObject.SetActive(false);

      return new Point { locatorPoint = locatorPoint, locatorDistance = locatorDistance };
    }

    public void RemoveTarget(string id) {
      if (!locatorTargets.ContainsKey(id)) {
        return;
      }

      var target = locatorTargets[id];

      pointsPool.Enqueue(target.point);
      locatorTargets.Remove(id);

      target.point.locatorPoint.Hide();
      target.point.locatorDistance.Hide();
    }

    private void OnGamePausedHandler() {
      paused = true;
      pointerUI.gameObject.SetActive(false);
    }

    private void OnGameResumedHandler() {
      paused = false;
    }

    private void CheckArea() {
      if (locatorTargets.Count == 0 || hidden || paused) {
        return;
      }

      var showLocator = false;
      var visiblePoints = new List<(float distance, LocatorPoint locatorPoint)>();

      foreach (var (id, locatorTarget) in locatorTargets) {
        var screenPos = cam.WorldToScreenPoint(locatorTarget.target);

        if (screenPos.x > 0 && screenPos.x < Screen.width &&
            screenPos.y > 0 && screenPos.y < Screen.height) {
          locatorTarget.point.locatorPoint.Hide();
          locatorTarget.point.locatorDistance.Hide();
          continue;
        }

        var playerPos = gameManager.PlayerController.PlayerCoords.GetPosition();
        var directionToTarget = (locatorTarget.target - playerPos).normalized;
        var distance = Vector3.Distance(playerPos, locatorTarget.target) / gameManager.GameConfig.CellSizeX;

        locatorTarget.point.locatorPoint.UpdateArrow(directionToTarget);
        locatorTarget.point.locatorDistance.UpdateDistance(distance);

        locatorTarget.point.locatorPoint.Show();
        locatorTarget.point.locatorDistance.Show();

        visiblePoints.Add((distance, locatorTarget.point.locatorPoint));

        showLocator = true;
      }

      if (Time.time - lastSortTime > sortInterval) {
        lastSortTime = Time.time;

        visiblePoints.Sort((a, b) => b.distance.CompareTo(a.distance));

        foreach (var (_, point) in visiblePoints) {
          point.transform.SetAsLastSibling();
        }
      }

      pointerUI.gameObject.SetActive(showLocator);
    }
  }
}