using Aniflex.Core;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Aniflex.MVVM.Models;
[DataContract]
public sealed partial class Anime : INotifyPropertyChanged
{
    [DataMember] public string? Name { get; set; }
    [DataMember] public string? Description { get; set; }
    [DataMember] public string? Uri { get; set; }
    [DataMember] public Thumbnail? Thumbnail { get; set; }
    [DataMember] public ObservableCollection<Batch> Batches { get; set; } = new();
    [DataMember] public ObservableCollection<Episode> Episodes { get; set; } = new();
    [DataMember] public DateTime LastEpisode { get; set; }
    [DataMember] public bool IsInSeason { get; set; }
    [DataMember] public bool IsFavorite { get; set; }
    [DataMember] public bool IsAutoDownload { get; set; }
    [DataMember] public VideoQuality AutoDownloadQuality { get; set; }

    public int WatchedEpisodes => Episodes.Where(episode => episode.IsWatched).Count();
    public int TotalEpisodes => Episodes.Count;
    public bool IsWatching => IsAutoDownload || (WatchedEpisodes > 0 && TotalEpisodes > WatchedEpisodes);
    public bool HasWatchedAll => WatchedEpisodes == TotalEpisodes;

    private void EnsureTwoEpisodesPreloaded()
    {
        if (!IsWatching)
            return;

        int firstWatched = int.MaxValue;
        int lastWatched = int.MinValue;
        int firstUnwatched = int.MaxValue;
        for (int i = 0; i < Episodes.Count; i++)
        {
            Episode episode = Episodes[i];
            firstWatched = episode.IsWatched ? Math.Min(firstWatched, i) : firstWatched;
            lastWatched = episode.IsWatched ? Math.Max(lastWatched, i) : lastWatched;
            firstUnwatched = episode.IsWatched ? firstUnwatched : Math.Min(firstUnwatched, i);
        }

        // not started to watch
        if (firstWatched == int.MaxValue)
            return;

        // not watching in order, dont know which to queue
        if (firstWatched < firstUnwatched && firstUnwatched < lastWatched)
            return;

        // no episode to be queued
        if (firstUnwatched == int.MinValue)
            return;

        for (int i = 0; i < 2; i++)
        {
            if (firstUnwatched + i < Episodes.Count)
            {
                Episodes[firstUnwatched + i].RequestBestQuality();
            }
        }
    }

    public bool UpdateFrom(Anime anime)
    {
        if (Name != anime.Name)
            return false;

        if (Description != anime.Description)
            Description = anime.Description;
        if (Uri != anime.Uri)
            Uri = anime.Uri;
        Thumbnail!.UpdateFrom(anime.Thumbnail!);
        anime.Batches.ForEach(batch =>
        {
            // TODO
        });
        anime.Episodes.ForEach(episode =>
        {
            if (!Episodes.Any(e => e.UpdateFrom(episode)))
            {
                Episodes.Add(episode);
            }
        });
        var orderedEpisodes = Episodes.OrderBy(episode => episode.Id).ToList();
        Episodes.Clear();
        orderedEpisodes.ForEach(episode => Episodes.Add(episode));
        foreach (Episode episode in Episodes)
        {
            episode.PropertyChanged += Episode_PropertyChanged;
        }
        if (LastEpisode != anime.LastEpisode)
            LastEpisode = anime.LastEpisode;
        if (IsInSeason != anime.IsInSeason)
            IsInSeason = anime.IsInSeason;
        EnsureTwoEpisodesPreloaded();
        return true;
    }

    private void Episode_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        EnsureTwoEpisodesPreloaded();
        if (e.PropertyName == "IsWatched")
        {
            OnPropertyChanged(nameof(WatchedEpisodes));
        }
    }
}
