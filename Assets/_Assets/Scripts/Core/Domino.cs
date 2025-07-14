using AS.Project.Core;
using Unity.Netcode;
using UnityEngine;

namespace Project.Core
{
    public class Domino : NetworkBehaviour
    {
        [SerializeField] private SpriteRenderer m_Sprite;

        private DominoProperties m_Properties;

        public void Init(DominoProperties properties)
        {
            m_Properties = properties;
            SetSprite(properties.Icon);
        }

        public Domino SetSprite(Sprite sprite)
        {
            m_Sprite.sprite = sprite;
            return this;
        }
    }
}