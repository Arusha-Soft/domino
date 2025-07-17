using Project.Core;
using UnityEngine;

namespace Project.Utilities
{
    public static class DominoUtilities
    {
        public static void MakeVertical(this Transform domino)
        {
            domino.rotation = Quaternion.identity;
        }

        public static void MakeHorizontal(this TempDominoPoint domino, bool isRightSide, int targetValue, Domino selectedDomino)
        {
            if (isRightSide)
            {
                if (targetValue == selectedDomino.DownValue)
                {
                    domino.transform.rotation = Quaternion.Euler(0, 0, -90f);
                    domino.SetDownIsConnected(true);
                }
                else
                {
                    domino.transform.rotation = Quaternion.Euler(0, 0, 90f);
                    domino.SetUpIsConnected(true);
                }
            }
            else
            {
                if (targetValue == selectedDomino.UpValue)
                {
                    domino.transform.rotation = Quaternion.Euler(0, 0, -90f);
                    domino.SetUpIsConnected(true);
                }
                else
                {
                    domino.transform.rotation = Quaternion.Euler(0, 0, 90f);
                    domino.SetDownIsConnected(true);
                }
            }
        }

        public static void MoveHorizontal(this Transform domino, bool isRightSide, Domino neighborDomino, float space)
        {
            var s = domino.GetSpriteRenderer().bounds.size;
            var w = neighborDomino.GetSpriteRenderer().bounds.size;
            float selectedDominoBoundsX = domino.GetSpriteRenderer().bounds.size.x;
            float neighborDominoBoundsX = neighborDomino.GetSpriteRenderer().bounds.size.x;
            float horizontalMoveAmount = ((selectedDominoBoundsX / 2f + neighborDominoBoundsX / 2f) + space) * (isRightSide ? 1f : -1f);

            Vector3 position = domino.transform.position;
            position.x = horizontalMoveAmount;
            domino.transform.position = position;
        }

        public static SpriteRenderer GetSpriteRenderer<T>(this T domino) where T : Component
        {
            return domino.GetComponent<SpriteRenderer>();
        }
    }
}