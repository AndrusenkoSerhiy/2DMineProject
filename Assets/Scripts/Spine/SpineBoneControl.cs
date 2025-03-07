using Settings;
using Spine.Unity;
using UnityEngine;

namespace Spine {
  public class SpineBoneControl : MonoBehaviour {
    public SkeletonAnimation skeletonAnimation;
    [SpineBone(dataField: "skeletonAnimation")]
    public string boneName;
    
    private Bone bone;

    void Start() {
      bone = skeletonAnimation.skeleton.FindBone(boneName);
      skeletonAnimation.AnimationState.SetAnimation(0, "idle", true);
      skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
    }

    private void LateUpdate() {
      if (bone!=null) {
        var mousePos = GameManager.Instance.UserInput.GetMousePosition();
        Vector3 skeletonSpacePoint = skeletonAnimation.transform.InverseTransformPoint(mousePos);
        bone.WorldX = mousePos.x;//Mathf.Sin(Time.time) * 10f;
        bone.WorldY = mousePos.y;//Mathf.Cos(Time.time) * 10f;
        //Debug.LogError($"bone is exist {bone.WorldX} | {bone.WorldY}");
        bone.SetLocalPosition(skeletonSpacePoint);
        //skeletonAnimation.Skeleton.UpdateWorldTransform(Skeleton.Physics.Pose);
      }
    }
  }
}