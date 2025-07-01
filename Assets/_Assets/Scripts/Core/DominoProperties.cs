using UnityEngine;

namespace AS.Project.Core
{
    [CreateAssetMenu(fileName = "DominoProperties", menuName = "Project/Core/DominoProperties")]
    public class DominoProperties : ScriptableObject
    {
        public Sprite Icon;
        public int LeftPoint;
        public int RightPoint;
    }
}