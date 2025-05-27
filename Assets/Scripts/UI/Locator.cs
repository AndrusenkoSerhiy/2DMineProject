using System.Collections.Generic;
using Scriptables.Items;
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

  public class Locator : MonoBehaviour {
    [SerializeField] private Transform locatorPoints;
    [SerializeField] private Transform locatorDistances;
    [SerializeField] private GameObject locatorPointPrefab;
    [SerializeField] private GameObject locatorDistancePrefab;
    [SerializeField] private GameObject pointerUI;
    [SerializeField] private Camera cam;
    [SerializeField] private List<LocatorPoint> points = new();
    [SerializeField] private List<LocatorDistance> distances = new();
    [SerializeField] private List<ItemObject> allowBuildings = new();
    [SerializeField] private float sortInterval = 1f;

    private Queue<Point> pointsPool = new();
    private Dictionary<string, LocatorTarget> locatorTargets = new();
    private GameManager gameManager;
    private bool hidden;
    private bool paused;
    private float lastSortTime;

    private void Awake() {
      if (points.Count == 0 || points.Count != distances.Count) {
        Debug.LogError("Locator: Points and distances count mismatch or empty list.");
        return;
      }

      gameManager = GameManager.Instance;

      for (var i = 0; i < points.Count; i++) {
        var point = points[i];
        var distance = distances[i];
        point.gameObject.SetActive(false);
        distance.gameObject.SetActive(false);
        pointsPool.Enqueue(new Point { locatorPoint = point, locatorDistance = distance });
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

    public void SetTargetBuilding(Vector3 position, ItemObject itemObject) {
      if (!allowBuildings.Contains(itemObject)) {
        return;
      }

      SetTarget(position, itemObject.Id, itemObject.UiDisplay);
    }

    public void SetTarget(Vector3 position, string id, Sprite icon) {
      if (locatorTargets.ContainsKey(id)) {
        return;
      }

      var point = pointsPool.Count > 0 ? pointsPool.Dequeue() : CreateNewPoint();

      point.locatorPoint.SetPoint(icon);
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