using AS.Project.Core;
using Project.Utilities;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Project.Core
{
    public class Domino : NetworkBehaviour
    {
        [SerializeField] private DominoProperties m_Default;//TODO remove this
        [SerializeField] private float m_MoveDuration = 1;
        [SerializeField] private SpriteRenderer m_Sprite;
        [SerializeField] private Drag2DObject m_Drag2DObject;
        [SerializeField] private GameObject m_Blocker;
        [SerializeField] private LayerMask m_DominoLayer;

        private DominoProperties m_Properties;
        private DominoStatus m_Status = DominoStatus.Active;
        private Coroutine m_Moving;

        ///for vertical dominos up is right and down is left
        public int UpValue { private set; get; }
        public int DownValue { private set; get; }
        public int RightValue
        {
            get
            {
                if (GetSignedAngle() > 0)
                {
                    return DownValue;
                }
                else if (GetSignedAngle() < 0)
                {
                    return UpValue;
                }
                else
                {
                    return UpValue;
                }
            }
        }

        public int LeftValue
        {
            get
            {
                if (GetSignedAngle() > 0)
                {
                    return UpValue;
                }
                else if (GetSignedAngle() < 0)
                {
                    return DownValue;
                }
                else
                {
                    return DownValue;
                }
            }
        }

        float GetSignedAngle()
        {
            float angle = transform.rotation.eulerAngles.z;
            angle = angle % 360;
            if (angle > 180)
                angle -= 360;
            return angle;
        }

        public bool UpIsConnected { private set; get; } = false;
        public bool DownIsConnected { private set; get; } = false;
        public event Action<Domino> OnStartDrag;
        public event Action<Domino> OnEndDrag;

        private void Awake() // TODO remove this
        {
            Init(m_Default);
        }

        public void Init(DominoProperties properties)
        {
            m_Properties = properties;
            SetSprite(properties.Icon);
            SetStatus(m_Status);
            UpValue = properties.RightPoint;
            DownValue = properties.LeftPoint;

            m_Drag2DObject.OnStartDrag += OnStartDraging;
            m_Drag2DObject.OnEndDrag += OnEndDraging;
        }

        public Domino SetSprite(Sprite sprite)
        {
            m_Sprite.sprite = sprite;
            return this;
        }

        public Domino SetStatus(DominoStatus status)
        {
            m_Status = status;

            switch (m_Status)
            {
                case DominoStatus.Active:
                    //Debug.Log($"Domino {m_Properties.name} actived");
                    m_Drag2DObject.enabled = true;
                    m_Blocker.SetActive(false);
                    break;
                case DominoStatus.Deactive:
                    //Debug.Log($"Domino {m_Properties.name} deactived");
                    m_Drag2DObject.enabled = false;
                    m_Blocker.SetActive(true);
                    break;
            }

            return this;
        }

        public Domino MoveTo(Transform target, Action onFinish = null)
        {
            if (m_Moving != null)
            {
                StopCoroutine(m_Moving);
            }

            m_Moving = StartCoroutine(Moving(target, onFinish));

            return this;
        }

        public Domino SetUpIsConnected(bool isConnected)
        {
            UpIsConnected = isConnected;
            return this;
        }

        public Domino SetDownIsConnected(bool isConnected)
        {
            DownIsConnected = isConnected;
            return this;
        }

        public bool IsFree(out bool freeFromLeft, out bool freeFromRight, out bool freeFromUp, out bool freeFromDown)
        {
            bool result = false;
            freeFromUp = false;
            freeFromDown = false;
            freeFromRight = false;
            freeFromLeft = false;

            //if (IsVertical())
            //{
            Debug.DrawRay(transform.position, Vector3.right * 100, Color.red, 5);
            Debug.DrawRay(transform.position, Vector3.left * 100, Color.red, 5);

            RaycastHit2D[] rightHitData = Physics2D.RaycastAll(transform.position, Vector3.right, float.MaxValue, m_DominoLayer);
            RaycastHit2D[] leftHitData = Physics2D.RaycastAll(transform.position, Vector3.left, float.MaxValue, m_DominoLayer);

            for (int i = 0; i < rightHitData.Length; i++)
            {
                if (rightHitData[i].collider.gameObject == this.gameObject)
                {
                    freeFromRight = true;
                    continue;
                }

                freeFromRight = rightHitData[i].collider == null;

                if (freeFromRight)
                {
                    break;
                }
            }

            for (int i = 0; i < leftHitData.Length; i++)
            {
                if (leftHitData[i].collider.gameObject == this.gameObject)
                {
                    freeFromLeft = true;
                    continue;
                }

                freeFromLeft = leftHitData[i].collider == null;

                if (freeFromLeft)
                {
                    break;
                }
            }

            result = freeFromRight || freeFromLeft;
            //}
            //else
            //{
            //    Debug.DrawRay(transform.position, Vector3.up * 100, Color.red, 5);
            //    Debug.DrawRay(transform.position, Vector3.down * 100, Color.red, 5);

            //    RaycastHit2D[] upHitData = Physics2D.RaycastAll(transform.position, Vector3.up, float.MaxValue, m_DominoLayer);
            //    RaycastHit2D[] downHitData = Physics2D.RaycastAll(transform.position, Vector3.down, float.MaxValue, m_DominoLayer);

            //    for (int i = 0; i < upHitData.Length; i++)
            //    {
            //        if (upHitData[i].collider.gameObject == this.gameObject)
            //        {
            //            freeFromUp = true;
            //            continue;
            //        }

            //        freeFromUp = upHitData[i].collider == null;

            //        if (freeFromUp)
            //        {
            //            break;
            //        }

            //    }

            //    for (int i = 0; i < downHitData.Length; i++)
            //    {
            //        if (downHitData[i].collider.gameObject == this.gameObject)
            //        {
            //            freeFromDown = true;
            //            continue;
            //        }

            //        freeFromDown = downHitData[i].collider == null;

            //        if (freeFromDown)
            //        {
            //            break;
            //        }
            //    }

            //    result = freeFromUp || freeFromDown;
            //}

            return result;
        }

        public bool IsVertical() =>
            transform.rotation.eulerAngles.z == 0;

        public bool IsDouble() =>
            m_Properties.LeftPoint == m_Properties.RightPoint;

        private IEnumerator Moving(Transform target, Action onFinish)
        {
            if (target != null)
            {
                float elapsed = 0f;
                while (transform.position != target.position)
                {
                    elapsed += Time.deltaTime / m_MoveDuration;
                    transform.position = Vector3.Lerp(transform.position, target.position, elapsed);
                    transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, elapsed);
                    yield return null;
                }
            }

            onFinish?.Invoke();
            m_Moving = null;
        }

        [ContextMenu("A")]
        private void Updatea()
        {
            Debug.Log($"Domino: {gameObject.name} IsFree: {IsFree(out bool FreeFromLeft, out bool freeFromRight, out bool freeFromUp, out bool FreeFromDown)} " +
                $"Free From Up: {freeFromUp} / Free From Down: {FreeFromDown} / Free From Left: {FreeFromLeft} / Free From Right: {freeFromRight}" +
                $"Size: {this.GetSpriteRenderer().size}");
        }
        private void OnStartDraging()
        {
            OnStartDrag?.Invoke(this);
        }

        private void OnEndDraging()
        {
            OnEndDrag?.Invoke(this);
        }
    }

    public enum DominoStatus
    {
        Active,
        Deactive
    }
}