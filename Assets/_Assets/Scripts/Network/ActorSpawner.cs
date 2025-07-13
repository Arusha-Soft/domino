using Project.Gameplay;
using Project.UI;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Project.Network
{
    public class ActorSpawner : NetworkBehaviour
    {
        [SerializeField] private Actor m_ActorPrefab;
        [SerializeField] public int MaxPlayers = 4;
        [SerializeField] private List<Transform> m_SpawnPoints;
        [SerializeField] private List<Actor> m_Players = new List<Actor>();

        public event Action OnSpawnFinished;

        private NetworkVariable<int> m_PlayerCount = new NetworkVariable<int>();

        private void OnEnable()
        {
            NetworkHUD.OnStart += OnStart;
        }

        private void OnStart()
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;

            m_PlayerCount.OnValueChanged += OnPlayerCountChanged;
        }

        private void OnDisable()
        {
            NetworkHUD.OnStart -= OnStart;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (IsHost)
            {
                Actor actor = Instantiate(m_ActorPrefab);
                actor.ActorName.Value = new FixedString128Bytes($"Player_{clientId}");
                actor.NetworkObject.SpawnAsPlayerObject(clientId, true);

                m_Players.Add(actor);
                m_PlayerCount.Value++;
            }
        }

        private void OnPlayerCountChanged(int previousValue, int newValue)
        {
            if (IsClient)
            {
                if (newValue == MaxPlayers)
                {
                    SetPlayerPositions();
                }
            }
        }

        private void SetPlayerPositions()
        {
            Debug.Log("SetPlayerPositions");

            int offset = (int)NetworkManager.LocalClientId;
            int playerCount = m_PlayerCount.Value;
            int counter = 0;

            int startIndex = offset == 0 ? 0 : playerCount - offset;
            int index = startIndex;
            List<Transform> tempSpawnPoints = new List<Transform>();

            while (counter < playerCount)
            {
                tempSpawnPoints.Add(m_SpawnPoints[index]);

                if (index + 1 > playerCount - 1)
                {
                    index = 0;
                }
                else
                {
                    index++;
                }

                counter++;
            }

            for (int i = 0; i < tempSpawnPoints.Count; i++)
            {
                NetworkClient actor = NetworkManager.ConnectedClients[(ulong)i];
                actor.PlayerObject.gameObject.transform.SetPositionAndRotation(tempSpawnPoints[i].position, tempSpawnPoints[i].rotation);
            }

            OnSpawnFinished?.Invoke();
        }

        private void OnClientDisconnect(ulong clientId)
        {

        }
    }
}