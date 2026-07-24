using UnityEngine;

namespace SculptGame.Building
{
    [CreateAssetMenu(fileName = "NewBuildableObject", menuName = "SculptGame/Buildable Object Data")]
    public class BuildableObjectData : ScriptableObject
    {
        public string objectId = "cube";
        public string displayName = "큐브 (Cube)";
        public Sprite icon;
        public GameObject prefab;
        public PrimitiveType primitiveShape = PrimitiveType.Cube;
        public Color defaultColor = Color.white;
        public Vector3 defaultScale = Vector3.one;
    }
}
