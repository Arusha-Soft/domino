using Project.Core;
using Project.Network;
using System.Collections;
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

        [SerializeField] private ulong[] m_OwnedDominoIds;

        public override void OnNetworkSpawn()
        {
            gameObject.name = ActorName.Value.ToString();

            ActorName.OnValueChanged += OnChangeName;
        }

        public override void OnNetworkDespawn()
        {
            ActorName.OnValueChanged -= OnChangeName;
        }

        [ClientRpc]
        public void InitializeClientRpc(ulong[] dominoIds)
        {
            m_OwnedDominoIds = dominoIds;
            Dominos = DominoGenerator.Instance.GetDominosById(m_OwnedDominoIds);

            if (IsOwner)
            {
                RebuildDeckServerRpc();
            }
        }

        [ServerRpc]
        private void RebuildDeckServerRpc()
        {
            for (int i = 0; i < Dominos.Count; i++)
            {
                Dominos[i].GetComponent<NetworkObject>().TrySetParent(m_2dGorup.transform);
            }

            ArrageDominosClientRpc();
        }

        [ClientRpc]
        private void ArrageDominosClientRpc()
        {
            //m_2dGorup.ArrangeChildren();
        }

        private void OnChangeName(FixedString128Bytes pre, FixedString128Bytes newValue)
        {
            gameObject.name = newValue.ToString();
        }
    }
}