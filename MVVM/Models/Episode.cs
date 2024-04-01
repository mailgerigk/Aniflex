using Aniflex.Core;

using PropertyChanged;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Aniflex.MVVM.Models;
[DataContract]
public sealed partial class Episode : INotifyPropertyChanged
{
    [DataMember] public string Id { get; set; } = string.Empty;
    [DataMember] public ObservableCollection<Magnet> Magnets { get; set; } = new();
    [DataMember] public DateTime ReleaseTime { get; set; }
    [OnChangedMethod(nameof(OnKeepAliveChanged))]
    [DataMember] public bool KeepAlive { get; set; }
    [OnChangedMethod(nameof(OnIsWatchedChanged))]
    [DataMember] public bool IsWatched { get; set; }

    public IEnumerable<VideoQuality> DownloadedVideoQualities => Magnets.Where(magnet => magnet.IsDownloaded).Select(magent => magent.Quality);
    public bool CanBeAutoDeleted => !KeepAlive && IsWatched;
    public bool IsDownloadingOrDownloaded => Magnets.Any(magnet => magnet.Download is not null || magnet.IsDownloaded);

    private void OnKeepAliveChanged(bool newValue, bool oldValue)
    {
        if (newValue == oldValue)
            return;

        Magnets.ForEach(magnet => magnet.KeepAlive = newValue);
    }

    private void OnIsWatchedChanged(bool newValue, bool oldValue)
    {
        if (newValue == oldValue)
            return;

        Magnets.ForEach(magnet => magnet.IsWatched = newValue);
    }

    public bool UpdateFrom(Episode episode)
    {
        if (Id != episode.Id)
            return false;

        foreach (Magnet magnet in episode.Magnets)
        {
            if (!Magnets.Any(m => m.UpdateFrom(magnet)))
            {
                Magnets.Add(magnet);
            }
        }
        var orderedMagnets = Magnets.OrderByDescending(magnet => magnet.Quality).ToList();
        Magnets.Clear();
        orderedMagnets.ForEach(magnet => Magnets.Add(magnet));
        Magnets.ForEach(magnet => magnet.KeepAlive = KeepAlive);
        Magnets.ForEach(magnet => magnet.IsWatched = IsWatched);

        return true;
    }

    public void FreeSpace()
    {
        if (CanBeAutoDeleted)
        {
            Magnets.ForEach(magnet => magnet.KeepAlive = KeepAlive);
            Magnets.ForEach(magnet => magnet.IsWatched = IsWatched);

            Magnets.ForEach(magnet => magnet.FreeSpace());
        }
    }

    public void RequestBestQuality()
    {
        if (Magnets.OrderByDescending(magnet => (int)magnet.Quality).FirstOrDefault() is not Magnet magnet)
            return;

        magnet.DownloadRequested = true;
    }
}
