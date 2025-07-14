using Project.Core;
using Project.Network;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Project.Gameplay
{
    public abstract class Actor : NetworkBehaviour
    {
        [SerializeField] private Horizontal2dGroup m_2dGorup;
        [SerializeField] private Sprite m_UnknownDominoSprite;
        [SerializeField] private float m_NormalSpacing = 0;
        [SerializeField] private float m_SideSpacing = 0.5f;

        public NetworkVariable<FixedString128Bytes> ActorName = new NetworkVariable<FixedString128Bytes>();

        protected List<Domino> Dominos;

        private int m_SpawnPointIndex;

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

        public void SetSpawnPointInedex(int index)
        {
            m_SpawnPointIndex = index;
        }

        [ServerRpc]
        private void RebuildDeckServerRpc()
        {
            for (int i = 0; i < Dominos.Count; i++)
            {
                Dominos[i].GetComponent<NetworkObject>().TrySetParent(m_2dGorup.transform);
            }

            EnableDominoNetwrokTransformsClientRpc(false);
            ArrageDominosClientRpc();
        }

        [ClientRpc]
        private void ArrageDominosClientRpc()
        {
            if (!IsLocalPlayer)
            {
                for (int i = 0; i < Dominos.Count; i++)
                {
                    Dominos[i].SetSprite(m_UnknownDominoSprite);
                }
            }

            Debug.Log($"Player Id: {OwnerClientId} Index: {m_SpawnPointIndex}");

            float spacing = m_SpawnPointIndex % 2 == 0 ? m_NormalSpacing : m_SideSpacing;

            m_2dGorup.SetSpacing(spacing)
                .ArrangeChildren();
        }

        [ClientRpc]
        private void EnableDominoNetwrokTransformsClientRpc(bool enabled)
        {
            for (int i = 0; i < Dominos.Count; i++)
            {
                Dominos[i].GetComponent<NetworkTransform>().enabled = enabled;
            }
        }

        private void OnChangeName(FixedString128Bytes pre, FixedString128Bytes newValue)
        {
            gameObject.name = newValue.ToString();
        }
    }
}