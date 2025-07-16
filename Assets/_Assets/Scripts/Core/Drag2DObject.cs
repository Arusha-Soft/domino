using System;
using UnityEngine;

namespace Project.Core
{
    public class Drag2DObject : MonoBehaviour
    {
        public bool IsDragging = false;

        public event Action OnStartDrag;
        public event Action OnEndDrag;

        private Vector3 m_Offset;

        private void Update()
        {
            // Handle mouse
            if (Input.GetMouseButtonDown(0))
            {
                StartDrag(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndDrag();
            }
            else if (IsDragging && Input.GetMouseButton(0))
            {
                Drag(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }

            // Handle touch
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector3 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
                touchPos.z = 0f;

                if (touch.phase == TouchPhase.Began)
                {
                    StartDrag(touchPos);
                }
                else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    if (IsDragging)
                        Drag(touchPos);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    EndDrag();
                }
            }
        }

        private void StartDrag(Vector3 inputPos)
        {
            inputPos.z = 0f;
            RaycastHit2D hit = Physics2D.Raycast(inputPos, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == this.gameObject)
            {
                IsDragging = true;
                m_Offset = transform.position - inputPos;
                OnStartDraging();
            }
        }

        private void Drag(Vector3 inputPos)
        {
            inputPos.z = 0f;
            transform.position = inputPos + m_Offset;
        }

        private void EndDrag()
        {
            if (IsDragging)
            {
                IsDragging = false;
                OnEndDraging();
            }
        }

        private void OnStartDraging()
        {
            OnStartDrag?.Invoke();
        }

        private void OnEndDraging()
        {
            OnEndDrag?.Invoke();
        }
    }
}
