using System.Collections;
using UnityEngine;

namespace Project.Core
{
    public class Horizontal2dGroup : MonoBehaviour
    {
        [SerializeField] private float m_MoveDuration = 2f;
        [SerializeField] private float m_Spacing;

        [ContextMenu("Update")]
        public void ArrangeChildren()
        {
            float totalWidth = 0f;
            int childCount = transform.childCount;

            // First pass: calculate total width
            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();

                float width = 1f;

                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    width = spriteRenderer.sprite.bounds.size.x * child.localScale.x;
                }

                totalWidth += width;

                if (i < childCount - 1)
                    totalWidth += m_Spacing;
            }

            float startX = -totalWidth / 2f;
            float currentX = startX;

            StopAllCoroutines();

            // Second pass: move children
            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();

                float width = 1f;

                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    width = spriteRenderer.sprite.bounds.size.x * child.localScale.x;
                }

                Vector3 target = new Vector3(currentX + width / 2f, 0f, 0f);
                StartCoroutine(Move(child, target));

                currentX += width + m_Spacing;
            }
        }

        public void Stop()
        {
            StopAllCoroutines();
        }

        public Horizontal2dGroup SetSpacing(float spacing)
        {
            m_Spacing = spacing;
            return this;
        }

        private IEnumerator Move(Transform targetTransform, Vector3 targetLocalPosition)
        {
            if (targetTransform != null)
            {
                float elapsed = 0f;
                while (targetTransform.localPosition != targetLocalPosition)
                {
                    elapsed += Time.deltaTime / m_MoveDuration;
                    targetTransform.localPosition = Vector3.Lerp(targetTransform.localPosition, targetLocalPosition, elapsed);
                    targetTransform.rotation = Quaternion.Lerp(targetTransform.rotation, Quaternion.identity, elapsed);
                    yield return null;
                }
            }
        }
    }
}