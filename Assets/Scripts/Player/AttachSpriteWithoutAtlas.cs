using Spine;
using Spine.Unity;
using Spine.Unity.AttachmentTools; // Для роботи з AttachmentTools
using UnityEngine;

public class AttachSpriteWithoutAtlas : MonoBehaviour {
  public SkeletonMecanim skeletonMecanim; // Прив'язка до SkeletonMecanim
  public Sprite spriteToAttach; // Спрайт, який хочемо прикріпити

  [SpineSlot] public string slotName = "hand_slot"; // Назва слота

  void Start() {
    AttachSpriteToSlot(slotName, spriteToAttach);
  }

  void AttachSpriteToSlot(string slotName, Sprite sprite) {
    if (sprite == null || skeletonMecanim == null) return;

    // Отримуємо Shader для створення RegionAttachment
    Shader shader = Shader.Find("Spine/Skeleton");

    // Створюємо RegionAttachment зі спрайта та вказуємо Shader
    RegionAttachment attachment = sprite.ToRegionAttachmentPMAClone(shader);

    // Знаходимо слот і прикріплюємо спрайт до слота
    Slot slot = skeletonMecanim.Skeleton.FindSlot(slotName);
    if (slot != null && attachment != null) {
      slot.Attachment = attachment;
      Debug.Log($"Sprite '{sprite.name}' успішно прикріплено до слота '{slotName}' для SkeletonMecanim.");
    }
    else {
      Debug.LogError("Слот не знайдено або Attachment не створено.");
    }
  }
}