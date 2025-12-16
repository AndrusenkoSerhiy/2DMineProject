using System;
using System.Collections.Generic;
using System.Linq;
using Animation;
using Inventory;
using Scriptables;
using Scriptables.Stats;
using UnityEngine;
using Utils;
using World;

namespace Player {
  public class BaseAttack : MonoBehaviour {
    [SerializeField] protected PlayerStatsObject statsObject;
    [SerializeField] protected Animator animator;
    [SerializeField] protected ObjectHighlighter objectHighlighter;
    [SerializeField] protected BoxCollider2D attackCollider;
    [SerializeField] protected Transform attackTransform;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private List<string> lockReasons = new();
    protected float attackTimeCounter;
    protected LayerMask attackLayer;
    protected int attackID;
    protected int maxTargets;

    protected bool isRangedAttack;

    protected Vector2 colliderSize;

    protected List<IDamageable> targets = new();
    [SerializeField] private bool isHighlightLock;
    [SerializeField] private Vector2 originalSize;
    [SerializeField] protected int lookDirection;
    protected AnimatorParameters animParam;
    protected PlayerEquipment playerEquipment;

    protected PlayerStats playerStats;
    protected bool firstAttack;
    private Coords coords;
    public PlayerStats PlayerStats => playerStats ??= GameManager.Instance.CurrPlayerController.PlayerStats;

    protected virtual void Awake() {
      AnimationEventManager.onAttackStarted += HandleAnimationStarted;
      //AnimationEventManager.onAttackEnded += HandleAnimationEnded;
      animParam = GameManager.Instance.AnimatorParameters;
      playerEquipment = GameManager.Instance.PlayerEquipment;
    }

    //use when we exit from robot 
    public void ClearLockList() {
      lockReasons.Clear();
    }
    protected virtual void Start() {
      attackTimeCounter = PlayerStats.TimeBtwAttacks;
      PrepareAttackParams();
      originalSize = attackCollider.size;
      GameManager.Instance.UserInput.OnAttackPerformed += PressAttack;
      GameManager.Instance.UserInput.OnAttackCanceled += CancelAttack;
    }

    protected virtual void PressAttack(object sender, EventArgs e) {
      animator.SetBool(animParam.IsAttacking, true);
    }

    protected virtual void CancelAttack(object sender, EventArgs e) {
      animator.SetBool(animParam.IsAttacking, false);
      firstAttack = false;
    }

    public bool GetIsAttacking() {
      return animator.GetBool(animParam.IsAttacking);
    }
    //reason use for block action when you lock hightlight in build mode and open/closed inventory
    public void LockHighlight(bool state, string reason = "", bool lockPos = true) {
      if (state && !string.IsNullOrEmpty(reason) && !lockReasons.Contains(reason)) {
        lockReasons.Add(reason);
      }
      
      if (!state && lockReasons.Contains(reason)) {
        lockReasons.Remove(reason);
      }

      if (!state && lockReasons.Count > 0) {
        return;
      }
      
      if (lockPos) {
        isHighlightLock = state;
      }
      
      if (state) {
        attackCollider.transform.localPosition = new Vector3(0, 1, 0);
        if (originalSize.Equals(Vector2.zero)) {
          originalSize = attackCollider.size;
        }
        attackCollider.size = Vector2.zero;
      }
      else {
        attackCollider.size = originalSize;
      }

      objectHighlighter.EnableCrosshair(!state);
    }

    protected virtual void PrepareAttackParams() {
    }

    protected virtual void Update() {
      if (isHighlightLock)
        return;

      UpdateColliderPos();
      HandleAttack();
      GetDirection();
    }

    public virtual void GetDirection() { }

    protected virtual void TriggerAttack() {
      attackTimeCounter = 0f;
    }

    private void HandleAttack() {
      if (GameManager.Instance.UserInput.IsAttacking() /*&& currentTarget != null*/
          && attackTimeCounter >= PlayerStats.TimeBtwAttacks) {
        TriggerAttack();
      }

      attackTimeCounter += Time.deltaTime;
    }

    private void UpdateColliderPos() {
      var mousePos = GetMousePosition();
      // Calculate direction and distance from parent
      var parentPosition = attackTransform.position;
      var direction = (mousePos - parentPosition).normalized;
      var distance = Vector3.Distance(parentPosition, mousePos);
      // Clamp the distance between minDistance and maxDistance
      var clampedDistance = Mathf.Clamp(distance, minDistance, maxDistance);
      // Set the new position of the child collider
      var newPosition = parentPosition + direction * clampedDistance;
      newPosition.z = 0f;
      attackCollider.transform.position = newPosition;
    }

