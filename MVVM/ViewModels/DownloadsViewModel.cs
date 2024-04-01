using Aniflex.MVVM.Models;
using Aniflex.Services;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace Aniflex.MVVM.ViewModels;

public sealed partial class DownloadsViewModel : INotifyPropertyChanged
{
    readonly IEnumerable<Anime> animes;
    readonly Dispatcher dispatcher;

    public ObservableCollection<DownloadViewModel> Downloads { get; set; } = new();

    public DownloadsViewModel(DownloadManager downloadManager, IEnumerable<Anime> animes)
    {
        this.animes = animes;
        dispatcher = Dispatcher.CurrentDispatcher;
        downloadManager.DownloadStarted += DownloadManager_DownloadStarted;
    }

    Anime? GetAnime(Magnet magnet)
    {
        foreach (var anime in animes)
        {
            foreach (var episode in anime.Episodes)
            {
                if (episode.Magnets.Contains(magnet))
                    return anime;
            }
        }
        return null;
    }

    Episode? GetEpisode(Magnet magnet)
    {
        foreach (var anime in animes)
        {
            foreach (var episode in anime.Episodes)
            {
                if(episode.Magnets.Contains(magnet))
                    return episode;
            }
        }
        return null;
    }

    private void DownloadManager_DownloadStarted(Download download)
    {
        var viewModel = new DownloadViewModel();
        viewModel.EpisodeId = GetEpisode(download.Magnet!)?.Id ?? "Unkown Episode Id";
        viewModel.AnimeName = GetAnime(download.Magnet!)?.Name ?? "Unkown Anime Name";
        viewModel.Image = GetAnime(download.Magnet!)?.Thumbnail!.Source;

        download.Progress!.OnProgress += progress =>
        {
            viewModel.Current = progress.Current;
            viewModel.Total = progress.Total;
        };

        dispatcher.BeginInvoke(() =>
        {
            Downloads.Add(viewModel);
        }, DispatcherPriority.Background);
    }
}
