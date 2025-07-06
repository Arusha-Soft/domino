using AS.Project.Core;
using Project.Core;
using System.Collections.Generic;
using UnityEngine;

public class DominoGenerator : MonoBehaviour
{
    [SerializeField] private float m_GenerateRadius = 5f;
    [SerializeField] private float m_MoveDuration = 2;
    [SerializeField] private Domino m_DominoPrefab;
    [SerializeField] private List<DominoProperties> m_DominoProperties;

    private List<Domino> m_Dominos;

    public IReadOnlyList<Domino> Dominos => m_Dominos;


    private void Awake()
    {
        
    }

    public void Generate()
    {
        for (int i = 0; i < m_DominoProperties.Count; i++)
        {
            Vector3 random = Random.insideUnitSphere * 360;
            random.y = 0;

            Domino domino = Instantiate(m_DominoPrefab, Vector3.zero, Quaternion.Euler(random));
        }
    }
}
