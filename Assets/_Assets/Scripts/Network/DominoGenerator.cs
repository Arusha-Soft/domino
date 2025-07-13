using AS.Project.Core;
using Project.Core;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Project.Network
{
    public class DominoGenerator : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private ActorSpawner m_ActorSpawner;

        [Header("Settings")]
        [SerializeField] private float m_GenerateRadius = 5f;
        [SerializeField] private float m_MoveDuration = 2;
        [SerializeField] private Domino m_DominoPrefab;
        [SerializeField] private List<DominoProperties> m_DominoProperties;

        private List<Domino> m_Dominos = new List<Domino>();

        [SerializeField] private ulong[] m_DominoIds;
        [SerializeField] private ulong[] m_RandomizedDominoIds;
        [SerializeField] private ulong[] m_OwnedDominoIds;

        public override void OnNetworkSpawn()
        {
            m_ActorSpawner.OnSpawnFinished += OnSpawnFinished;
        }

        private void OnSpawnFinished()
        {
            if (!IsHost)
            {
                return;
            }

            m_DominoIds = new ulong[m_DominoProperties.Count];

            for (int i = 0; i < m_DominoProperties.Count; i++)
            {
                Domino domino = Instantiate(m_DominoPrefab);
                Vector3 randomRotation = new Vector3(0, 0, Random.Range(0, 360));
                Vector3 randomPosition = Random.insideUnitSphere * m_GenerateRadius;
                randomPosition.z = 0;

                domino.transform.position = randomPosition;
                domino.transform.rotation = Quaternion.Euler(randomRotation);

                domino.NetworkObject.Spawn(true);
                m_DominoIds[i] = domino.NetworkObject.NetworkObjectId;
            }

            SendDominoIdsClientRpc(m_DominoIds);
            m_RandomizedDominoIds = m_DominoIds;
            Shuffle(m_RandomizedDominoIds);
            SetClientDominoClientRpc(m_RandomizedDominoIds);
        }

        [ClientRpc]
        private void SendDominoIdsClientRpc(ulong[] dominoIds)
        {
            m_DominoIds = dominoIds;

            for (int i = 0; i < m_DominoIds.Length; i++)
            {
                NetworkObject dominoNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[m_DominoIds[i]];
                Domino domino = dominoNetworkObject.GetComponent<Domino>();
                domino.Init(m_DominoProperties[i]);
            }
        }

        [ClientRpc]
        private void SetClientDominoClientRpc(ulong[] randomizedDominoIds)
        {
            int idCount = randomizedDominoIds.Length / m_ActorSpawner.MaxPlayers;

            int startIndex = ((int)OwnerClientId * idCount);

            m_OwnedDominoIds = new ulong[idCount];
            int index = 0;

            for (int i = startIndex; i < idCount; i++)
            {
                m_OwnedDominoIds[index] = randomizedDominoIds[i];
                Debug.Log("Id: " + m_OwnedDominoIds[index]);
                index++;
            }
        }

        private void Shuffle(ulong[] array)
        {
            System.Random rng = new System.Random();

            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (array[k], array[n]) = (array[n], array[k]);
            }
        }

        [ContextMenu("G")]
        public void Generate()
        {
            List<FixedString128Bytes> randomDominos = GetRandomDominos();
            int count = m_DominoProperties.Count / NetworkManager.Singleton.ConnectedClientsIds.Count;

            for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
            {
                DominoData[] dominoDatas = new DominoData[count];

                for (int j = 0; j < count; j++)
                {
                    Vector3 randomRotation = new Vector3(0, 0, Random.Range(0, 360));
                    Vector3 randomPosition = Random.insideUnitSphere * m_GenerateRadius;
                    randomPosition.z = 0;
                    int randomIndex = Random.Range(0, randomDominos.Count);

                    DominoData data = new DominoData()
                    {
                        Id = randomDominos[randomIndex],
                        Position = randomPosition,
                        Rotation = randomRotation
                    };

                    randomDominos.RemoveAt(randomIndex);
                    dominoDatas[j] = data;
                }

                ClientRpcParams rpcParams = new ClientRpcParams()
                {
                    Send = new ClientRpcSendParams()
                    {
                        TargetClientIds = new ulong[] { NetworkManager.Singleton.ConnectedClientsIds[i] }
                    }
                };

                GenerateDominoClientRpc(dominoDatas, rpcParams);
            }
        }

        private List<FixedString128Bytes> GetRandomDominos()
        {
            List<DominoProperties> temp = new List<DominoProperties>();
            temp.AddRange(m_DominoProperties);
            List<FixedString128Bytes> result = new List<FixedString128Bytes>();

            while (temp.Count > 0)
            {
                int randomIndex = Random.Range(0, temp.Count);
                result.Add(temp[randomIndex].Id);
                temp.RemoveAt(randomIndex);
            }

            return result;
        }

        [ClientRpc()]
        private void GenerateDominoClientRpc(DominoData[] dominos, ClientRpcParams rpcParams = default)
        {
            for (int i = 0; i < dominos.Length; i++)
            {
                Debug.Log(dominos[i].Id);
                Domino domino = Instantiate(m_DominoPrefab, dominos[i].Position, Quaternion.Euler(dominos[i].Rotation));
                domino.Init(GetDominoPropertyById(dominos[i].Id.ToString()));
                m_Dominos.Add(domino);
            }
        }

        private DominoProperties GetDominoPropertyById(string id)
        {
            for (int i = 0; i < m_DominoProperties.Count; i++)
            {
                if (m_DominoProperties[i].Id == id)
                {
                    return m_DominoProperties[i];
                }
            }

            return null;
        }

        [System.Serializable]
        public struct DominoData : INetworkSerializable
        {
            public FixedString128Bytes Id;
            public Vector3 Position;
            public Vector3 Rotation;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Id);
                serializer.SerializeValue(ref Position);
                serializer.SerializeValue(ref Rotation);
            }
        }
    }
}