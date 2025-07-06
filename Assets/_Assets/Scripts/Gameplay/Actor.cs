using Project.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Gameplay
{
    public abstract class Actor : MonoBehaviour
    {
        [SerializeField] private Horizontal2dGroup m_2dGorup;

        protected List<Domino> Dominos;

        public void Initialize(List<Domino> dominos)
        {
            Dominos = dominos;
            RebuildDeck();
        }

        private void RebuildDeck()
        {
            for (int i = 0; i < Dominos.Count; i++)
            {
                Dominos[i].transform.SetParent(m_2dGorup.transform);
            }

            m_2dGorup.ArrangeChildren();
        }
    }
}