using System.Collections.Generic;
using System.Linq;

namespace Project.Utilities
{
    public static class ListUtilities
    {
        public static void RemoveDuplicatesByCount(this List<int> list, int countToRemove = 2)
        {
            var groups = list.GroupBy(x => x).ToList();

            foreach (var group in groups)
            {
                int removeCount = (group.Count() / countToRemove) * countToRemove;

                for (int i = 0; i < removeCount; i++)
                {
                    list.Remove(group.Key);
                }
            }
        }
    }
}