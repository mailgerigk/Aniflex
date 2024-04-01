using Aniflex.Core;
using Aniflex.Diagnostic;
using Aniflex.MVVM.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace Aniflex.Services;

public sealed class DownloadManager
{
    public event Action<Download>? DownloadStarted;
    public event Action<Download>? DownloadCompleted;

    private const int MaxConcurrentDownloads = 5;
    private readonly HashSet<Magnet> queue = new();
    private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(MaxConcurrentDownloads, MaxConcurrentDownloads);

    public DownloadManager()
    {
        var thread = new Thread(obj =>
        {
            while (true)
            {
                autoResetEvent.WaitOne();
                HashSet<Magnet> queuedMagnets = new();
                lock (queue)
                {
                    foreach (Magnet magnet in queue)
                    {
                        queuedMagnets.Add(magnet);
                    }
                    queue.Clear();
                }

                foreach (Magnet magnet in queuedMagnets)
                {
                    semaphore.Wait();
                    PerformDownload(magnet);
                }
            }
        })
        {
            IsBackground = true
        };
        thread.Start();
    }

    private void PerformDownload(Magnet magnet)
    {
        var download = new Download
        {
            Magnet = magnet,
            Progress = new(),
        };
        download.Progress.Download = download;
        DownloadStarted?.Invoke(download);

        ThreadPool.QueueUserWorkItem(obj =>
        {
            DirectoryInfo tempDir = Directory.CreateTempSubdirectory();
            var startInfo = new ProcessStartInfo
            {
                FileName = "aria2c.exe",
                Arguments = $"--seed-time 0 \"{magnet.Uri}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WorkingDirectory = tempDir.FullName,
            };
            download.Process = Process.Start(startInfo);
            while (!download.Process!.HasExited)
            {
                string? line = download.Process.StandardOutput.ReadLine();
                if (string.IsNullOrEmpty(line))
                    continue;
                int firstSpace = line.IndexOf(" ");
                if (firstSpace < 0)
                    continue;
                int firstSlash = line.IndexOf("/");
                if (firstSlash < 0 || firstSlash < firstSpace)
                    continue;
                int firstParen = line.IndexOf("(");
                if (firstParen < 0 || firstParen < firstSlash)
                    continue;
                string current = line.Substring(firstSpace + 1, firstSlash - firstSpace - 1).Replace(",", ".");
                string currentToParse = new string( current.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
                if (!double.TryParse(currentToParse, CultureInfo.InvariantCulture, out double currentValue))
                    continue;
                if (current.EndsWith("KiB")) currentValue *= 1024;
                else if (current.EndsWith("MiB")) currentValue *= 1024 * 1024;
                else if (current.EndsWith("GiB")) currentValue *= 1024 * 1024 * 1024;
                string total = line.Substring(firstSlash + 1, firstParen - firstSlash - 1).Replace(",", "."); ;
                var totalToParse = new string( total.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
                if (!double.TryParse(totalToParse, CultureInfo.InvariantCulture, out double totalValue))
                    continue;
                if (total.EndsWith("KiB")) totalValue *= 1024;
                else if (total.EndsWith("MiB")) totalValue *= 1024 * 1024;
                else if (total.EndsWith("GiB")) totalValue *= 1024 * 1024 * 1024;
                download.Progress.Total = (ulong)totalValue;
                download.Progress.Report((ulong)currentValue);
            }

            string localBasePath = Path.Combine(".cache", "animes", tempDir.Name);
            foreach (string file in Directory.GetFiles(tempDir.FullName, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(tempDir.FullName, file);
                string path = Path.Combine(localBasePath, relativePath);
                EnsurePath.Exists(path);
                try
                {
                    File.Move(file, path);
                    magnet.LocalFiles.Add(path);
                }
                catch (Exception e)
                {
                    Log.Default.Exception(e);
                }
            }
            semaphore.Release();
            download.Progress.Report(download.Progress.Total);
            download.Magnet.CheckLocalFiles();
            download.Magnet.DownloadRequested = false;
        });
    }

    private void QueueDownloadIfNeeded(Magnet magnet)
    {
        if (!magnet.IsDownloaded && magnet.DownloadRequested && magnet.Download is null)
        {
            lock (queue)
            {
                queue.Add(magnet);
            }
            autoResetEvent.Set();
        }
    }

    public void SetMagnets(IEnumerable<Magnet> magnets)
    {
        foreach (Magnet magnet in magnets)
        {
            magnet.PropertyChanged += Magnet_PropertyChanged;
            QueueDownloadIfNeeded(magnet);
        }
    }

    private void Magnet_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is Magnet magnet)
        {
            QueueDownloadIfNeeded(magnet);
        }
    }
}
