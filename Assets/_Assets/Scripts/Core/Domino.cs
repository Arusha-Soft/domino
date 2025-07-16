using AS.Project.Core;
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

        private DominoProperties m_Properties;
        private DominoStatus m_Status = DominoStatus.Active;
        private Coroutine m_Moving;

        public int UpValue { private set; get; }
        public int DownValue { private set; get; }

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

        public Domino MoveTo(Transform target)
        {
            if (m_Moving != null)
            {
                StopCoroutine(m_Moving);
            }

            m_Moving = StartCoroutine(Moving(target));

            return this;
        }

        public bool IsDouble() =>
            m_Properties.LeftPoint == m_Properties.RightPoint;

        private IEnumerator Moving(Transform target)
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

            m_Moving = null;
        }

        private void Update()
        {
            Debug.Log(m_Sprite.bounds.size);
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