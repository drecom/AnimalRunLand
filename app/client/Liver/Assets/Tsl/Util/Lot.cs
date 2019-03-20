using System;
using System.Collections.Generic;
using System.Linq;
namespace Util
{
public class Lot
{
    // sequence の中から重み付きでランダムに要素を選択する関数
    // 関数名の Draw は「くじを引く」という意味の「draw lots」から。
    //
    // 重み付きであるため、例えば Lot.Draw(new int[] { 10, 90 }) とした場合には、10%の確率で0が、90%の確率で1が返される。
    // Lot.Draw(new int[] { 1, 9 }) とした場合も同様である。
    //
    // 重みのデータは keySelector(x) によって取得する。
    // keySelector が指定されていない場合は x => x と同じ意味になる。
    //
    // sequence が空の場合は例外が発生する。
    // 戻り値は要素のインデックス。
    public static int Draw(IEnumerable<int> sequence)
    {
        return Draw(sequence, x => x, (a, b) => UnityEngine.Random.Range(a, b + 1));
    }
    public static int Draw<T>(IEnumerable<T> sequence, Func<T, int> keySelector, Func<int, int, int> randomRange)
    {
        var ws = sequence.Select(keySelector).ToArray();

        if (ws.Length == 0)
        {
            throw new Exception("要素数が0");
        }

        var s = ws.Sum();

        if (s == 0)
        {
            throw new Exception("重みの合計が0");
        }

        var n = 0;
        var r = randomRange(0, s - 1);

        for (int i = 0; i < ws.Length; i++)
        {
            var w = ws[i];
            n += w;

            if (r < n)
            {
                return i;
            }
        }

        throw new Exception("ここに来たらバグ");
    }
}
}
