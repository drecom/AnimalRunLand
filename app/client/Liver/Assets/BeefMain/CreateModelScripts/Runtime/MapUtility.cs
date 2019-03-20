using UnityEngine;
using System.Collections;

namespace BeefMain.Runtime
{
public class MapUtility
{
    /// <summary>
    /// The world scale.
    /// </summary>
    public static Vector2 WorldScale = Vector2.one * 100000;

    /// <summary>
    /// 日本付近（北緯 35度）でのおよそ 1km の経緯度
    /// </summary>
    public static readonly Vector2 OneKilometerDegree = new Vector2(1.0f / 90, 1.0f / 90);

    /// <summary>
    /// 日本経緯度原点
    /// ## 経度：東経139度44分28秒8869
    /// ## 緯度：北緯 35度39分29秒1572
    /// </summary>
    public static readonly Vector2 NihonCenterPosition = new Vector2(
        new ArcDegree(139, 44, 28.8869f).decimalDegree(),
        new ArcDegree(35, 39, 29.1572f).decimalDegree());

    /// <summary>
    /// 度 (角度) を扱う。
    /// </summary>
    public class ArcDegree
    {
        public int degree;
        public int minute;
        public float second;

        public ArcDegree(int degree, int minute, float second)
        {
            this.degree = degree;
            this.minute = minute;
            this.second = second;
        }

        /// <summary>
        /// 小数点付き度を計算する。
        /// </summary>
        /// <returns>小数点付き度</returns>
        public float decimalDegree()
        {
            return degree +
                   (minute * 1.0f / 60) +
                   (second / 3600);
        }
    }
}

}