using System;
using System.Collections.Generic;

namespace Util
{
public class TimeUtility
{
    // フレームを秒に変換する
    public static float ConvertFrameToSec(int frame)
    {
        return (float)frame / 60.0f;
    }
    public static float ConvertFrameToSec(double frame)
    {
        return (float)frame / 60.0f;
    }
}
}
