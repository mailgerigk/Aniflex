using System;

namespace Aniflex.MVVM.Models;

public sealed class DownloadProgress : IProgress<ulong>
{
    public event Action<DownloadProgress>? OnProgress;

    public Download? Download { get; set; }
    public ulong Current { get; private set; }
    public ulong Total { get; set; }

    public void Report(ulong value)
    {
        Current = value;
        OnProgress?.Invoke(this);
    }
}
