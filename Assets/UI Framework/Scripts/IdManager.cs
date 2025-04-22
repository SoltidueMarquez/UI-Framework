using System.Collections.Generic;
using System.Linq;

namespace UI_Framework.Scripts
{
    public static class IdManager
    {
        private static HashSet<int> availableIDs = new HashSet<int>(); // 存储回收的ID
        private static int currentID = 0;

        /// <summary>
        /// 获取一个唯一的ID
        /// </summary>
        public static int GetUniqueID()
        {
            if (availableIDs.Count > 0)
            {
                var id = availableIDs.OrderBy(x => x).First(); // 获取最小ID
                availableIDs.Remove(id); // 从回收池删除
                return id;
            }
            else
            {
                return ++currentID; // 生成新的ID
            }
        }

        /// <summary>
        /// 目前还只能自增，之后再想想id的回收吧
        /// </summary>
        /// <param name="id"></param>
        public static void ReCycleId(int id)
        {
            availableIDs.Add(id); // 回收ID
        }
    }
}