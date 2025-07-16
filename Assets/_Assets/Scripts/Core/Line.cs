using UnityEngine;

namespace Project.Core
{
    public class Line : MonoBehaviour
    {
        [SerializeField] private Transform m_StartLine;
        [SerializeField] private Transform m_EndLine;

        public Transform Start => m_StartLine;
        public Transform End => m_EndLine;


        public Vector3 GetMiddle() => Vector3.Lerp(m_StartLine.position, m_EndLine.position, 0.5f);

        private void OnDrawGizmos()
        {
            if (m_StartLine == null || m_EndLine == null) return;

            Gizmos.DrawLine(m_StartLine.position, m_EndLine.position);
        }
    }
}