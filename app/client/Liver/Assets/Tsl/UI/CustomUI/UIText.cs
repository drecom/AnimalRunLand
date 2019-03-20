using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace CustomUI
{
/// <summary>
/// Labels are graphics that display text.
/// </summary>
public class UIText : MaskableGraphic, ILayoutElement
{
    private class TextLineData
    {
        public int totalWidth;
        public List<CharacterInfo> charactersInfo;
        public bool justfit;    // 禁則処理で入った時に均等割り付けをする

        public TextLineData()
        {
            totalWidth = 0;
            charactersInfo = new List<CharacterInfo>();
            justfit = false;
        }
    }

    private class TextData
    {
        public List<TextLineData> lineData;
        public int maxWidth;
        public float totalHeight;

        public TextData()
        {
            lineData = new List<TextLineData>();
            maxWidth = 0;
            totalHeight = 0.0f;
        }
    }

    public class Layout
    {
        public enum Horizontal
        {
            Overflow,
            Warp
        }

        public enum Vertical
        {
            Overflow,
            Truncate
        }
    }

    public class Alignment
    {
        public enum Vertical
        {
            Top,
            Middle,
            Bottom
        }

        public enum Horizontal
        {
            Left,
            Center,
            Right
        }
    }

    // 表示方法の指定
    public enum DisplayType
    {
        Flush,      // 全て
        Teletype    // 1文字づつ
    }

    // private FontData
    [SerializeField]
    private Font font = null;

    [SerializeField]
    private FontStyle style = FontStyle.Normal;

    [SerializeField]
    private int fontSize = 10;

    [TextArea(3, 10), SerializeField]
    private string text = String.Empty;

    [SerializeField]
    private Alignment.Vertical verticalAlignment = Alignment.Vertical.Top;

    [SerializeField]
    private Alignment.Horizontal horizontalAlignment = Alignment.Horizontal.Left;

    [SerializeField]
    private Layout.Horizontal horizontalLayout = Layout.Horizontal.Overflow;

    [SerializeField]
    private Layout.Vertical verticalLayout = Layout.Vertical.Overflow;

    [SerializeField]
    private float lineSpaceing = 0.0f;

    [SerializeField]
    private bool justfit = false;

    [SerializeField]
    private float fitEnabledCharacterInterval = 2.0f;

    [SerializeField]
    private int alphabetWarpLimit = 8;

    // 表示タイプの指定
    [SerializeField]
    private DisplayType displayType = DisplayType.Flush;

    // 一括表示のロジック
    [SerializeField]
    private FlushText flushText = new FlushText();

    [SerializeField]
    private TeletypeText teletypeText = new TeletypeText();

    // 現在の表示タイプのロジック
    // flushTextなどTextRangeを継承しているクラスを参照するためだけの変数
    private TextRange textRange = null;

    private int totalCharacterCount = 0;
    ///

    // [SerializeField]
    // private int characterSpaceing = 1;
    [NonSerialized]
    public float TotalHeight = 0; // テキストエリアの高さ

    // 禁則処理 http://ja.wikipedia.org/wiki/%E7%A6%81%E5%89%87%E5%87%A6%E7%90%86
    // 行頭禁則文字
    private readonly static char[] HyphenationFront = (
                ",)]｝、。）〕〉》」』】〙〗〟’”｠»" +// 終わり括弧類 簡易版
                "ァィゥェォッャュョヮヵヶっぁぃぅぇぉっゃゅょゎ" +//行頭禁則和字
                "‐゠–〜ー" +//ハイフン類
                "?!！？‼⁇⁈⁉" +//区切り約物
                "・:;" +//中点類
                "。.").ToCharArray();//句点類

    private readonly static char[] HyphenationBack = "(（[｛〔〈《「『【〘〖〝‘“｟«".ToCharArray();//始め括弧類

    private readonly static char[] Numbers = "0123456789０１２３４５６７８９".ToCharArray();
    private readonly static char[] Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    /**** フィールド設定のプロパティ ********/

    public override Texture mainTexture
    {
        get
        {
            if (font != null && font.material != null && font.material.mainTexture != null)
            {
                return font.material.mainTexture;
            }

            return base.mainTexture;
        }
    }

    public Font Font
    {
        set
        {
            font = value;
            SetVerticesDirty();
        }

        get
        {
            return font;
        }
    }

    public FontStyle Style
    {
        set
        {
            style = value;
            SetVerticesDirty();
        }

        get
        {
            return style;
        }
    }

    public int FontSize
    {
        set
        {
            fontSize = value;
            SetVerticesDirty();
        }

        get
        {
            return fontSize;
        }
    }

    public string Text
    {
        set
        {
            text = value;
            totalCharacterCount = text.Replace("\n", "").Length;

            if (textRange != null) { textRange.Reset(); }

            SetVerticesDirty();
        }

        get
        {
            return text;
        }
    }
    public void SetText(string text)
    {
        Text = text;
    }

    public Alignment.Vertical VarticalAlignment
    {
        set
        {
            verticalAlignment = value;
            SetLayoutDirty();
        }

        get
        {
            return verticalAlignment;
        }
    }

    public Alignment.Horizontal HorizontalAlignment
    {
        set
        {
            horizontalAlignment = value;
            SetLayoutDirty();
        }

        get
        {
            return horizontalAlignment;
        }
    }

    public Layout.Vertical VerticalLayout
    {
        set
        {
            verticalLayout = value;
            SetLayoutDirty();
        }

        get
        {
            return verticalLayout;
        }
    }

    public Layout.Horizontal HorizontalLayout
    {
        set
        {
            horizontalLayout = value;
            SetLayoutDirty();
        }

        get
        {
            return horizontalLayout;
        }
    }

    public float LineSpaceing
    {
        set
        {
            lineSpaceing = value;
            SetLayoutDirty();
        }

        get
        {
            return lineSpaceing;
        }
    }

    public bool Justfit
    {
        set
        {
            justfit = value;
            SetLayoutDirty();
        }

        get
        {
            return justfit;
        }
    }

    private float FitEnabledCharacterInterval
    {
        set
        {
            fitEnabledCharacterInterval = value;
        }

        get
        {
            return fitEnabledCharacterInterval;
        }
    }

    public DisplayType DisplayMode
    {
        set
        {
            displayType = value;
        }

        get
        {
            return displayType;
        }
    }

    public float TeletypeSpeed
    {
        set
        {
            teletypeText.Speed = value;
        }

        get
        {
            return teletypeText.Speed;
        }
    }

    public UnityEvent OnTeletypeFinished
    {
        get
        {
            return teletypeText.OnFinished;
        }
    }

    /***************/

    public float pixelsPerUnit
    {
        get
        {
            var localCanvas = canvas;

            if (!localCanvas)
            {
                return 1;
            }

            // For dynamic fonts, ensure we use one pixel per pixel on the screen.
            if (!font || font.dynamic)
            {
                return localCanvas.scaleFactor;
            }

            // For non-dynamic fonts, calculate pixels per unit based on specified font size relative to font object's own font size.
            if (fontSize <= 0 || font.fontSize <= 0)
            {
                return 1;
            }

            return font.fontSize / (float)fontSize;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        useLegacyMeshGeneration = false;
        UpdateDisplayMode(displayType);
        totalCharacterCount = text.Replace("\n", "").Length;
    }

    protected void Update()
    {
        if (IsPlaying && totalCharacterCount > 0 && textRange.IsUpdate(totalCharacterCount))
        {
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
        {
            return;
        }

        int maxHeight = GetMaxHeight();;
        Rect rect = rectTransform.rect;
        Vector2 anchor = Anchor();
        Vector2 refPoint = Vector2.zero;
        refPoint.x = (anchor.x == 1 ? rect.xMax : rect.xMin);
        refPoint.y = (anchor.y == 0 ? rect.yMin : rect.yMax);
        Vector2 roundingOffset = PixelAdjustPoint(refPoint) - refPoint;
        IList<UIVertex> verts = CreateVertices(rect, anchor, maxHeight);
        int vertCount = verts.Count;
        UIVertex[] tempVerts = new UIVertex[4];
        // float unitsPerPixel = 1 / pixelsPerUnit;
        toFill.Clear();

        for (int i = 0; i < vertCount; ++i)
        {
            int tempVertsIndex = i & 3;
            tempVerts[tempVertsIndex] = verts[i];
            // tempVerts[tempVertsIndex].position *= unitsPerPixel;

            if (roundingOffset != Vector2.zero)
            {
                tempVerts[tempVertsIndex].position.x += roundingOffset.x;
                tempVerts[tempVertsIndex].position.y -= roundingOffset.y;
            }

            if (tempVertsIndex == 3)
            {
                toFill.AddUIVertexQuad(tempVerts);
            }
        }
    }
    /**** 頂点データの生成  *********************/

    private IList<UIVertex> CreateVertices(Rect visibleRect, Vector2 anchor, int maxHeight)
    {
        string[] lineText = text.Split('\n');
        IList<UIVertex> vertices = new List<UIVertex>();
        TextData textData = new TextData();
        font.RequestCharactersInTexture(text, fontSize, style);

        // 禁則や幅情報から表示内容を整える
        for (int i = 0; i < lineText.Length; ++i)
        {
            if (!CreateLineTextInfo(textData, lineText[i], visibleRect, maxHeight))
            {
                break;
            }
        }

        // 自動サイズ調整のため、テキストの高さを記録しておく
        this.TotalHeight = textData.totalHeight;
        Vector2 visibleSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        float topPoint = (visibleSize.y * -(rectTransform.pivot.y - 0.5f)) + (-visibleSize.y * (anchor.y - 0.5f)) + (anchor.y * textData.totalHeight);
        int descent = font.lineHeight - font.ascent;
        int displayCharacterCount = 0;

        // 整えた情報から頂点データを生成する
        for (int i = 0; i < textData.lineData.Count; ++i)
        {
            int lineCharacterCount = textData.lineData[i].charactersInfo.Count;
            float fittingCharacterInterval = 0.0f;

            if ((justfit || textData.lineData[i].justfit) && lineCharacterCount > 1)
            {
                // 均等割付をする場合の行辺りの余白
                fittingCharacterInterval = (visibleRect.width - textData.lineData[i].totalWidth) / (lineCharacterCount - 1);

                if (fittingCharacterInterval > fitEnabledCharacterInterval)
                {
                    fittingCharacterInterval = 0.0f;
                }
            }

            // 0 : +0.5
            // 0.5 : 0
            // 1 : -0.5
            float leftPoint = (visibleSize.x * -(rectTransform.pivot.x - 0.5f)) + visibleSize.x * (anchor.x - 0.5f);
            float offsetX = -(textData.lineData[i].totalWidth * anchor.x) - (fittingCharacterInterval * (lineCharacterCount - 1)) * anchor.x;
            // topPoint -= (textData.lineData[i].maxHeight + (lineSpaceing * ((i == 0) ? 0 : 1)));
            topPoint -= maxHeight;

            for (int j = 0; j < lineCharacterCount; ++j)
            {
                int proportionAdjustValue = textData.lineData[i].charactersInfo[j].advance - textData.lineData[i].charactersInfo[j].glyphWidth;
                proportionAdjustValue = (proportionAdjustValue == 0 ? 0 : proportionAdjustValue / 2);
                Rect rect = new Rect();
                rect.x = leftPoint + offsetX + proportionAdjustValue;
                rect.y = topPoint + descent + textData.lineData[i].charactersInfo[j].minY;
                rect.width = textData.lineData[i].charactersInfo[j].glyphWidth;
                rect.height = textData.lineData[i].charactersInfo[j].glyphHeight;
                GenerateVertices(vertices, ref rect, textData.lineData[i].charactersInfo[j], color);
                leftPoint += (textData.lineData[i].charactersInfo[j].advance + fittingCharacterInterval);
                displayCharacterCount += 1;

                if (IsPlaying && displayCharacterCount >= textRange.Length(totalCharacterCount))
                {
                    return vertices;
                }
            }

            topPoint -= lineSpaceing;
        }

        return vertices;
    }

    // 1行辺りのテキストの分割とマッピング情報を取得
    private bool CreateLineTextInfo(TextData textData, string lineText, Rect visibleRect, int maxHeigth)
    {
        TextLineData lineData = new TextLineData();
        float totalHeight = textData.totalHeight;
        int continuePoint = -1;

        for (int i = 0; i < lineText.Length; ++i)
        {
            if (!font.HasCharacter(lineText[i]))
            {
                continue;
            }

            CharacterInfo characterInfo;

            if (font.GetCharacterInfo(lineText[i], out characterInfo, fontSize, style))
            {
                // 行末禁則判定のため次の文字に関しての
                // チェックをこのタイミングで行う
                if (IsHyphenationBack(lineText[i]) && i + 1 < lineText.Length)
                {
                    // Debug.Log("word : " + lineText[i]);
                    CharacterInfo nextCharacterInfo;
                    font.GetCharacterInfo(lineText[i + 1], out nextCharacterInfo, fontSize, style);

                    if (IsNewLine(lineData.totalWidth + characterInfo.advance, nextCharacterInfo.advance, visibleRect.width))
                    {
                        // Debug.Log("next word : " + lineText[i + 1]);
                        continuePoint = i;
                        lineData.justfit = true;
                        break;
                    }
                }

                // warpは自動改行なので
                // ここで横の長さがrectの大きさを超えてしまうかチェック
                if (IsNewLine(lineData.totalWidth, characterInfo.advance, visibleRect.width))
                {
                    if (!IsHyphenationFront(lineText[i]))
                    {
                        continuePoint = i;
                        break;
                    }
                    else
                    {
                        // 行頭の禁則文字じゃない場合この行に含めて
                        // 均等割り付けにする
                        lineData.justfit = true;
                    }
                }
                else if (horizontalLayout == Layout.Horizontal.Warp)
                {
                    Func<char[], int, bool> serialWords = (words, warpLimitCount) =>
                    {
                        if (Exists(words, lineText[i]))
                        {
                            int characterCount = 0;
                            bool inline = IsSerialWordCurrentLine(out characterCount, words, warpLimitCount, lineData.totalWidth + characterInfo.advance, lineText, i, visibleRect.width);

                            // 連続した数値の個数が-1の場合次の行にして表示する
                            if (!inline)
                            {
                                continuePoint = i;
                                lineData.justfit = true;
                                return false;
                            }

                            for (int j = 0; j < characterCount; ++j)
                            {
                                lineData.totalWidth += characterInfo.advance;
                                lineData.charactersInfo.Add(characterInfo);
                                font.GetCharacterInfo(lineText[i + j + 1], out characterInfo, fontSize, style);
                            }

                            i += characterCount;

                            // 連続した数字がこの行に入り切らない場合均等割付を有効にして収めるようにする
                            if (lineData.totalWidth + characterInfo.advance > visibleRect.width)
                            {
                                lineData.justfit = true;
                            }
                        }

                        return true;
                    };

                    // 数字が連続する時に自動改行をしてほしくないので
                    // 連続した数字が存在して、自動改行されるか判定をして
                    // 改行される場合開始地点で開業するように
                    if (!serialWords(Numbers, 0))
                    {
                        break;
                    }

                    // アルファベットが連続する時に自動改行を変な位置で自動改行しないため
                    if (!serialWords(Alphabet, alphabetWarpLimit))
                    {
                        break;
                    }
                }

                lineData.totalWidth += characterInfo.advance;
                lineData.charactersInfo.Add(characterInfo);
                // Debug.Log("glyphWidth : " + characterInfo.glyphWidth);
                // Debug.Log("glyphHeight : " + characterInfo.glyphHeight );
                // Debug.Log("size : " + characterInfo.size);
                // Debug.Log("maxX : " + characterInfo.maxX);
                // Debug.Log("maxX : " + characterInfo.minX);
                // Debug.Log("minY : " + characterInfo.minY);
                // Debug.Log("maxY : " + characterInfo.maxY);
                // Debug.Log("advance : " + characterInfo.advance);
                // Debug.Log("bearing : " + characterInfo.bearing);
            }
        }

        // テキスト全体の最大の高さを保持しておく
        totalHeight += maxHeigth;

        // 合計の高さがrectを超える場合表示しない
        if (verticalLayout == Layout.Vertical.Truncate && visibleRect.height < totalHeight)
        {
            return false;
        }

        totalHeight += lineSpaceing;
        textData.totalHeight = totalHeight;
        textData.lineData.Add(lineData);

        if (continuePoint != -1)
        {
            string restString = lineText.Substring(continuePoint);
            return CreateLineTextInfo(textData, restString, visibleRect, maxHeigth);
        }

        return true;
    }

    // 現在の行に数字を含めるか判定
    private bool IsSerialWordCurrentLine(out int characterCount, char[] words, int warpLimitCount, int lineTotalWidth, string text, int startIndex, float visibleWidth)
    {
        bool result = true;
        characterCount = 0;
        CharacterInfo characterInfo;
        int characterTotalWidth = 0;

        while ((startIndex + characterCount + 1) < text.Length)
        {
            if (!Exists(words, text[startIndex + characterCount + 1]))
            {
                break;
            }

            font.GetCharacterInfo(text[startIndex + characterCount], out characterInfo, fontSize, style);

            // 連続した数字の途中で自動改行になる場合この数字は次の行で表示するように
            // ただ、頭の文字が数字でこの行に入らない場合は永久に表示されないことになるので
            // この行の横幅を超えても表示をしてしまう
            if (IsNewLine(lineTotalWidth + characterTotalWidth, characterInfo.advance, visibleWidth) && startIndex != 0)
            {
                // 連続した数値の途中で改行が入る場合
                // 最初の数値の地点で開業するので
                // 戻り値は-1にする
                result = false;
                break;
            }

            if (warpLimitCount != 0 && characterCount + 1 >= warpLimitCount)
            {
                break;
            }

            characterTotalWidth += characterInfo.advance;
            characterCount += 1;
        }

        return result;
    }

    // 頂点データを追加する
    private void GenerateVertices(IList<UIVertex> vertices, ref Rect rect, CharacterInfo characterInfo, Color color)
    {
        UIVertex vertex = new UIVertex();
        vertex.position = new Vector3(rect.xMin, rect.yMax);
        vertex.uv0 = new Vector2(characterInfo.uvTopLeft.x, characterInfo.uvTopLeft.y);
        vertex.tangent = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
        vertex.normal = new Vector3(0.0f, 0.0f, -1.0f);
        vertex.color = color;
        vertices.Add(vertex);
        vertex = new UIVertex();
        vertex.position = new Vector3(rect.xMax, rect.yMax);
        vertex.uv0 = new Vector2(characterInfo.uvTopRight.x, characterInfo.uvTopRight.y);
        vertex.tangent = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
        vertex.normal = new Vector3(0.0f, 0.0f, -1.0f);
        vertex.color = color;
        vertices.Add(vertex);
        vertex = new UIVertex();
        vertex.position = new Vector3(rect.xMax, rect.yMin);
        vertex.uv0 = new Vector2(characterInfo.uvBottomRight.x, characterInfo.uvBottomRight.y);
        vertex.tangent = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
        vertex.normal = new Vector3(0.0f, 0.0f, -1.0f);
        vertex.color = color;
        vertices.Add(vertex);
        vertex = new UIVertex();
        vertex.position = new Vector3(rect.xMin, rect.yMin);
        vertex.uv0 = new Vector2(characterInfo.uvBottomLeft.x, characterInfo.uvBottomLeft.y);
        vertex.tangent = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
        vertex.normal = new Vector3(0.0f, 0.0f, -1.0f);
        vertex.color = color;
        vertices.Add(vertex);
    }

    /**** 表示方法(DisplayMode)に関する処理 ********/
    private void UpdateDisplayMode(DisplayType type)
    {
        displayType = type;

        switch (displayType)
        {
            case DisplayType.Flush:
                textRange = flushText;
                break;

            case DisplayType.Teletype:
                textRange = teletypeText;
                break;

            default:
                Debug.Assert(false, "unknown type");
                break;
        }
    }


    /**** ILayoutElementのオーバーライド ********/
    public virtual void CalculateLayoutInputHorizontal() {}
    public virtual void CalculateLayoutInputVertical() {}

    public virtual float minWidth
    {
        get { return 0; }
    }

    public virtual float preferredWidth
    {
        get
        {
            return 0.0f;
            // var settings = GetGenerationSettings(Vector2.zero);
            // return cachedTextGeneratorForLayout.GetPreferredWidth(m_Text, settings) / pixelsPerUnit;
        }
    }

    public virtual float flexibleWidth { get { return -1; } }

    public virtual float minHeight
    {
        get { return 0; }
    }

    public virtual float preferredHeight
    {
        get
        {
            return 0.0f;
            // var settings = GetGenerationSettings(new Vector2(rectTransform.rect.size.x, 0.0f));
            // return cachedTextGeneratorForLayout.GetPreferredHeight(m_Text, settings) / pixelsPerUnit;
        }
    }

    public virtual float flexibleHeight { get { return -1; } }

    public virtual int layoutPriority { get { return 0; } }

    /******/
    // 行頭の禁則文字か
    private bool IsHyphenationFront(char character)
    {
        return Array.Exists<char>(HyphenationFront, item => item == character);
    }

    // 行末の禁則文字か
    private bool IsHyphenationBack(char character)
    {
        return Array.Exists<char>(HyphenationBack, item => item == character);
    }

    // 数字判定
    private bool IsNumbers(char character)
    {
        return Array.Exists<char>(Numbers, item => item == character);
    }

    private bool Exists(char[] words, char character)
    {
        return Array.Exists<char>(words, item => item == character);
    }

    private bool IsWarp()
    {
        return horizontalLayout == Layout.Horizontal.Warp;
    }

    // 改行をするべき箇所か
    private bool IsNewLine(int width, int glyphWidth, float visibleWidth)
    {
        return IsWarp() && (width + glyphWidth) > visibleWidth;
    }

    private Vector2 Anchor()
    {
        return new Vector2(HorizontalAnchor(), VerticalAnchor());
    }

    private float HorizontalAnchor()
    {
        switch (horizontalAlignment)
        {
            case Alignment.Horizontal.Left:
                return 0.0f;

            case Alignment.Horizontal.Center:
                return 0.5f;

            case Alignment.Horizontal.Right:
                return 1.0f;

            default:
                Debug.Assert(false);
                break;
        }

        return 0.0f;
    }

    private float VerticalAnchor()
    {
        switch (verticalAlignment)
        {
            case Alignment.Vertical.Top:
                return 0.0f;

            case Alignment.Vertical.Middle:
                return 0.5f;

            case Alignment.Vertical.Bottom:
                return 1.0f;

            default:
                Debug.Assert(false);
                break;
        }

        return 0.0f;
    }

    private int GetMaxHeight()
    {
        if (fontSize == 0)
        {
            return font.lineHeight;
        }

        return (font.lineHeight * fontSize / font.fontSize);
    }

    protected override void OnEnable()
    {
        Font.textureRebuilt += TextureRebuildCallback;
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        Font.textureRebuilt -= TextureRebuildCallback;
        base.OnDisable();
    }

    private void TextureRebuildCallback(Font targetFont)
    {
        Debug.LogWarningFormat("call texture rebulid callback {0}", font.name);

        if (font == targetFont)
        {
            if (CanvasUpdateRegistry.IsRebuildingGraphics() || CanvasUpdateRegistry.IsRebuildingLayout())
            {
                UpdateGeometry();
            }
            else
            {
                SetAllDirty();
            }
        }
    }

    // デバッグ用
    // 実行時に呼び出さないでください
    //
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (!IsPlaying)
        {
            ResetTextRange();
        }

        base.OnValidate();
    }

    // 特定関数の処理時間の計算
    private void TaskTimeCalculate(Action task)
    {
        System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
        task();
        stopWatch.Stop();
        Debug.LogFormat("Method Task Timespan : {0}", stopWatch.ElapsedMilliseconds);
    }

    protected override void Reset()
    {
        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    // 現在有効なテキスト表示範囲処理の初期化を行う
    // シミュレーション実現のために追加
    public void ResetTextRange()
    {
        totalCharacterCount = text.Replace("\n", "").Length;
        UpdateDisplayMode(displayType);
    }
#endif

#if UNITY_EDITOR
    private bool SimurateMode = false;
#endif
    // Trueを返す時にTextRangeを継承しているIsUpdateとLenghtを呼び出す
    // 実行時には常にtrueを返す
    public bool IsPlaying
    {
        set
        {
#if UNITY_EDITOR
            SimurateMode = value;
#endif
        }

        get
        {
#if UNITY_EDITOR
            return (Application.isPlaying || SimurateMode);
#else
            return true;
#endif
        }
    }
}
}
