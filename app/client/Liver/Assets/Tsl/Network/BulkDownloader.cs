using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/*
大量のファイルをダウンロードするためのクラス

WWW を並列に起動してダウンロードする。
並列起動数を変更するには BulkDownloader.DownloadConcurrency を変更すること。

また、ヒープ使用量が一定サイズを超えないように調節している。
このサイズは BulkDownloader.MaxHeapSize を変更すること。

以下のように利用する。

var bd = new BulkDownloader();
bd.Add(url1, size1, storePath1);
bd.Add(url2, size2, storePath2);
bd.Add(url3, size3, storePath3);
...

while (true)
{
    var future = bd.Start(mono);
    foreach (var v in future)
    {
        // 進捗を表示する
        var progress = (float)bd.DownloadedSize / (float)bd.MaxDownloadSize;
        progressBar.SetProgress(progress);

        yield return v;
    }

    if (future.HasException)
    {
        // エラー処理
        // ユーザにリトライするか確認し、再度やるならbd.Startからやり直し
        var retryFuture = ModalDialog.ShowRetry();
        foreach (var v in retryFuture) yield return v;
        if (retryFuture.Result)
        {
            // リトライする
            continue;
        }
        else
        {
            // 終了
            throw new Exception("エラー");
        }
    }
    else
    {
        break;
    }
}
*/
public class BulkDownloader
{
    public class DownloadInfo
    {
        public readonly string Url;
        public readonly int Size;
        public readonly string StorePath;

        public int DownloadedSize
        {
            get
            {
                switch (state)
                {
                    case DownloadInfo.State.Waiting:
                        return 0;

                    case DownloadInfo.State.Downloading:
                        return (int)(Size * www.progress);

                    case DownloadInfo.State.Error:
                        return 0;

                    case DownloadInfo.State.Completed:
                        return Size;
                }

                throw new Exception(string.Format("state {} not in State", state));
            }
        }

        internal enum State
        {
            Waiting,
            Downloading,
            Completed,
            Error,
        }
        internal State state;
        internal WWW www;
        internal Exception exception;

        // エラー状態をクリア
        internal void ClearError()
        {
            if (state == DownloadInfo.State.Error)
            {
                state = DownloadInfo.State.Waiting;
                exception = null;
            }
        }

        public DownloadInfo(string url, int size, string storePath)
        {
            this.Url = url;
            this.Size = size;
            this.StorePath = storePath;
        }

    }

    public static int DownloadConcurrency = 10;
    public static int MaxHeapSize = 5 * 1024 * 1024;
    public static int MaxRetry = 5;

    List<DownloadInfo> downloadInfos = new List<DownloadInfo>();

    public void Add(string url, int size, string storePath)
    {
        downloadInfos.Add(new DownloadInfo(url, size, storePath));
    }

    public int MaxDownloadSize
    {
        get { return downloadInfos.Sum(x => x.Size); }
    }

    public int DownloadedSize
    {
        get { return downloadInfos.Sum(x => x.DownloadedSize); }
    }

    class Worker
    {
        HashSet<DownloadInfo> downloadInfoSet;

        public Worker(HashSet<DownloadInfo> downloadInfoSet)
        {
            this.downloadInfoSet = downloadInfoSet;
        }

        public Util.Future<Util.Unit> Start()
        {
            return new Util.Future<Util.Unit>(promise => StartE(promise));
        }

        IEnumerator StartE(Util.Promise<Util.Unit> promise)
        {
            while (true)
            {
                // ダウンロード可能なデータが無ければ終了する
                if (downloadInfoSet.Count == 0)
                {
                    promise.Result = Util.Unit.Value;
                    yield break;
                }

                // エラーが発生しているなら終了する
                var errorInfo = downloadInfoSet.FirstOrDefault(x => x.state == DownloadInfo.State.Error);

                if (errorInfo != null)
                {
                    promise.Exception = errorInfo.exception;
                    yield break;
                }

                var downloadInfo = GetNext();

                if (downloadInfo == null)
                {
                    // ダウンロード可能なデータが見つからないので少し待ってリトライ
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                int retry = 0;

                while (true)
                {
                    Exception exception = null;
                    downloadInfo.state = DownloadInfo.State.Downloading;
                    downloadInfo.www = new WWW(downloadInfo.Url);

                    try
                    {
                        yield return downloadInfo.www;

                        if (downloadInfo.www.error != null)
                        {
                            exception = new Exception(downloadInfo.www.error);
                        }
                        else
                        {
                            try
                            {
                                using (var file = File.OpenWrite(downloadInfo.StorePath))
                                {
                                    file.Write(downloadInfo.www.bytes, 0, downloadInfo.www.bytes.Length);
                                }
                            }
                            catch (Exception)
                            {
                                try { File.Delete(downloadInfo.StorePath); }
                                catch (Exception) { }

                                exception = new Exception(string.Format("failed to write file: {0}, Url={1}", downloadInfo.StorePath, downloadInfo.Url));
                            }
                        }
                    }
                    finally
                    {
                        downloadInfo.www.Dispose();
                        downloadInfo.www = null;
                    }

                    if (exception != null)
                    {
                        if (retry < BulkDownloader.MaxRetry)
                        {
                            // 規定回数まではリトライし続ける
                            retry += 1;
                            continue;
                        }
                        else
                        {
                            downloadInfo.state = DownloadInfo.State.Error;
                            downloadInfo.exception = exception;
                        }
                    }
                    else
                    {
                        downloadInfo.state = DownloadInfo.State.Completed;
                    }

                    break;
                }

                // ダウンロードが完了したので downloadInfoSet から削除
                downloadInfoSet.Remove(downloadInfo);
            }
        }

        DownloadInfo GetNext()
        {
            // ダウンロード中データのサイズを調べる
            var downloading = downloadInfoSet.Where(x => x.state == DownloadInfo.State.Downloading).Sum(x => x.Size);

            // ダウンロード中データの合計サイズが MaxHeapSize 以上なら諦める
            // （メモリが足りなくなるので）
            if (downloading >= BulkDownloader.MaxHeapSize)
            {
                return null;
            }

            // ダウンロード中でないデータを探す
            return downloadInfoSet.FirstOrDefault(x => x.state == DownloadInfo.State.Waiting);
        }
    }

    public Util.Future<Util.Unit> Start(MonoBehaviour mono)
    {
        foreach (var info in downloadInfos)
        {
            info.ClearError();
        }

        var downloadInfoSet = new HashSet<DownloadInfo>(downloadInfos);
        var futures = Enumerable.Range(0, DownloadConcurrency).Select(_ => new Worker(downloadInfoSet)).Select(worker => worker.Start());

        foreach (var future in futures)
        {
            mono.StartCoroutine(future.GetEnumerator());
        }

        return futures.WhenAll().Then(_ =>
        {
            return Util.Unit.Value;
        });
    }
}
