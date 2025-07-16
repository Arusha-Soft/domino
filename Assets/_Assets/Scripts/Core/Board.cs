using Project.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace Project.Core
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private Line m_MiddleLine;
        [SerializeField] private Transform m_TempPointPrefab;
        [SerializeField] private Transform m_TempPointParent;
        [SerializeField] private float m_MoveAcceptThresholdDistance = 5f;
        [SerializeField] private float m_SpaceBetweenDominos = 0.1f;

        [SerializeField] private Domino[] m_AllDominos; // Todo make this list private and init on runtime

        private List<Domino> m_OnBoardDominos = new List<Domino>();
        private List<Transform> m_TempPoints = new List<Transform>();
        private ObjectPool<Transform> m_TempPointPool;
        private int OnBoardDominoCount => m_OnBoardDominos.Count;

        private void Awake()
        {
            m_TempPointPool = new ObjectPool<Transform>(OnCreate, OnGet, OnRelease);
            Init(m_AllDominos);
        }

        private void OnRelease(Transform transform)
        {
            transform.gameObject.SetActive(false);
        }

        private void OnGet(Transform transform)
        {
            transform.gameObject.SetActive(true);
        }

        private Transform OnCreate()
        {
            return Instantiate(m_TempPointPrefab, m_TempPointParent);
        }

        public void Init(Domino[] dominos)
        {
            m_AllDominos = dominos;

            for (int i = 0; i < m_AllDominos.Length; i++)
            {
                m_AllDominos[i].OnStartDrag += OnStartDrag;
                m_AllDominos[i].OnEndDrag += OnEndDraging;
            }
        }

        Domino selectedDomino;
        [ContextMenu("SetupDomino Checl")]
        private void SC()
        {
            Check(SetupDomino_FirstMove, SetupDomino_SecondMove, SetupDomino_OtherMoves);
        }

        #region SetupDomino
        private void SetupDomino_FirstMove()
        {
            for (int i = 0; i < m_AllDominos.Length; i++)
            {
                Domino domino = m_AllDominos[i];
                domino.SetStatus(domino.IsDouble() ? DominoStatus.Active : DominoStatus.Deactive);
            }
        }

        private void SetupDomino_SecondMove()
        {
            int value = m_OnBoardDominos[0].UpValue;

            for (int i = 0; i < m_AllDominos.Length; i++)
            {
                Domino domino = m_AllDominos[i];

                if (m_OnBoardDominos.Contains(domino))
                {
                    continue;
                }

                domino.SetStatus((domino.UpValue == value || domino.DownValue == value) ? DominoStatus.Active : DominoStatus.Deactive);
            }
        }

        private void SetupDomino_OtherMoves()
        {
            List<int> points = new List<int>(4);
            Domino lastDomino = m_OnBoardDominos[m_OnBoardDominos.Count - 1];
            Domino oneToLastDomino = m_OnBoardDominos[m_OnBoardDominos.Count - 2];

            points.Add(lastDomino.DownValue);
            points.Add(lastDomino.UpValue);
            points.Add(oneToLastDomino.DownValue);
            points.Add(oneToLastDomino.UpValue);

            points.RemoveDuplicatesByCount();

            for (int i = 0; i < m_AllDominos.Length; i++)
            {
                Domino domino = m_AllDominos[i];

                if (m_OnBoardDominos.Contains(domino))
                {
                    continue;
                }

                domino.SetStatus(DominoStatus.Deactive);

                for (int j = 0; j < points.Count; j++)
                {
                    if (domino.UpValue == points[j] || domino.DownValue == points[j])
                    {
                        domino.SetStatus(DominoStatus.Active);
                        break;
                    }
                }
            }
        }

        #endregion

        #region MoveDomino
        private void MoveDomino_FirstMove()
        {
            Transform tempPoint = GetTempPoint();
            tempPoint.MakeVertical();
            Vector3 centerOfLine = m_MiddleLine.GetMiddle();
            tempPoint.position = centerOfLine;
        }

        private void MoveDomino_SecondMove()
        {
            Transform tempPointRight = GetTempPoint();
            Transform tempPointLeft = GetTempPoint();
            Domino lastDomino = m_OnBoardDominos[0];
            int targetValue = lastDomino.UpValue; // both are same

            tempPointRight.MakeHorizontal(true, targetValue, selectedDomino);
            tempPointLeft.MakeHorizontal(false, targetValue, selectedDomino);
            tempPointRight.MoveHorizontal(true, lastDomino, m_SpaceBetweenDominos);
            tempPointLeft.MoveHorizontal(false, lastDomino, m_SpaceBetweenDominos);
        }

        private void MoveDomino_OtherMoves()
        {
            //TODO
        }
        #endregion

        private void Check(Action onFirstMove, Action onSecondMove, Action onOtherMoves)
        {
            if (OnBoardDominoCount == 0) // first move
            {
                onFirstMove?.Invoke();
            }
            else
            {
                if (OnBoardDominoCount == 1)// second move
                {
                    onSecondMove?.Invoke();
                }
                else // other moves
                {
                    onOtherMoves?.Invoke();
                }
            }
        }

        private Transform GetTempPoint()
        {
            Transform tempPoint = m_TempPointPool.Get();
            m_TempPoints.Add(tempPoint);
            return tempPoint;
        }

        private bool TryMoveDominoToTempPoint(Domino movedDomino)
        {
            Transform closestTempPoint = GetClosestTempPoint(movedDomino);
            float distance = VectorUtilities.DistanceXY(movedDomino.transform.position, closestTempPoint.position);

            if (distance > m_MoveAcceptThresholdDistance)
            {
                return false;
            }

            movedDomino.MoveTo(closestTempPoint);
            m_OnBoardDominos.Add(movedDomino);

            return true;
        }

        private Transform GetClosestTempPoint(Domino movedDomino)
        {
            List<float> distances = new List<float>(m_TempPoints.Count);

            for (int i = 0; i < m_TempPoints.Count; i++)
            {
                distances.Add(VectorUtilities.DistanceXY(movedDomino.transform.position, m_TempPoints[i].position));
            }

            float lowestDistance = float.MaxValue;
            int index = 0;

            for (int i = 0; i < distances.Count; i++)
            {
                if (lowestDistance > distances[i])
                {
                    lowestDistance = distances[i];
                    index = i;
                }
            }

            return m_TempPoints[index];
        }

        private void OnStartDrag(Domino domino)
        {
            selectedDomino = domino;
            Check(MoveDomino_FirstMove, MoveDomino_SecondMove, MoveDomino_OtherMoves);
            //throw new System.NotImplementedException();
        }

        private void OnEndDraging(Domino domino)
        {
            TryMoveDominoToTempPoint(domino);
            ClearTempPoints();
        }

        private void ClearTempPoints()
        {
            for (int i = 0; i < m_TempPoints.Count; i++)
            {
                m_TempPointPool.Release(m_TempPoints[i]);
            }

            m_TempPoints.Clear();
        }
    }
}