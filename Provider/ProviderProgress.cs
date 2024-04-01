using Aniflex.MVVM.Models;

using System;
using System.ComponentModel;
using System.Threading;

namespace Aniflex.Provider;

public sealed partial class ProviderProgress : IProgress<Anime>, INotifyPropertyChanged
{
    public event Action<ProviderProgress, int, Anime>? OnProgress;

    public int Total { get; set; }

    private int current;
    public int Current => current;

    public void Report(Anime value)
    {
        int currentValue = Interlocked.Increment(ref current);
        OnProgress?.Invoke(this, currentValue, value);
    }
}
