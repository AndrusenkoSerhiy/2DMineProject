using UnityEngine;

namespace Scriptables {
  [CreateAssetMenu(fileName = "AnimatorParameters", menuName = "AnimatorParameters")]
  public class AnimatorParameters : ScriptableObject {
    [SerializeField] private string fall;
    [SerializeField] private string velocityX;
    [SerializeField] private string velocityY;
    [SerializeField] private string jump;
    [SerializeField] private string attack;
    [SerializeField] private string isDead;
    [SerializeField] private string takeDamage;
    [SerializeField] private string die;
    
    public int FallHash => Animator.StringToHash(fall);
    public int VelocityXHash => Animator.StringToHash(velocityX);
    public int VelocityYHash => Animator.StringToHash(velocityY);
    public int JumpHash => Animator.StringToHash(jump);
    public int AttackHash => Animator.StringToHash(attack);
    public int IsDeadHash => Animator.StringToHash(isDead);
    public int TakeDamage => Animator.StringToHash(takeDamage);
    public int Die => Animator.StringToHash(die);
  }
}