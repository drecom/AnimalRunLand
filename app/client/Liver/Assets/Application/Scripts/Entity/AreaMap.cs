using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Liver.Entity
{
// 画像を保持するためのクラス
public class AreaMap
{
    int width;
    int height;

    Texture2D texture = null;

    public AreaMap(Texture2D texture)
    {
        this.width  = texture.width;
        this.height = texture.height;
        this.texture = texture;
    }


    public bool Contain(int x, int y)
    {
        return (x >= 0) && (y >= 0) && (x < this.width) && (y < this.height);
    }

    public Color GetPixel(int x, int y)
    {
        return texture.GetPixel(x, y);
    }
}

}
