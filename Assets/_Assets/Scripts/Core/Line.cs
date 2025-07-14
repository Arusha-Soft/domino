using UnityEngine;

namespace Project.Core
{
    public class Line : MonoBehaviour
    {
        [SerializeField] private Transform m_StartLine;
        [SerializeField] private Transform m_EndLine;

        public Transform Start => m_StartLine;
        public Transform End => m_EndLine;

        private void OnDrawGizmos()
        {
            if (m_StartLine == null || m_EndLine == null) return;

            Gizmos.DrawLine(m_StartLine.position, m_EndLine.position);
        }
    }
}