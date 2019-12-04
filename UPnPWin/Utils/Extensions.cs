using System.Collections;
using System.Collections.Generic;

namespace UPnPWin.Utils
{
    internal static class Extensions
    {
        internal static List<T> ToList<T>(this IList value)
        {
            List<T> returnValue = new List<T>(value.Count);
            foreach (var item in value)
                returnValue.Add((T)item);

            return returnValue;
        }
    }
}
