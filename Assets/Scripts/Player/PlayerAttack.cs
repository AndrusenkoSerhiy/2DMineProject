using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;
using UnityEngine.Rendering;
using World;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Transform _attackTransform;
    [SerializeField] private float _attackRange = 0.5f;
    [SerializeField] private float _timeBtwAttacks = 0.2f;
    [SerializeField] private LayerMask _attackLayer;
    [SerializeField] private CellObjectsPool _pool;
    [SerializeField] private ScriptableStats _stats;
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimatorEventReceiver eventReceiver;
    public bool ShouldBeDamaging { get; private set; } = false;
    private List<IDamageable> iDamageables = new List<IDamageable>();
    private RaycastHit2D[] hits;

    private float _attackTimeCounter;

    private void Awake()
    {
        eventReceiver.OnAnimationStarted += HandleAnimationStarted;
        eventReceiver.OnAnimationEnded += HandleAnimationEnded;
    }

    private void OnDestroy()
    {
        eventReceiver.OnAnimationStarted -= HandleAnimationStarted;
        eventReceiver.OnAnimationEnded -= HandleAnimationEnded;
    }

    private void Start()
    {
        _attackTimeCounter = _timeBtwAttacks;
    }

    private void Update()
    {
        if (UserInput.instance.controls.GamePlay.Attack.WasPressedThisFrame() && _attackTimeCounter >= _timeBtwAttacks)
        {
            _attackTimeCounter = 0f;

            //Attack();
            _animator.SetTrigger("Attack");
        }

        _attackTimeCounter += Time.deltaTime;
    }

    private void Attack()
    {
        hits = Physics2D.CircleCastAll(_attackTransform.position, _attackRange, transform.right, 0f, _attackLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            IDamageable iDamageable = hits[i].collider.gameObject.GetComponent<IDamageable>();

            if (iDamageable == null || iDamageable.HasTakenDamage)
            {
                continue;
            }

            Debug.LogError(iDamageable);

            iDamageable.Damage(_stats.AttackDamage);
            iDamageables.Add(iDamageable);

            float hp = iDamageable.GetHealth();

            if (hp <= 0)
            {
                _pool.ReturnObject(iDamageable as CellObject);
            }
        }
    }

    public IEnumerator DamageWhileSlashIsActive()
    {
        Debug.LogError("DamageWhileSlashIsActive");
        ShouldBeDamaging = true;

        while (ShouldBeDamaging)
        {
            Attack();

            yield return null;
        }

        ReturnAttackablesToDamageable();
    }

    private void ReturnAttackablesToDamageable()
    {
        foreach (IDamageable damaged in iDamageables)
        {
            damaged.HasTakenDamage = false;
        }

        iDamageables.Clear();
    }

    public void ShouldBeDamagingToFalse()
    {
        ShouldBeDamaging = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(_attackTransform.position, _attackRange);
    }

    #region Animation Triggers

    private void HandleAnimationStarted(AnimationEvent animationEvent)
    {
        Debug.LogError("HandleAnimationStarted");
        StartCoroutine(DamageWhileSlashIsActive());
    }

    private void HandleAnimationEnded(AnimationEvent animationEvent)
    {
        Debug.LogError("HandleAnimationEnded");
        ShouldBeDamagingToFalse();
    }

    #endregion
}
