using System;
using UnityEngine;

namespace AS.Project.Core
{
    [CreateAssetMenu(fileName = "DominoProperties", menuName = "Project/Core/DominoProperties")]
    public class DominoProperties : ScriptableObject
    {
        public string Id;
        public Sprite Icon;
        public int LeftPoint;
        public int RightPoint;

        [ContextMenu("Generate Id")]
        private void GenerateId()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}