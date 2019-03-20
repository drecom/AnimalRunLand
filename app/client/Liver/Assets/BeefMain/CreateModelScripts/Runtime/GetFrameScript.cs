using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeefCreateModelScripts.Runtime
{
public class GetFrameScript
{

    public static IEnumerable<Vector2> GetFramePoints(List<Vector2> points)
    {
        return new GetFrameScript(points).GetFramePoints();
    }

    private Vector2[] points;
    private GetFrameScript(List<Vector2> points)
    {
        this.points = points.ToArray();
    }

    // 枠を形成するポイントを求める
    private IEnumerable<Vector2> GetFramePoints()
    {
        // 最も左側のポイントを得る
        var left = points
                   .Where(p => p.x == points.Min(p2 => p2.x))
                   .First();
        // 最も右側のポイントを得る
        var right = points
                    .Where(p => p.x == points.Max(p2 => p2.x))
                    .First();
        // 下半分を求める
        var q1 = GetFramePointsHalf(left, right, (q, t) => q.Max(p => Gradient(t, p)));
        // 上半分を求める
        var q2 = GetFramePointsHalf(left, right, (q, t) => q.Min(p => Gradient(t, p)));
        return q1.Concat(q2.Reverse());
    }

    // 2点の傾き
    private float Gradient(Vector2 p1, Vector2 p2)
    {
        return ((p2.y - p1.y) / (p2.x - p1.x));
    }

    // 上(下)半分を求める
    private IEnumerable<Vector2> GetFramePointsHalf(Vector2 now, Vector2 right,
            System.Func<IEnumerable<Vector2>, Vector2, float> maxGradient)
    {
        yield return now;

        while (now != right)
        {
            // 現地点より右側にあるポイントを得る
            var query = points
                        .Where(p => p != now)
                        .Where(p => p.x >= now.x);
            // 右側にあるポイントの中で最大傾斜角をもつポイントを求める
            var next = query
                       .Where(p => Gradient(now, p) == maxGradient(query, now)).First();
            yield return next;
            now = next;
        }
    }
}
}
