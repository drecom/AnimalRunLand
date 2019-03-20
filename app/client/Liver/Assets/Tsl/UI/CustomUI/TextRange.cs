using System;
using UnityEngine;
using UnityEngine.Events;

namespace CustomUI
{
// UITextの文字表示の制御を行うクラスを定義
public interface TextRange
{
    /// <summary>
    /// リセット(Textが再設定された時に実行)
    /// </summary>
    void Reset();

    /// <summary>
    /// 表示する文字の更新が必要か判定をする
    /// </summary>
    /// <param name="totalCharacterCount">表示する予定の文字数</param>
    /// <returns>更新の必要有る/無し</returns>
    bool IsUpdate(int totalCharacterCount);

    /// <summary>
    /// 表示する文字カウントを返す
    /// </summary>
    /// <param name="totalCharacterCount">表示する予定の文字数</param>
    /// <returns>表示する文字数</returns>
    int Length(int totalCharacterCount);
}

[Serializable]
public class FlushText : TextRange
{
    public void Reset()
    {
    }

    public bool IsUpdate(int totalCharacterCount)
    {
        return false;
    }

    public int Length(int totalCharacterCount)
    {
        return totalCharacterCount;
    }
}

[Serializable]
public class TeletypeText : TextRange
{
    [SerializeField]
    private float speed = 1.0f / 40.0f; // 一文字進めるための速度

    [SerializeField]
    private UnityEvent onFinished;

    private float time = 0.0f;
    private int displayCharacterCount = 0;

    bool exit = false;

    public float Speed
    {
        set
        {
            speed = value;
        }

        get
        {
            return speed;
        }
    }

    public UnityEvent OnFinished
    {
        get
        {
            return onFinished;
        }
    }

    public void Reset()
    {
        time = 0.0f;
        displayCharacterCount = 0;
        exit = false;
    }

    public bool IsUpdate(int totalCharacterCount)
    {
        if (displayCharacterCount >= totalCharacterCount)
        {
            if (!exit)
            {
                exit = true;
                onFinished.Invoke();
            }

            return false;
        }

        time += Time.deltaTime;
        int currentDisplayCharacterCount = (int)(time / speed);

        if (currentDisplayCharacterCount >= displayCharacterCount)
        {
            displayCharacterCount = currentDisplayCharacterCount;
            return true;
        }

        return false;
    }

    public int Length(int totalCharacterCount)
    {
        return (displayCharacterCount > totalCharacterCount) ? totalCharacterCount : displayCharacterCount;
    }
}
}
