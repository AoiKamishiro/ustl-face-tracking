using System;
using System.Collections.Generic;

namespace USTL.FaceTracking.Editor
{
    public static class EnumUtility
    {
        public static List<T> GetAllElements<T>(bool includeNegative = false) where T : Enum
        {
            Array all = Enum.GetValues(typeof(T));
            List<T> tmp = new(all.Length);
            foreach (T item in all)
            {
                if (includeNegative || Convert.ToInt64(item) >= 0)
                {
                    tmp.Add(item);
                }
            }

            return tmp;
        }
    }
}
