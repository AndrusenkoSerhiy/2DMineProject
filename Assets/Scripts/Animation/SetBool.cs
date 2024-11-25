using UnityEngine;

public class SetBool : StateMachineBehaviour {
    public string boolName;  
    public bool enterValue;
    public bool exitValue;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (!string.IsNullOrEmpty(boolName))
            animator.SetBool(boolName, enterValue);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (!string.IsNullOrEmpty(boolName))
            animator.SetBool(boolName, exitValue);
    }
}