    protected void UpdateParams(float minDist, float maxDist, float sizeX, float sizeY) {
      minDistance = minDist;
      maxDistance = maxDist;
      attackCollider.size = new Vector2(sizeX, sizeY);
    }

    private Vector3 GetMousePosition() {
      var mousePos =
        GameManager.Instance.MainCamera.ScreenToWorldPoint(GameManager.Instance.UserInput.GetMousePosition());
      mousePos.z = 0f;
      return mousePos;
    }
    
    List<IDamageable> results = new();
    private IOrderedEnumerable<KeyValuePair<Vector2Int, IDamageable>> sorted;

    protected List<IDamageable> LockByWall() {
      results.Clear();
      var tempList = SetTargetsFromHighlight();
      
      if (tempList == null || tempList.Count == 0) {
        return null;
      }

      var cellMap = CreateCellMap(tempList);
      
      if (lookDirection == 1 || lookDirection == -1) {
        return CheckVertical(cellMap);
      }
      if (lookDirection == 0) {
        return CheckHorizontal(cellMap);
      }
      if (lookDirection == 2 || lookDirection == -2) {
        return CheckDiagonal(cellMap);
      }
      
      return null;
    }
    
    private Dictionary<Vector2Int, IDamageable> CreateCellMap(List<IDamageable> tempList) {
      var map = new Dictionary<Vector2Int, IDamageable>();
      foreach (var cell in tempList) {
        CoordsTransformer.WorldToGrid(cell.GetPosition(), ref coords);
        var key = new Vector2Int(coords.X, coords.Y);
        //add only first IDamageable
        if (!map.ContainsKey(key)) {
          map.Add(new Vector2Int(coords.X, coords.Y), cell);
        }  
      }
      return map;
    }
    
    private List<IDamageable> CheckDiagonal(Dictionary<Vector2Int, IDamageable> cellMap) {
        if (GameManager.Instance.CurrPlayerController.GetFlip() == 1) {
          if(lookDirection == 2)
            sorted = cellMap.OrderByDescending(pair => pair.Key.y).ThenBy(pair => pair.Key.x);
          if(lookDirection == -2)
            sorted = cellMap.OrderBy(pair => pair.Key.y).ThenBy(pair => pair.Key.x);
        }
        else {
          if(lookDirection == 2)
            sorted = cellMap.OrderByDescending(pair => pair.Key.y).ThenByDescending(pair => pair.Key.x);
          if(lookDirection == -2)
            sorted = cellMap.OrderBy(pair => pair.Key.y).ThenByDescending(pair => pair.Key.x);
        }
        // йдемо по горизонталі
        //Debug.LogError("diagonal");
      
        var rowCount = -1;
        var colCount = -1;
        var blocked = false;
        foreach (var cell in sorted) {
          var cellObj = GameManager.Instance.ChunkController.GetCell(cell.Key.x, cell.Key.y);
          if (cellObj != null && !cellObj.CanGetDamage) {
            if (rowCount == -1) {
              rowCount = cell.Key.y;
            }
            if (colCount == -1) {
              colCount = cell.Key.x;
            }
          }

          if (GameManager.Instance.CurrPlayerController.GetFlip() == 1) {
            if (lookDirection == -2) {
              if (colCount != -1 && rowCount != -1) {
                if ((cell.Key.x >= colCount && cell.Key.y >= rowCount)) {
                  continue;
                } 
              }
            }
            if (lookDirection == 2) {
              if (colCount != -1 && rowCount != -1) {
                if ((cell.Key.x >= colCount && cell.Key.y <= rowCount)) {
                  continue;
                } 
              }
            }
          }
          else {
            if (lookDirection == -2) {
              if (colCount != -1 && rowCount != -1) {
                if ((cell.Key.x <= colCount && cell.Key.y >= rowCount)) {
                  continue;
                }
              }
            }
            if (lookDirection == 2) {
              if (colCount != -1 && rowCount != -1) {
                if ((cell.Key.x <= colCount && cell.Key.y <= rowCount)) {
                  continue;
                }
              }
            }
          }
          
          results.Add(cell.Value);
        }

        return results;
    }

