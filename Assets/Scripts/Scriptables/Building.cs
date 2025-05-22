using UnityEngine;

namespace Scriptables {
  [CreateAssetMenu(menuName = "Buildings/Create Building", fileName = "Building")]
  public class Building : BaseScriptableObject {
    public int SizeX;
    public int SizeY;
    [SerializeField] private Color previewColor;
    [SerializeField] private Color blockColor;
    public Color PreviewColor => previewColor;
    public Color BlockColor => blockColor;
  }
}