using System;
using System.Linq;
using System.Collections.Generic;

namespace Util.Extension
{
public static class ExtendedIEnumerable
{
    // alternative First() method
    // If happen error, this function return `null` instead exception.
    public static T FindFirstOptional<T>(this IEnumerable<T> e, Predicate<T> pred)
    where T : class
    {
        foreach (T x in e)
        {
            if (pred(x))
            {
                return x;
            }
        }

        return null;
    }

    /// <summary>
    /// 目的の値に最も近い値を返します
    /// </summary>
    public static int Nearest(this IEnumerable<int> self, int target)
    {
        var min = self.Min(c => Math.Abs(c - target));
        return self.First(c => Math.Abs(c - target) == min);
    }
    /// <summary>
    /// 目的の値に最も近い値を返します
    /// </summary>
    public static float Nearest(this IEnumerable<float> self, float target)
    {
        var min = self.Min(c => Math.Abs(c - target));
        return self.First(c => Math.Abs(c - target) == min);
    }
}
}
