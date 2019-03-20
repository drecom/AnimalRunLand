using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Util
{
/*
非同期処理のためのクラス。

IEnumerator は、そのまま利用すると戻り値やエラーを取れないので、いい感じに取れるようにするのが目的。

    Util.Future<int> GetValueAsync();

    IEnumerator Run() {
        var valueF = GetValueAsync();
        // GetValueAsync の処理が終わるのを待つ
        foreach (var v in valueF) yield return v;
        // 結果を取得する
        if (valueF.HasException)
            Debug.Log(valueF.Exception);
        else
            Debug.Log(valueF.Result);
    }

Then を使うことで、非同期呼び出しの結果を変換することができる。

    // intの結果を文字列に変換する
    Util.Future<string> GetStringAsync() {
        return GetValueAsync().Then(value => value.ToString());
    }

yield で待つためにはイテレータブロックを利用する必要がある。
イテレータブロックが使えない場合には、コールバックで受け取ることもできる。

    void Run() {
        var valueF = GetValueAsync();
        valueF.SetCallback(valueF_ => {
            if (valueF_.HasException)
                Debug.Log(valueF_.Exception);
            else
                Debug.Log(valueF_.Result);
        });
        // StartCoroutine で valueF のループを回す
        StartCoroutine(valueF.GetEnumerator());
    }

IEnumerator を返す関数を Util.Future に変換するには、Promise を受け取って結果を設定する必要がある。

    IEnumerator GetValueE(Util.Promise<int> promise) {
        // いろいろIO処理とかネットワーク処理を行う
        promise.Result = result;
    }

    void Run() {
        var valueF = new Util.Future<int>(promise => GetValueE(promise));
        ...
    }
*/

public class Future
{
    private static IEnumerator GetReadyValue<T>(Util.Promise<T> promise, T value)
    {
        promise.Result = value;
        yield break;
    }

    public static Future<T> MakeReady<T>(T value)
    {
        return new Util.Future<T>(promise => GetReadyValue(promise, value));
    }
}

public class Future<T>
{
    Promise<T> promise;
    IEnumerator enumerator;
    Action<Future<T>> callback;

    public Future(Func<Promise<T>, IEnumerator> func)
    {
        this.promise = new Promise<T>();
        this.enumerator = func(promise);
    }

    void InvokeCallback(Action<Future<T>> callback)
    {
        if (callback != null)
        {
            try
            {
                callback(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                this.promise.Exception = e;
            }
        }
    }
    public IEnumerator GetEnumerator()
    {
        while (true)
        {
            bool moveNext;

            try
            {
                moveNext = this.enumerator.MoveNext();
            }
            catch (Exception e)
            {
                this.promise.Exception = e;
                break;
            }

            if (!moveNext)
            {
                break;
            }

            yield return this.enumerator.Current;
        }

        this.promise.EndEnumerator();
        InvokeCallback(this.callback);
    }

    public T Result { get { return promise.Result; } }
    public bool HasException { get { return promise.HasException; } }
    public Exception Exception { get { return promise.Exception; } }

    public void ThrowIfException()
    {
        if (this.HasException)
        {
            throw this.Exception;
        }
    }

    IEnumerator ThenFunc<U>(Func<T, U> func, Promise<U> promise)
    {
        foreach (var v in this)
        {
            yield return v;
        }

        if (this.HasException)
        {
            promise.Exception = this.Exception;
        }
        else
        {
            try
            {
                promise.Result = func(this.Result);
            }
            catch (Exception e)
            {
                promise.Exception = e;
            }
        }
    }
    public Future<U> Then<U>(Func<T, U> func)
    {
        return new Future<U>(promise => ThenFunc(func, promise));
    }

    public void SetCallback(Action<Future<T>> callback)
    {
        if (this.promise.IsEndEnumerator())
        {
            InvokeCallback(callback);
        }
        else
        {
            this.callback = callback;
        }
    }
}

public class Promise<T>
{
    bool endEnumerator;
    bool setResult;
    T result;
    Exception exception;

    internal bool IsEndEnumerator()
    {
        return this.endEnumerator;
    }
    internal void EndEnumerator()
    {
        this.endEnumerator = true;
    }

    public T Result
    {

        internal get
        {
            if (HasException)
            {
                throw this.Exception;
            }

            return this.result;
        }
        set
        {
            setResult = true;
            this.result = value;
            this.exception = null;
        }
    }
    internal bool HasException
    {
        // 例外が明示的にセットされた、
        // 正常な結果がセットされていない、
        // enumeratorが最後まで実行されていない場合はエラーとして扱う
        get { return exception != null || !setResult || !endEnumerator; }
    }
    public Exception Exception
    {

        internal get
        {
            if (!HasException)
            {
                throw new Exception("Exception is not occured.");
            }

            if (exception != null)
            {
                return this.exception;
            }
            else if (!setResult)
            {
                return new Exception("result is not set");
            }
            else
            {
                return new Exception("do not complete enumeration");
            }
        }
        set
        {
            this.exception = value;
            this.result = default(T);
        }
    }
}
}

public static class FutureExtension
{
    static IEnumerator WhenAllE<T>(IEnumerable<Util.Future<T>> futures, Util.Promise<T[]> promise)
    {
        var futureArray = futures.ToArray();
        Exception exception = null;

        foreach (var future in futureArray)
        {
            foreach (var v in future) { yield return v; }

            if (future.HasException)
            {
                exception = future.Exception;
            }
        }

        if (exception != null)
        {
            promise.Exception = exception;
        }
        else
        {
            promise.Result = futureArray.Select(x => x.Result).ToArray();
        }
    }
    public static Util.Future<T[]> WhenAll<T>(this IEnumerable<Util.Future<T>> futures)
    {
        return new Util.Future<T[]>(promise => WhenAllE(futures, promise));
    }
}