    private List<IDamageable> CheckVertical(Dictionary<Vector2Int, IDamageable> cellMap) {
      var playerCoords = GameManager.Instance.CurrPlayerController.PlayerCoords;
      // йдемо по вертикалі
      //Debug.LogError("vertical");
      sorted = lookDirection == -1
        ? cellMap.OrderBy(pair => pair.Key.x).ThenBy(pair => pair.Key.y)
        : cellMap.OrderByDescending(pair => pair.Key.x).ThenByDescending(pair => pair.Key.y);
      
      var colCount = -1;
      var blocked = false;
      foreach (var cell in sorted) {
        if (colCount != cell.Key.x) {
          colCount = cell.Key.x;
          blocked = false;
        }
        var cellObj = GameManager.Instance.ChunkController.GetCell(cell.Key.x, cell.Key.y);
        var special = GameManager.Instance.ChunkController.GetCell(cell.Key.x, playerCoords.Coords.Y + lookDirection);
        if (objectHighlighter.MaxHighlights != 1) {
          if (cellObj != null && !cellObj.CanGetDamage ||
              special != null && !special.CanGetDamage && playerCoords.Coords.X != cell.Key.x) {
            blocked = true;
          }

          if (blocked && colCount == cell.Key.x) {
            continue;
          }
        }

        if (!blocked) {
          results.Add(cell.Value);
        }
      }
      return results;
    }

    private List<IDamageable> CheckHorizontal(Dictionary<Vector2Int, IDamageable> cellMap) {
      var sorted = GameManager.Instance.CurrPlayerController.GetFlip() == 1
        ? cellMap.OrderBy(pair => pair.Key.y).ThenBy(pair => pair.Key.x)
        : cellMap.OrderBy(pair => pair.Key.y).ThenByDescending(pair => pair.Key.x);

        // йдемо по горизонталі
        //Debug.LogError("horizontal");
        var playerCoords = GameManager.Instance.CurrPlayerController.PlayerCoords;
        var rowCount = -1;
        var blocked = false;
        foreach (var cell in sorted) {
          if (rowCount != cell.Key.y) {
            rowCount = cell.Key.y;
            blocked = false;
          }
          
          var cellObj = GameManager.Instance.ChunkController.GetCell(cell.Key.x, cell.Key.y);
          var special = GameManager.Instance.ChunkController.GetCell(playerCoords.Coords.X + GameManager.Instance.CurrPlayerController.GetFlip(), cell.Key.y);
          //check if player damage
          if (objectHighlighter.MaxHighlights != 1) {
            if (cellObj != null && !cellObj.CanGetDamage ||
                special != null && !special.CanGetDamage && playerCoords.Coords.X != cell.Key.x) {
              blocked = true;
            }

            if (blocked && rowCount == cell.Key.y) {
              continue;
            }
          }

          if (!blocked) {
            results.Add(cell.Value);
          }
        }
        
      return results;
    }
    protected virtual void Attack() {
      //return to previous version, without check block 
      //SetTargetsFromHighlight();
      //targets = SetTargetsFromHighlight();
      targets = LockByWall();
      
      if (targets == null || targets.Count == 0) {
        return;
      }
      
      foreach (var target in targets) {
        if (target == null) continue;
        var damage = target.DamageableType == DamageableType.Enemy ? playerStats.EntityDamage : playerStats.BlockDamage;
        target.Damage(damage, true);
      }

      AfterTargetsTakenDamage(targets.Count);
      
      for (int i = 0; i < targets.Count; i++) {
        targets[i]?.AfterDamageReceived();
      }
    }

    protected virtual void RangeAttack() {
    }

    protected virtual void AfterTargetsTakenDamage(int targetsCount) {
    }

    private List<IDamageable> SetTargetsFromHighlight() {
      List<IDamageable> tempTargets = new();
      foreach (var elem in objectHighlighter.Highlights) {
        tempTargets.Add(elem.damageableRef);
      }
      return tempTargets;
    }

    private void HandleAnimationStarted(AnimationEvent animationEvent, GameObject go) {
      if (go != gameObject) {
        return;
      }

      if (isRangedAttack) {
        RangeAttack();
      }
      else {
        Attack();
      }
      
      DestroyTarget();
    }

    private void HandleAnimationEnded(AnimationEvent animationEvent, GameObject go) {
      Debug.LogError("Animation Ended");
      if (go != gameObject)
        return;
      
      DestroyTarget();
    }

    protected void DestroyTarget() {
      if (targets == null || targets.Count == 0) {
        return;
      }
      
      foreach (var t in targets) {
        if (t == null) continue;
        var getHp = t.GetHealth();
        if (getHp <= 0) {
          t.DestroyObject();
        }
      }

      ClearTarget();
    }

    private void ClearTarget() {
      targets.Clear();
    }

    protected virtual void OnDestroy() {
      AnimationEventManager.onAttackStarted -= HandleAnimationStarted;
      //AnimationEventManager.onAttackEnded -= HandleAnimationEnded;
      if (GameManager.HasInstance) {
        GameManager.Instance.UserInput.OnAttackPerformed -= PressAttack;
        GameManager.Instance.UserInput.OnAttackCanceled -= CancelAttack;
      }
    }
  }
}