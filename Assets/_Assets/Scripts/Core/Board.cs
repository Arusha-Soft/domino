using Project.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace Project.Core
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private Line m_MiddleLine;
        [SerializeField] private TempDominoPoint m_TempPointPrefab;
        [SerializeField] private Transform m_TempPointParent;
        [SerializeField] private float m_MoveAcceptThresholdDistance = 5f;
        [SerializeField] private float m_SpaceBetweenDominos = 0.1f;
        [SerializeField] private int m_MaxLineDominoCount = 27;

        [SerializeField] private Domino[] m_AllDominos; // Todo make this list private and init on runtime

        private List<Domino> m_OnBoardDominos = new List<Domino>();
        private List<TempDominoPoint> m_TempPoints = new List<TempDominoPoint>();
        private ObjectPool<TempDominoPoint> m_TempPointPool;
        private int m_CurrentLineDominoCount = 0;
        private bool m_FirstLineIsFilled = false;
        private bool m_UpLineIsStarted = false;
        private bool m_DownLineIsStarted = false;
        private int OnBoardDominoCount => m_OnBoardDominos.Count;

        private void Awake()
        {
            m_TempPointPool = new ObjectPool<TempDominoPoint>(OnCreate, OnGet, OnRelease);
            Init(m_AllDominos);
        }

        private void OnRelease(TempDominoPoint dominoPoint)
        {
            dominoPoint.Clear();
            dominoPoint.gameObject.SetActive(false);
        }

        private void OnGet(TempDominoPoint dominoPoint)
        {
            dominoPoint.gameObject.SetActive(true);
        }

        private TempDominoPoint OnCreate()
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

        Domino m_SelectedDomino;
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
            List<int> points = new List<int>();
            TwoEndLineDominosData data = GetTwoEndLineDominos();
            AddPoints(points, data.Domino1);
            AddPoints(points, data.Domino2);

            for (int i = 0; i < points.Count; i++)
            {
                Debug.Log(points[i]);
            }
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

            static void AddPoints(List<int> points, Domino domino)
            {
                if (domino.IsFree(out bool freeFromLeft, out bool freeFromRight, out _, out _))
                {
                    if (freeFromLeft)
                    {
                        points.Add(domino.LeftValue);
                    }
                    if (freeFromRight)
                    {
                        points.Add(domino.RightValue);
                    }
                }
            }
        }

        #endregion

        #region MoveDomino
        private void MoveDomino_FirstMove()
        {
            TempDominoPoint tempPoint = GetTempPoint();
            tempPoint.transform.MakeVertical();
            Vector3 centerOfLine = m_MiddleLine.GetMiddle();
            tempPoint.transform.position = centerOfLine;
        }

        private void MoveDomino_SecondMove()
        {
            Domino lastDomino = m_OnBoardDominos[0];
            GenerateDominoPoint(lastDomino, m_SelectedDomino);
        }

        private void MoveDomino_OtherMoves()
        {
            TwoEndLineDominosData twoEndLineDominosData = GetTwoEndLineDominos();
            Domino domino1 = twoEndLineDominosData.Domino1;
            Domino domino2 = twoEndLineDominosData.Domino2;

            GenerateDominoPoint(domino1, m_SelectedDomino);
            GenerateDominoPoint(domino2, m_SelectedDomino);
        }

        private void GenerateDominoPoint(Domino onBoardDomino, Domino selectedDomino)
        {
            if (onBoardDomino.IsFree(out bool freeFromLeft, out bool freeFromRight, out _, out _))
            {
                if (freeFromLeft)
                {
                    Check(false);
                }

                if (freeFromRight)
                {
                    Check(true);
                }
            }

            void Check(bool isFreeFromRight)
            {
                Vector3 position = onBoardDomino.transform.position;
                Vector3 rotation = Vector3.zero;

                TempDominoPoint dominoPoint = GetTempPoint();

                position += Vector3.right * (isFreeFromRight ? 5 : -5);

                if (m_FirstLineIsFilled)
                {
                    if (m_UpLineIsStarted || m_DownLineIsStarted)
                    {
                        Debug.Log("Not implemented");
                    }
                    else
                    {
                        if (onBoardDomino.IsFree(out freeFromLeft, out freeFromRight, out _, out _))
                        {
                            if (freeFromLeft || freeFromRight)
                            {
                                if (selectedDomino.DownValue == onBoardDomino.DownValue ||
                                    selectedDomino.DownValue == onBoardDomino.UpValue)
                                {
                                    rotation = Vector3.up;
                                }
                                else if (selectedDomino.UpValue == onBoardDomino.DownValue ||
                                         selectedDomino.UpValue == onBoardDomino.UpValue)
                                {
                                    rotation = Vector3.down;
                                }
                                else
                                {
                                    ReleaseTempPoint(dominoPoint);
                                }
                            }

                            dominoPoint.transform.up = rotation;
                            position = CalculatePosition(dominoPoint, isFreeFromRight);
                            position.y += onBoardDomino.GetSpriteRenderer().bounds.size.y / 2f;
                        }
                    }
                }
                else
                {
                    if (selectedDomino.IsDouble())
                    {
                        if (selectedDomino.UpValue != onBoardDomino.UpValue &&
                            selectedDomino.UpValue != onBoardDomino.DownValue)
                        {
                            ReleaseTempPoint(dominoPoint);
                        }
                    }
                    else
                    {
                        if (onBoardDomino.IsDouble())
                        {
                            if (onBoardDomino.UpValue == selectedDomino.UpValue)
                            {
                                rotation = onBoardDomino.transform.position - position;
                            }
                            else if (onBoardDomino.UpValue == selectedDomino.DownValue)
                            {
                                rotation = position - onBoardDomino.transform.position;
                            }
                            else
                            {
                                ReleaseTempPoint(dominoPoint);
                            }
                        }
                        else
                        {
                            if (onBoardDomino.IsFree(out bool freeFromLeft, out bool freeFromRight, out _, out _))
                            {
                                if (freeFromLeft)
                                {
                                    if (onBoardDomino.LeftValue == selectedDomino.UpValue)
                                    {
                                        rotation = onBoardDomino.transform.position - position;
                                    }
                                    else if (onBoardDomino.LeftValue == selectedDomino.DownValue)
                                    {
                                        rotation = position - onBoardDomino.transform.position;
                                    }
                                    else
                                    {
                                        ReleaseTempPoint(dominoPoint);
                                    }
                                }

                                if (freeFromRight)
                                {
                                    if (onBoardDomino.RightValue == selectedDomino.UpValue)
                                    {
                                        rotation = onBoardDomino.transform.position - position;
                                    }
                                    else if (onBoardDomino.RightValue == selectedDomino.DownValue)
                                    {
                                        rotation = position - onBoardDomino.transform.position;
                                    }
                                    else
                                    {
                                        ReleaseTempPoint(dominoPoint);
                                    }
                                }
                            }
                        }
                    }

                    dominoPoint.transform.up = rotation;
                    position = CalculatePosition(dominoPoint, isFreeFromRight);
                }


                //if (isFreeFromRight)
                //{
                //    if (onBoardDomino.UpValue == selectedDomino.UpValue ||
                //        onBoardDomino.UpValue == selectedDomino.DownValue)
                //    {
                //        rotation = position - onBoardDomino.transform.position;
                //    }
                //    else
                //    {
                //        ReleaseTempPoint(dominoPoint);
                //    }
                //}
                //else
                //{
                //    if (onBoardDomino.DownValue == selectedDomino.UpValue ||
                //        onBoardDomino.DownValue == selectedDomino.DownValue)
                //    {
                //        rotation = onBoardDomino.transform.position - position;
                //    }
                //    else
                //    {
                //        ReleaseTempPoint(dominoPoint);
                //    }
                //}

                //if (onBoardDomino.UpValue == selectedDomino.UpValue ||
                //    onBoardDomino.DownValue == selectedDomino.UpValue)
                //{
                //    rotation = onBoardDomino.transform.position - position;
                //}
                //else if (onBoardDomino.UpValue == selectedDomino.DownValue ||
                //         onBoardDomino.DownValue == selectedDomino.DownValue)
                //{
                //    rotation = position - onBoardDomino.transform.position;
                //}
                //else
                //{
                //    ReleaseTempPoint(dominoPoint);
                //}

                //rotation = isFreeFromRight ? rotation : -rotation;

                dominoPoint.transform.position = position;
            }

            Vector3 CalculatePosition(TempDominoPoint dominoPoint, bool isRight)
            {
                Vector3 position;
                float selectedDominoBoundsX = dominoPoint.GetSpriteRenderer().bounds.size.x;
                float onBoardDominBoundsX = onBoardDomino.GetSpriteRenderer().bounds.size.x;
                float horizontalMoveAmount = ((selectedDominoBoundsX / 2f + onBoardDominBoundsX / 2f) + m_SpaceBetweenDominos) * (isRight ? 1f : -1f);
                position = onBoardDomino.transform.position;
                position.x += horizontalMoveAmount;
                return position;
            }
        }

        private List<int> GetAvailableDominoPoints()
        {
            List<int> points = new List<int>(4);
            Domino lastDomino = m_OnBoardDominos[m_OnBoardDominos.Count - 1];
            Domino oneToLastDomino = m_OnBoardDominos[m_OnBoardDominos.Count - 2];

            points.Add(lastDomino.DownValue);
            points.Add(lastDomino.UpValue);
            points.Add(oneToLastDomino.DownValue);
            points.Add(oneToLastDomino.UpValue);

            points.RemoveDuplicatesByCount();
            return points;
        }

        private TwoEndLineDominosData GetTwoEndLineDominos()
        {
            Domino domino1 = null;
            Domino domino2 = null;

            for (int i = 0; i < m_OnBoardDominos.Count; i++)
            {
                Domino domino = m_OnBoardDominos[i];
                if (domino.IsFree(out _, out _, out _, out _))
                {
                    if (domino1 == null)
                    {
                        domino1 = domino;
                    }
                    else if (domino2 == null)
                    {
                        domino2 = domino;
                    }
                    else
                    {
                        Debug.LogWarning("We have more then two free domino");
                    }
                }
            }

            return new TwoEndLineDominosData()
            {
                Domino1 = domino1,
                Domino2 = domino2
            };
        }

        private struct TwoEndLineDominosData
        {
            public Domino Domino1;
            public Domino Domino2;
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

        private TempDominoPoint GetTempPoint()
        {
            TempDominoPoint dominoPoint = m_TempPointPool.Get();
            m_TempPoints.Add(dominoPoint);
            return dominoPoint;
        }

        private void ReleaseTempPoint(TempDominoPoint tempPoint)
        {
            m_TempPoints.Remove(tempPoint);
            m_TempPointPool.Release(tempPoint);
        }

        private bool TryMoveDominoToTempPoint(Domino movedDomino)
        {
            TempDominoPoint closestTempPoint = GetClosestTempPoint(movedDomino);
            float distance = VectorUtilities.DistanceXY(movedDomino.transform.position, closestTempPoint.transform.position);

            if (distance > m_MoveAcceptThresholdDistance)
            {
                return false;
            }

            movedDomino.SetUpIsConnected(closestTempPoint.UpIsConnected);
            movedDomino.SetDownIsConnected(closestTempPoint.DownIsConnected);
            movedDomino.MoveTo(closestTempPoint.transform, () =>
            {
                m_CurrentLineDominoCount += movedDomino.IsVertical() ? 1 : 2;
                CheckLineFilled();
            });

            m_OnBoardDominos.Add(movedDomino);

            return true;
        }

        private void CheckLineFilled()
        {
            if (m_CurrentLineDominoCount >= m_MaxLineDominoCount)
            {
                m_FirstLineIsFilled = true;
                Debug.Log("Line filled");
            }
        }
        private TempDominoPoint GetClosestTempPoint(Domino movedDomino)
        {
            List<float> distances = new List<float>(m_TempPoints.Count);

            for (int i = 0; i < m_TempPoints.Count; i++)
            {
                distances.Add(VectorUtilities.DistanceXY(movedDomino.transform.position, m_TempPoints[i].transform.position));
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
            m_SelectedDomino = domino;
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