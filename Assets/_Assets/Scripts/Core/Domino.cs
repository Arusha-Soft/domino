using AS.Project.Core;
using UnityEngine;

namespace Project.Core
{
    public class Domino : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Sprite;

        public void Init(DominoProperties properties)
        {
            m_Sprite.sprite = properties.Icon;
        }
    }
}