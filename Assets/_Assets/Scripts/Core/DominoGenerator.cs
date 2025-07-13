using AS.Project.Core;
using Project.Core;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class DominoGenerator : NetworkBehaviour
{
    [SerializeField] private float m_GenerateRadius = 5f;
    [SerializeField] private float m_MoveDuration = 2;
    [SerializeField] private Domino m_DominoPrefab;
    [SerializeField] private List<DominoProperties> m_DominoProperties;

    private List<Domino> m_Dominos = new List<Domino>();

    public IReadOnlyList<Domino> Dominos => m_Dominos;


    private void Awake()
    {
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
