using Project.Core;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Project.Gameplay
{
    public abstract class Actor : NetworkBehaviour
    {
        [SerializeField] private Horizontal2dGroup m_2dGorup;

        public NetworkVariable<FixedString128Bytes> ActorName = new NetworkVariable<FixedString128Bytes>();

        protected List<Domino> Dominos;

        public override void OnNetworkSpawn()
        {
            gameObject.name = ActorName.Value.ToString();

            ActorName.OnValueChanged += OnChangeName;
        }

        public override void OnNetworkDespawn()
        {
            ActorName.OnValueChanged -= OnChangeName;
        }

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

        private void OnChangeName(FixedString128Bytes pre , FixedString128Bytes newValue)
        {
            gameObject.name = newValue.ToString();
        }
    }
}