using AS.Project.Core;
using Unity.Netcode;
using UnityEngine;

namespace Project.Core
{
    public class Domino : NetworkBehaviour
    {
        [SerializeField] private SpriteRenderer m_Sprite;

        public void Init(DominoProperties properties)
        {
            m_Sprite.sprite = properties.Icon;
        }
    }
}